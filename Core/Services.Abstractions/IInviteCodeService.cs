namespace Services.Abstractions;

public interface IInviteCodeService
{
    /// <summary>
    /// Generate a new 6-character invite code for a company (created by recruiter)
    /// </summary>
    Task<string> GenerateAsync(int companyId, string createdByRecruiterId, int maxUses = 5, int? validDaysFromNow = 30);

    /// <summary>
    /// Get all active invite codes for a company (pagination ready)
    /// </summary>
    Task<IEnumerable<dynamic>> GetActiveCodesAsync(int companyId);

    /// <summary>
    /// Revoke/deactivate an invite code
    /// </summary>
    Task RevokeAsync(int codeId);

    /// <summary>
    /// Validate and consume an invite code. Returns CompanyId if valid, throws if invalid
    /// </summary>
    Task<int> ValidateAndUseAsync(string code);
}
