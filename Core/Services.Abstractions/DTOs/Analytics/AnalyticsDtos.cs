using System;
using System.Collections.Generic;

namespace Services.Abstractions.DTOs.Analytics
{
    public class CompanyAnalyticsDto
    {
        public int TotalApplicants { get; set; }
        public int ActiveJobs { get; set; }
        public double HireRate { get; set; }
        public IEnumerable<ApplicationTrendDto> ApplicationTrends { get; set; } = new List<ApplicationTrendDto>();
    }

    public class CandidateAnalyticsDto
    {
        public int ApplicationsSent { get; set; }
        public int SavedJobs { get; set; }
        public int UpcommingInterviews { get; set; }
    }

    public class ApplicationTrendDto
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
