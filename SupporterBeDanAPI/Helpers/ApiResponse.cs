namespace SupporterBeDanAPI.Helpers;

public static class ApiResponse
{
    public static object Success(object? data = null, string message = "Thành công")
    {
        return new
        {
            success = true,
            message,
            data
        };
    }

    public static object Error(string message, object? errors = null)
    {
        return new
        {
            success = false,
            message,
            errors
        };
    }

    public static object NotFound(string message = "Không tìm thấy dữ liệu")
    {
        return new
        {
            success = false,
            message,
            data = (object?)null
        };
    }

    public static object BadRequest(string message, object? errors = null)
    {
        return new
        {
            success = false,
            message,
            errors
        };
    }

    public static object Unauthorized(string message = "Chưa xác thực")
    {
        return new
        {
            success = false,
            message,
            data = (object?)null
        };
    }
}
