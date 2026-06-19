namespace PRN232.LMS.API.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = null
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = errors
    };

    public static ApiResponse<T> NotFound(string message = "Resource not found") => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = null
    };
}
