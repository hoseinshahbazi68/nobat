export enum DayOfWeek {
  Saturday = 0,
  Sunday = 1,
  Monday = 2,
  Tuesday = 3,
  Wednesday = 4,
  Thursday = 5,
  Friday = 6
}

export namespace DayOfWeek {
  export function getPersianName(day: DayOfWeek): string {
    const persianNames: Record<DayOfWeek, string> = {
      [DayOfWeek.Saturday]: 'شنبه',
      [DayOfWeek.Sunday]: 'یکشنبه',
      [DayOfWeek.Monday]: 'دوشنبه',
      [DayOfWeek.Tuesday]: 'سه‌شنبه',
      [DayOfWeek.Wednesday]: 'چهارشنبه',
      [DayOfWeek.Thursday]: 'پنج‌شنبه',
      [DayOfWeek.Friday]: 'جمعه'
    };
    return persianNames[day] || '';
  }
}

export interface DoctorSchedule {
  id?: number;
  doctorId: number;
  doctorName?: string;
  shiftId: number;
  shiftName?: string;
  dayOfWeek: DayOfWeek;
  dayOfWeekName?: string;
  startTime: string;
  endTime: string;
  count: number;
  serviceId: number;
  serviceName?: string;
  clinicId?: number;
}

export interface CreateDoctorScheduleRequest {
  doctorId: number;
  shiftId: number;
  dayOfWeek: DayOfWeek;
  startTime: string;
  endTime: string;
  count: number;
  serviceId: number;
  clinicId?: number;
}

export interface UpdateDoctorScheduleRequest {
  id: number;
  doctorId: number;
  shiftId: number;
  dayOfWeek: DayOfWeek;
  startTime: string;
  endTime: string;
  count: number;
  serviceId: number;
  clinicId?: number;
}

export interface WeeklySchedule {
  doctorId: number;
  doctorName?: string;
  schedules: DoctorSchedule[];
}
