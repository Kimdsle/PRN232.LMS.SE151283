namespace PRN232.LMS.API.Common;

public class PagedApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new();
    public List<string>? Errors { get; set; }
    public PaginationMetadata Pagination { get; set; } = new();

    public static PagedApiResponse<T> Ok(
        List<T> data,
        int page,
        int pageSize,
        int totalItems,
        string message = "Request processed successfully") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = null,
        Pagination = new PaginationMetadata
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        }
    };

    public static PagedApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = new List<T>(),
        Errors = errors,
        Pagination = new PaginationMetadata()
    };
}
