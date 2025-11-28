namespace Nobat.Application.Common;

/// <summary>
/// کلاس پایه برای پاسخ‌های API
/// </summary>
/// <typeparam name="T">نوع داده برگشتی</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// وضعیت پاسخ (true برای موفق، false برای خطا)
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// کد وضعیت HTTP
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// پیام توضیحی فارسی برای کاربر
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات اضافی (اختیاری)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// داده برگشتی
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// خطاها (در صورت وجود)
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// ایجاد پاسخ موفق
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "عملیات با موفقیت انجام شد", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Status = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// ایجاد پاسخ موفق بدون داده
    /// </summary>
    public static ApiResponse<T> Success(string message = "عملیات با موفقیت انجام شد", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Status = true,
            StatusCode = statusCode,
            Message = message
        };
    }

    /// <summary>
    /// ایجاد پاسخ خطا
    /// </summary>
    public static ApiResponse<T> Error(string message, int statusCode = 400, string? description = null, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Status = false,
            StatusCode = statusCode,
            Message = message,
            Description = description,
            Errors = errors
        };
    }

    /// <summary>
    /// ایجاد پاسخ خطا با استثنا
    /// </summary>
    public static ApiResponse<T> Error(string message, Exception exception, int statusCode = 500)
    {
        return new ApiResponse<T>
        {
            Status = false,
            StatusCode = statusCode,
            Message = message,
            Description = exception.Message,
            Errors = new List<string> { exception.ToString() }
        };
    }
}

/// <summary>
/// کلاس پاسخ API بدون نوع (برای پاسخ‌های بدون داده)
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// ایجاد پاسخ موفق
    /// </summary>
    public static new ApiResponse Success(string message = "عملیات با موفقیت انجام شد", int statusCode = 200)
    {
        return new ApiResponse
        {
            Status = true,
            StatusCode = statusCode,
            Message = message
        };
    }

    /// <summary>
    /// ایجاد پاسخ خطا
    /// </summary>
    public static new ApiResponse Error(string message, int statusCode = 400, string? description = null, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Status = false,
            StatusCode = statusCode,
            Message = message,
            Description = description,
            Errors = errors
        };
    }

    /// <summary>
    /// ایجاد پاسخ خطا با استثنا
    /// </summary>
    public static new ApiResponse Error(string message, Exception exception, int statusCode = 500)
    {
        return new ApiResponse
        {
            Status = false,
            StatusCode = statusCode,
            Message = message,
            Description = exception.Message,
            Errors = new List<string> { exception.ToString() }
        };
    }
}
