using Domain.Contracts;
using Services.Abstractions;
using Services.Abstractions.DTOs.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CompanyAnalyticsDto> GetCompanyAnalyticsAsync(int companyId)
        {
            // 1. Active Jobs
            var activeJobs = await _unitOfWork.Analytics.GetActiveJobsCountAsync(companyId);

            // 2. Total Applicants
            var totalApplicants = await _unitOfWork.Analytics.GetTotalApplicantsAsync(companyId);

            // 3. Hire Rate (Total Hired / Total Applied)
            var hiredCount = await _unitOfWork.Analytics.GetHiredCountAsync(companyId);

            double hireRate = totalApplicants > 0 ? Math.Round((double)hiredCount / totalApplicants * 100, 2) : 0;

            // 4. Trends (Simplified to last 7 days for demo)
            var rawData = await _unitOfWork.Analytics.GetApplicationTrendsAsync(companyId, 7);
            var trendsData = rawData.Select(k => new ApplicationTrendDto { Date = k.Key, Count = k.Value });

            return new CompanyAnalyticsDto
            {
                ActiveJobs = activeJobs,
                TotalApplicants = totalApplicants,
                HireRate = hireRate,
                ApplicationTrends = trendsData
            };
        }

        public async Task<CandidateAnalyticsDto> GetCandidateAnalyticsAsync(string candidateId)
        {
            var applications = await _unitOfWork.Analytics.GetCandidateApplicationCountAsync(candidateId);
            var savedJobs = await _unitOfWork.Analytics.GetCandidateSavedJobsCountAsync(candidateId);
            var interviews = await _unitOfWork.Analytics.GetCandidateUpcomingInterviewsCountAsync(candidateId);

            return new CandidateAnalyticsDto
            {
                ApplicationsSent = applications,
                SavedJobs = savedJobs,
                UpcommingInterviews = interviews
            };
        }
    }
}
