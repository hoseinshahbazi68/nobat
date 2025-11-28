import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DialogService } from '../../services/dialog.service';
import { DoctorScheduleService } from '../../services/doctor-schedule.service';
import { DoctorService } from '../../services/doctor.service';
import { ShiftService } from '../../services/shift.service';
import { ServiceService } from '../../services/service.service';
import { SnackbarService } from '../../services/snackbar.service';
import { PersianDateService } from '../../services/persian-date.service';
import { WeeklyDayScheduleDialogComponent, WeeklyDaySchedule, WeeklyDayScheduleDialogData } from './weekly-day-schedule-dialog.component';
import { DayOfWeek } from '../../models/doctor-schedule.model';
import { Shift } from '../../models/shift.model';
import { Service } from '../../models/service.model';
import { DoctorSchedule } from '../../models/doctor-schedule.model';

export interface WeeklyScheduleDay {
  dayIndex: number;
  dayName: string;
  schedule: WeeklyDaySchedule | null;
}

@Component({
  selector: 'app-weekly-schedule-view',
  templateUrl: './weekly-schedule-view.component.html',
  styleUrls: ['./weekly-schedule-view.component.scss']
})
export class WeeklyScheduleViewComponent implements OnInit {
  weekDays: WeeklyScheduleDay[] = [
    { dayIndex: 0, dayName: 'شنبه', schedule: null },
    { dayIndex: 1, dayName: 'یکشنبه', schedule: null },
    { dayIndex: 2, dayName: 'دوشنبه', schedule: null },
    { dayIndex: 3, dayName: 'سه‌شنبه', schedule: null },
    { dayIndex: 4, dayName: 'چهارشنبه', schedule: null },
    { dayIndex: 5, dayName: 'پنج‌شنبه', schedule: null },
    { dayIndex: 6, dayName: 'جمعه', schedule: null }
  ];

