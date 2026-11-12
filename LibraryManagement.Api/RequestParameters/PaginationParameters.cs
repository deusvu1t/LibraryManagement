namespace LibraryManagement.Api.RequestParameters;

public class PaginationParameters
{
    private const int MinimalPageNumber = 1;
    private const int DefaultPageNumber = 1;
    private const int MinimalPageSize = 1;
    private const int MaximalPageSize = 50;
    private const int DefaultPageSize = 10;
    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < MinimalPageNumber ? MinimalPageNumber : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, MinimalPageSize, MaximalPageSize);
    }
}
