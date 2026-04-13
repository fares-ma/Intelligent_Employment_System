using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace IES.api.Authentication;

/// <summary>
/// Authenticates every request without a JWT so [Authorize] endpoints return 200.
/// Use only when <c>AllowAnonymousApi</c> is true (e.g. local demo). Not for production.
/// </summary>
public sealed class AnonymousApiAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "AnonymousApi";

    private readonly AnonymousApiAuthOptions _anonymousOptions;

    public AnonymousApiAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<AnonymousApiAuthOptions> anonymousOptions)
        : base(options, logger, encoder)
    {
        _anonymousOptions = anonymousOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _anonymousOptions.UserId),
            new(ClaimTypes.Name, "AnonymousApi"),
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Role, "Recruiter"),
            new(ClaimTypes.Role, "Candidate"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
