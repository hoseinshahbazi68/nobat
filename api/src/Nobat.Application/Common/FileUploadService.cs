using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Nobat.Application.Common;

/// <summary>
/// سرویس آپلود فایل
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;
    private const string ProfilePicturesFolder = "uploads/profile-pictures";
    private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// آپلود فایل عکس پروفایل
    /// </summary>
    public async Task<string?> UploadProfilePictureAsync(IFormFile? file, int userId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        try
        {
            // بررسی اندازه فایل
            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("File size exceeds maximum allowed size. File size: {FileSize}, Max: {MaxSize}", file.Length, MaxFileSize);
                throw new InvalidOperationException($"حجم فایل نباید بیشتر از {MaxFileSize / (1024 * 1024)} مگابایت باشد");
            }

            // بررسی پسوند فایل
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Invalid file extension: {Extension}", fileExtension);
                throw new InvalidOperationException($"فقط فایل‌های تصویری با پسوندهای {string.Join(", ", AllowedExtensions)} مجاز هستند");
            }

            // ایجاد پوشه در صورت عدم وجود
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, ProfilePicturesFolder);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                _logger.LogInformation("Created directory: {Directory}", uploadsPath);
            }

            // تولید نام فایل منحصر به فرد
            var fileName = $"profile_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // ذخیره فایل
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("Profile picture uploaded successfully. UserId: {UserId}, FileName: {FileName}", userId, fileName);

            // برگرداندن مسیر نسبی
            return $"/{ProfilePicturesFolder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// حذف فایل عکس پروفایل
    /// </summary>
    public async Task DeleteFileAsync(string? filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            // تبدیل مسیر نسبی به مسیر کامل
            var fullPath = filePath.StartsWith("/")
                ? Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, filePath.TrimStart('/'))
                : filePath;

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            // خطا را throw نمی‌کنیم چون حذف فایل عملیات اختیاری است
        }
    }
}
