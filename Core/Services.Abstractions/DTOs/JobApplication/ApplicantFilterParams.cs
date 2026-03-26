using Shared.Pagination;

namespace Services.Abstractions.DTOs.JobApplication;

public class ApplicantFilterParams : PaginationParams
{
    public int? Status { get; set; }
    public decimal? MinScore { get; set; }
    public string? SortBy { get; set; } = "matchScore";
    public string? SortOrder { get; set; } = "desc";
}
