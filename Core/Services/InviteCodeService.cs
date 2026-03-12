using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

public class InviteCodeService : IInviteCodeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyInviteCodeRepository _inviteCodeRepository;
    private readonly ILogger<InviteCodeService> _logger;

    private static readonly Random _random = new();

    public InviteCodeService(
        IUnitOfWork unitOfWork,
        ICompanyInviteCodeRepository inviteCodeRepository,
        ILogger<InviteCodeService> logger)
    {
        _unitOfWork = unitOfWork;
        _inviteCodeRepository = inviteCodeRepository;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(int companyId, string createdByRecruiterId, int maxUses = 5, int? validDaysFromNow = 30)
    {
        // Verify company exists
        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company == null)
            throw new NotFoundException("Company", companyId.ToString());

        // Verify recruiter exists and belongs to company
        var recruiter = await _unitOfWork.Recruiters.GetWithCompanyAsync(createdByRecruiterId);
        if (recruiter == null || recruiter.CompanyId != companyId)
            throw new ForbiddenException("Recruiter must belong to the company");

        // Generate unique 6-char code (retry up to 10 times)
        string code = GenerateUniqueCode();
        int attempts = 0;
        while (attempts < 10 && await CodeExistsAsync(code))
        {
            code = GenerateUniqueCode();
            attempts++;
        }

        if (attempts >= 10)
            throw new BadRequestException("Failed to generate unique invite code. Please try again.");

        var inviteCode = new CompanyInviteCode
        {
            CompanyId = companyId,
            Code = code,
            MaxUses = maxUses,
            CurrentUses = 0,
            ExpiresAt = validDaysFromNow.HasValue ? DateTime.UtcNow.AddDays(validDaysFromNow.Value) : null,
            CreatedByRecruiterId = createdByRecruiterId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _inviteCodeRepository.Create(inviteCode);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Invite code generated for company {CompanyId}: {Code}", companyId, code);

        return code;
    }

    public async Task<IEnumerable<dynamic>> GetActiveCodesAsync(int companyId)
    {
        var codes = await _inviteCodeRepository.GetActiveByCompanyAsync(companyId);
        
        return codes.Select(c => new
        {
            c.Id,
            c.Code,
            c.MaxUses,
            c.CurrentUses,
            RemainingUses = c.MaxUses - c.CurrentUses,
            c.ExpiresAt,
            c.CreatedAt,
            c.IsActive
        });
    }

    public async Task RevokeAsync(int codeId)
    {
        var code = await _inviteCodeRepository.GetByIdAsync(codeId);
        if (code == null)
            throw new NotFoundException("Invite code", codeId.ToString());

        code.IsActive = false;
        _inviteCodeRepository.Update(code);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Invite code {CodeId} revoked", codeId);
    }

    public async Task<int> ValidateAndUseAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BadRequestException("Invite code is required");

        var inviteCode = await _inviteCodeRepository.GetByCodeAsync(code.Trim());
        if (inviteCode == null)
            throw new BadRequestException("Invalid invite code");

        if (!inviteCode.IsActive)
            throw new BadRequestException("Invite code has been revoked");

        if (inviteCode.ExpiresAt.HasValue && inviteCode.ExpiresAt < DateTime.UtcNow)
            throw new BadRequestException("Invite code has expired");

        if (inviteCode.CurrentUses >= inviteCode.MaxUses)
            throw new BadRequestException("Invite code has reached its usage limit");

        // Consume the code
        inviteCode.CurrentUses++;
        _inviteCodeRepository.Update(inviteCode);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Invite code {Code} used. Remaining uses: {Remaining}/{Max}",
            code, inviteCode.MaxUses - inviteCode.CurrentUses, inviteCode.MaxUses);

        return inviteCode.CompanyId;
    }

    private static string GenerateUniqueCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];
        
        lock (_random)
        {
            for (int i = 0; i < 6; i++)
                code[i] = chars[_random.Next(chars.Length)];
        }

        return new string(code);
    }

    private async Task<bool> CodeExistsAsync(string code)
    {
        var existing = await _inviteCodeRepository.GetByCodeAsync(code);
        return existing != null;
    }
}