  selectedDoctorId: number | null = null;
  shifts: Shift[] = [];
  services: Service[] = [];
  isLoading = false;
  weekStartDate: Date = new Date();

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private route: ActivatedRoute,
    private router: Router,
    private doctorScheduleService: DoctorScheduleService,
    private shiftService: ShiftService,
    private serviceService: ServiceService,
    private persianDateService: PersianDateService
  ) {
    // محاسبه شروع هفته (شنبه)
    this.weekStartDate = this.getWeekStartDate();
  }

  ngOnInit() {
    this.loadShifts();
    this.loadServices();

    // خواندن doctorId از query params
    this.route.queryParams.subscribe(params => {
      if (params['doctorId']) {
        const doctorId = +params['doctorId'];
        this.selectedDoctorId = doctorId;
        this.loadWeeklySchedules();
      } else {
        this.snackbarService.error('شناسه پزشک در URL یافت نشد', 'بستن', 5000);
      }
    });
  }

  loadShifts() {
    this.shiftService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.shifts = result.items;
      },
      error: (error) => {
        console.error('Error loading shifts:', error);
        this.snackbarService.error('خطا در بارگذاری لیست شیفت‌ها', 'بستن', 5000);
      }
    });
  }

  loadServices() {
    this.serviceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.services = result.items;
      },
      error: (error) => {
        console.error('Error loading services:', error);
        this.snackbarService.error('خطا در بارگذاری لیست خدمات', 'بستن', 5000);
      }
    });
  }

  loadWeeklySchedules() {
    if (!this.selectedDoctorId) {
      // اگر پزشک انتخاب نشده، همه برنامه‌ها را پاک کن
      this.weekDays.forEach(day => day.schedule = null);
      return;
    }

    this.isLoading = true;
    const weekStartDateStr = this.formatDateForApi(this.weekStartDate);

    this.doctorScheduleService.getWeeklySchedule(this.selectedDoctorId, weekStartDateStr).subscribe({
      next: (result) => {
        // پاک کردن همه برنامه‌ها
        this.weekDays.forEach(day => day.schedule = null);

        // پر کردن برنامه‌های موجود بر اساس dayOfWeek
        if (result.schedules && result.schedules.length > 0) {
          result.schedules.forEach(schedule => {
            const dayIndex = schedule.dayOfWeek;
            if (dayIndex >= 0 && dayIndex < 7) {
              this.weekDays[dayIndex].schedule = {
                shiftId: schedule.shiftId,
                startTime: schedule.startTime,
                endTime: schedule.endTime,
                count: schedule.count || 1
              };
            }
          });
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading weekly schedules:', error);
        const errorMessage = error.error?.message || 'خطا در بارگذاری برنامه هفتگی';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  getWeekStartDate(): Date {
    const today = new Date();
    const day = today.getDay(); // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    // در تقویم شمسی، شنبه اولین روز هفته است (معادل Saturday = 6)
    // پس باید به Saturday برسیم
    const diff = day === 6 ? 0 : (day < 6 ? 6 - day : 7 - day + 6);
    const saturday = new Date(today);
    saturday.setDate(today.getDate() - diff);
    saturday.setHours(0, 0, 0, 0);
    return saturday;
  }

  getDayOfWeek(date: Date): number {
    const day = date.getDay();
    // تبدیل به روز هفته شمسی: Saturday=6 -> 0, Sunday=0 -> 1, Monday=1 -> 2, ...
    return day === 6 ? 0 : (day + 1) % 7;
  }

  formatDateForApi(date: Date): string {
    // فرمت YYYY-MM-DD برای API
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  getDateForDay(dayIndex: number): Date {
    const date = new Date(this.weekStartDate);
    date.setDate(this.weekStartDate.getDate() + dayIndex);
    return date;
  }

  openDaySchedule(day: WeeklyScheduleDay) {
    if (!this.selectedDoctorId) {
      this.snackbarService.error('لطفاً ابتدا پزشک را انتخاب کنید', 'بستن', 3000);
      return;
    }

    this.dialogService.open(WeeklyDayScheduleDialogComponent, {
      width: '750px',
      maxWidth: '95vw',
      data: {
        dayIndex: day.dayIndex,
        dayName: day.dayName,
        schedule: day.schedule,
        shifts: this.shifts,
        services: this.services
      } as WeeklyDayScheduleDialogData
    }).subscribe(result => {
      if (result !== undefined) {
        if (result === null) {
          // اگر null برگشت، یعنی باید حذف شود
          day.schedule = null;
          this.saveDaySchedule(day);
        } else if (result) {
          day.schedule = result;
          this.saveDaySchedule(day);
        }
      }
    });
  }

  saveDaySchedule(day: WeeklyScheduleDay) {
    if (!this.selectedDoctorId) return;

    if (day.schedule) {
      // ذخیره یا به‌روزرسانی برنامه
      const scheduleData = {
        doctorId: this.selectedDoctorId,
        dayOfWeek: day.dayIndex as DayOfWeek,
        shiftId: day.schedule.shiftId || 0,
        serviceId: day.schedule.serviceId || 0,
        startTime: day.schedule.startTime || '08:00',
        endTime: day.schedule.endTime || '12:00',
        count: day.schedule.count || 1
      };

      // بررسی اینکه آیا برنامه قبلی برای این روز و شیفت وجود دارد
      this.doctorScheduleService.getAll({
        doctorId: this.selectedDoctorId,
        page: 1,
        pageSize: 100
      }).subscribe({
        next: (result: any) => {
          // پیدا کردن برنامه موجود برای این روز و شیفت
          const existingSchedule = result.items?.find((s: DoctorSchedule) =>
            s.dayOfWeek === scheduleData.dayOfWeek &&
            s.shiftId === scheduleData.shiftId
          );

          if (existingSchedule && existingSchedule.id) {
            // به‌روزرسانی برنامه موجود
            this.doctorScheduleService.update({
              id: existingSchedule.id,
              ...scheduleData
            }).subscribe({
              next: () => {
                this.snackbarService.success(`برنامه ${day.dayName} با موفقیت به‌روزرسانی شد`, 'بستن', 3000);
                this.loadWeeklySchedules();
              },
              error: (error: any) => {
                const errorMessage = error.error?.message || 'خطا در به‌روزرسانی برنامه';
                this.snackbarService.error(errorMessage, 'بستن', 5000);
              }
            });
          } else {
            // ایجاد برنامه جدید
            this.doctorScheduleService.create(scheduleData).subscribe({
              next: () => {
                this.snackbarService.success(`برنامه ${day.dayName} با موفقیت ذخیره شد`, 'بستن', 3000);
                this.loadWeeklySchedules();
              },
              error: (error: any) => {
                const errorMessage = error.error?.message || 'خطا در ذخیره برنامه';
                this.snackbarService.error(errorMessage, 'بستن', 5000);
              }
            });
          }
        },
        error: (error: any) => {
          // در صورت خطا، سعی می‌کنیم برنامه جدید ایجاد کنیم
          this.doctorScheduleService.create(scheduleData).subscribe({
            next: () => {
              this.snackbarService.success(`برنامه ${day.dayName} با موفقیت ذخیره شد`, 'بستن', 3000);
              this.loadWeeklySchedules();
            },
            error: (err: any) => {
              const errorMessage = err.error?.message || 'خطا در ذخیره برنامه';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    } else {
      // حذف برنامه
      // با حذف فیلد Date، نمی‌توان برنامه خاصی را بر اساس روز هفته حذف کرد
      // در این حالت، برنامه‌های مربوط به این روز از لیست حذف می‌شوند
      this.snackbarService.info('برای حذف برنامه، لطفاً از صفحه اصلی برنامه حضور استفاده کنید', 'بستن', 5000);
    }
  }

  getDayScheduleInfo(day: WeeklyScheduleDay): string {
    if (!day.schedule) {
      return 'بدون برنامه';
    }

    const shift = this.shifts.find(s => s.id === day.schedule?.shiftId);
    const parts: string[] = [];

    if (shift) {
      parts.push(`شیفت: ${shift.name}`);
    }

    if (day.schedule.startTime && day.schedule.endTime) {
      parts.push(`${day.schedule.startTime} - ${day.schedule.endTime}`);
    }

    if (day.schedule.count && day.schedule.count > 0) {
      parts.push(`تعداد: ${day.schedule.count}`);
    }

    return parts.length > 0 ? parts.join(' | ') : 'برنامه تنظیم شده';
  }

  getShiftName(shiftId?: number): string | null {
    if (!shiftId) return null;
    const shift = this.shifts.find(s => s.id === shiftId);
    return shift?.name || null;
  }
}
