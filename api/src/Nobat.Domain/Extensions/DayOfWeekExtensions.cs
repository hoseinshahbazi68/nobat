using Nobat.Domain.Enums;

namespace Nobat.Domain.Extensions;

/// <summary>
/// Extension methods برای DayOfWeek enum
/// </summary>
public static class DayOfWeekExtensions
{
    /// <summary>
    /// تبدیل enum DayOfWeek به نام فارسی
    /// </summary>
    /// <param name="dayOfWeek">روز هفته</param>
    /// <returns>نام فارسی روز</returns>
    public static string ToPersianName(this DayOfWeekModel dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeekModel.Saturday => "شنبه",
            DayOfWeekModel.Sunday => "یکشنبه",
            DayOfWeekModel.Monday => "دوشنبه",
            DayOfWeekModel.Tuesday => "سه‌شنبه",
            DayOfWeekModel.Wednesday => "چهارشنبه",
            DayOfWeekModel.Thursday => "پنج‌شنبه",
            DayOfWeekModel.Friday => "جمعه",
            _ => "نامشخص"
        };
    }
}
