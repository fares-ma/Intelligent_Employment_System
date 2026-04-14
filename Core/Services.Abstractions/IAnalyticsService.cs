using Services.Abstractions.DTOs.Analytics;
using System.Threading.Tasks;

namespace Services.Abstractions
{
    public interface IAnalyticsService
    {
        Task<CompanyAnalyticsDto> GetCompanyAnalyticsAsync(int companyId);
        Task<CandidateAnalyticsDto> GetCandidateAnalyticsAsync(string candidateId);
    }
}
