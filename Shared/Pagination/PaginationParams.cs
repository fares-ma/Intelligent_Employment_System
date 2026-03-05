namespace Shared.Pagination;

/// <summary>
/// Standard pagination parameters for list queries.
/// </summary>
public class PaginationParams
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Page number (1-indexed). Default: 1.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value > 0 ? value : 1;
    }

    /// <summary>
    /// Number of items per page. Default: 10. Max: 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => value
        };
    }
}
