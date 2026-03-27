using Services.Abstractions.DTOs.Dashboard;

namespace Services.Abstractions;

public interface IDashboardService
{
    Task<CandidateDashboardDto> GetCandidateDashboardAsync(string candidateId);
    Task<CompanyDashboardDto> GetCompanyDashboardAsync(int companyId);
}
