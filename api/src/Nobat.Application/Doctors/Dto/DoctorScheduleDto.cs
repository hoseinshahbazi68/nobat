using Nobat.Application.Clinics.Dto;
using Nobat.Domain.Enums;

namespace Nobat.Application.Doctors.Dto;

/// <summary>
/// DTO برنامه پزشک
/// </summary>
public class DoctorScheduleDto
{
    /// <summary>
    /// شناسه برنامه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// نام پزشک
    /// </summary>
    public string? DoctorName { get; set; }

    /// <summary>
    /// شناسه شیفت
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// نام شیفت
    /// </summary>
    public string? ShiftName { get; set; }

    /// <summary>
    /// روز هفته
    /// </summary>
    public DayOfWeekModel DayOfWeek { get; set; }

    /// <summary>
    /// نام فارسی روز هفته
    /// </summary>
    public string DayOfWeekName { get; set; } = string.Empty;

    /// <summary>
    /// زمان شروع
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// تعداد
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// شناسه کلینیک (اختیاری - اگر null باشد، مطب شخصی پزشک است)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// اطلاعات کلینیک (اختیاری)
    /// </summary>
    public ClinicDto? Clinic { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// نام خدمت
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد برنامه پزشک
/// </summary>
public class CreateDoctorScheduleDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه شیفت
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// روز هفته
    /// </summary>
    public DayOfWeekModel DayOfWeek { get; set; }

    /// <summary>
    /// زمان شروع
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// تعداد
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// شناسه کلینیک (اختیاری - اگر null باشد، مطب شخصی پزشک است)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی برنامه پزشک
/// </summary>
public class UpdateDoctorScheduleDto
{
    /// <summary>
    /// شناسه برنامه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه شیفت
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// روز هفته
    /// </summary>
    public DayOfWeekModel DayOfWeek { get; set; }

    /// <summary>
    /// زمان شروع
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// تعداد
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// شناسه کلینیک (اختیاری - اگر null باشد، مطب شخصی پزشک است)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }
}

/// <summary>
/// DTO برنامه هفتگی پزشک
/// </summary>
public class WeeklyScheduleDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// نام پزشک
    /// </summary>
    public string? DoctorName { get; set; }

    /// <summary>
    /// لیست برنامه‌ها
    /// </summary>
    public List<DoctorScheduleDto> Schedules { get; set; } = new();
}

/// <summary>
/// DTO تولید برنامه پزشک
/// </summary>
public class GenerateScheduleDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// تاریخ شروع
    /// </summary>
    public string StartDate { get; set; } = string.Empty;

    /// <summary>
    /// تاریخ پایان
    /// </summary>
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// لیست شناسه شیفت‌ها
    /// </summary>
    public List<int> ShiftIds { get; set; } = new();

    /// <summary>
    /// شناسه کلینیک (اختیاری - اگر null باشد، مطب شخصی پزشک است)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }
}
