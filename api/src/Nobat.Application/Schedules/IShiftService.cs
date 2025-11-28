using Nobat.Application.Schedules.Dto;
using Sieve.Models;

namespace Nobat.Application.Schedules;

/// <summary>
/// رابط سرویس شیفت
/// </summary>
public interface IShiftService
{
    /// <summary>
    /// دریافت شیفت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت یافت شده یا null</returns>
    Task<ShiftDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست صفحه‌بندی شده شیفت‌ها
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    Task<PagedResult<Nobat.Application.Schedules.Dto.ShiftDto>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد شیفت جدید
    /// </summary>
    /// <param name="dto">اطلاعات شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت ایجاد شده</returns>
    Task<ShiftDto> CreateAsync(CreateShiftDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی شیفت
    /// </summary>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت به‌روز شده</returns>
    Task<ShiftDto> UpdateAsync(UpdateShiftDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف شیفت
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

 
