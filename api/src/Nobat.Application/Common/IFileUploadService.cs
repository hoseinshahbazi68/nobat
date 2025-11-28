using Microsoft.AspNetCore.Http;

namespace Nobat.Application.Common;

/// <summary>
/// رابط سرویس آپلود فایل
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// آپلود فایل عکس پروفایل
    /// </summary>
    /// <param name="file">فایل آپلود شده</param>
    /// <param name="userId">شناسه کاربر (برای نام فایل)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مسیر نسبی فایل ذخیره شده</returns>
    Task<string?> UploadProfilePictureAsync(IFormFile? file, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف فایل عکس پروفایل
    /// </summary>
    /// <param name="filePath">مسیر فایل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task DeleteFileAsync(string? filePath, CancellationToken cancellationToken = default);
}
