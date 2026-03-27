using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Services.Abstractions;
using Services.Abstractions.DTOs.Auth;
using Services;

namespace IES.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IInviteCodeService> _inviteCodeMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inviteCodeMock = new Mock<IInviteCodeService>();
        _configMock = new Mock<IConfiguration>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Setup config for JWT tokens
        var jwtSettingsSection = new Mock<IConfigurationSection>();
        jwtSettingsSection.Setup(x => x["Secret"]).Returns("SuperSecretKeyThatIsVeryLongForHS256");
        jwtSettingsSection.Setup(x => x["ExpiryInHours"]).Returns("24");
        jwtSettingsSection.Setup(x => x["Issuer"]).Returns("IES");
        jwtSettingsSection.Setup(x => x["Audience"]).Returns("IES");
        _configMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsSection.Object);

        _authService = new AuthService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _unitOfWorkMock.Object,
            _inviteCodeMock.Object,
            _configMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginRequestDto { Email = "test@test.com", Password = "Password1!" };
        var user = new CandidateUser { Id = "1", Email = "test@test.com", IsActive = true };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Candidate" });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Candidate", result.Role);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_AsCandidate_CreatesUserAndReturnsToken()
    {
        // Arrange
        var request = new RegisterRequestDto 
        { 
            Email = "new@test.com", 
            Password = "Password1!", 
            UserType = "Candidate",
            Gender = "Male",
            FirstName = "Test",
            LastName = "User"
        };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Candidate")).ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("Candidate", result.Role);
        Assert.NotNull(result.Token);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
}
