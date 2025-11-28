using Nobat.Application.Schedules.Dto;
using Sieve.Models;

namespace Nobat.Application.Schedules;

/// <summary>
/// رابط سرویس روز تعطیل
/// </summary>
public interface IHolidayService
{
    /// <summary>
    /// دریافت روز تعطیل بر اساس شناسه
    /// </summary>
    Task<HolidayDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست روزهای تعطیل با فیلتر و مرتب‌سازی
    /// </summary>
    Task<PagedResult<HolidayDto>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد روز تعطیل جدید
    /// </summary>
    Task<HolidayDto> CreateAsync(CreateHolidayDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی روز تعطیل
    /// </summary>
    Task<HolidayDto> UpdateAsync(UpdateHolidayDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف روز تعطیل
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
