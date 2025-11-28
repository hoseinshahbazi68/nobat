import { DayOfWeek } from '../models/doctor-schedule.model';

/**
 * تبدیل enum DayOfWeek به نام فارسی
 */
export function getDayOfWeekPersianName(dayOfWeek: DayOfWeek): string {
  const dayNames: { [key in DayOfWeek]: string } = {
    [DayOfWeek.Saturday]: 'شنبه',
    [DayOfWeek.Sunday]: 'یکشنبه',
    [DayOfWeek.Monday]: 'دوشنبه',
    [DayOfWeek.Tuesday]: 'سه‌شنبه',
    [DayOfWeek.Wednesday]: 'چهارشنبه',
    [DayOfWeek.Thursday]: 'پنج‌شنبه',
    [DayOfWeek.Friday]: 'جمعه'
  };

  return dayNames[dayOfWeek] || 'نامشخص';
}

/**
 * دریافت لیست تمام روزهای هفته با نام فارسی
 */
export function getAllDaysOfWeek(): Array<{ value: DayOfWeek; name: string }> {
  return [
    { value: DayOfWeek.Saturday, name: 'شنبه' },
    { value: DayOfWeek.Sunday, name: 'یکشنبه' },
    { value: DayOfWeek.Monday, name: 'دوشنبه' },
    { value: DayOfWeek.Tuesday, name: 'سه‌شنبه' },
    { value: DayOfWeek.Wednesday, name: 'چهارشنبه' },
    { value: DayOfWeek.Thursday, name: 'پنج‌شنبه' },
    { value: DayOfWeek.Friday, name: 'جمعه' }
  ];
}
