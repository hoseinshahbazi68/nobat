import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { PersianDateService } from '../../services/persian-date.service';
import { DoctorScheduleService } from '../../services/doctor-schedule.service';
import { HolidayService } from '../../services/holiday.service';
import { AppointmentService, AppointmentCountDto } from '../../services/appointment.service';
import { DoctorSchedule, DayOfWeek } from '../../models/doctor-schedule.model';
import { Holiday } from '../../models/holiday.model';

export interface CalendarDay {
  day: number;
  month: number;
  year: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isHoliday: boolean;
  isFriday: boolean;
  date: Date;
  schedules?: DoctorSchedule[];
  totalSlots?: number;
  bookedSlots?: number;
  availableSlots?: number;
}

@Component({
  selector: 'app-persian-calendar',
  template: `
    <div class="persian-calendar">
      <div class="calendar-header">
        <div class="month-year-selector">
          <select [(ngModel)]="selectedYear" (change)="onYearMonthChange()" class="year-select">
            <option *ngFor="let year of years" [value]="year">{{ year }}</option>
          </select>
          <select [(ngModel)]="selectedMonth" (change)="onYearMonthChange()" class="month-select">
            <option *ngFor="let month of months; let i = index" [value]="i + 1">
              {{ month }}
            </option>
          </select>
        </div>
        <div class="calendar-title">
          {{ getMonthName(selectedMonth) }} {{ selectedYear }}
        </div>
      </div>

      <div class="calendar-weekdays">
        <div class="weekday" *ngFor="let day of weekDays">{{ day }}</div>
      </div>

      <div class="calendar-days">
        <div
          *ngFor="let day of calendarDays"
          class="calendar-day"
          [class.other-month]="!day.isCurrentMonth"
          [class.today]="day.isToday"
          [class.selected]="isSelected(day)"
          [class.holiday]="day.isHoliday"
          [class.friday]="day.isFriday"
          (click)="selectDay(day)">
          <div class="day-content">
            <span class="day-number">{{ day.day }}</span>
            <div class="day-slots" *ngIf="day.isCurrentMonth && day.schedules && day.schedules.length > 0">
              <div class="slot-info">
                <span class="slot-label">کل:</span>
                <span class="slot-value total">{{ day.totalSlots || 0 }}</span>
              </div>
              <div class="slot-info">
                <span class="slot-label">آزاد:</span>
                <span class="slot-value available">{{ day.availableSlots || 0 }}</span>
              </div>
              <div class="slot-info">
                <span class="slot-label">گرفته:</span>
                <span class="slot-value booked">{{ day.bookedSlots || 0 }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .persian-calendar {
      background: white;
      border-radius: 16px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      padding: 40px;
      font-family: 'Vazirmatn', sans-serif;
      width: 100%;
    }

    .calendar-header {
      margin-bottom: 30px;
      text-align: center;
    }

    .month-year-selector {
      display: flex;
      gap: 15px;
      justify-content: center;
      margin-bottom: 20px;
    }

    .year-select,
    .month-select {
      padding: 12px 20px;
      border: 2px solid #dee2e6;
      border-radius: 10px;
      font-size: 1.1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: white;
      cursor: pointer;
      transition: all 0.3s ease;
      font-weight: 600;
    }

    .year-select:focus,
    .month-select:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .calendar-title {
      font-size: 1.8rem;
      font-weight: 800;
      color: #495057;
      margin-top: 10px;
    }

    .calendar-weekdays {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 8px;
      margin-bottom: 15px;
    }

    .weekday {
      text-align: center;
      font-weight: 700;
      font-size: 1.1rem;
      color: #667eea;
      padding: 16px;
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
      border-radius: 10px;
    }

    .calendar-days {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 8px;
    }

    .calendar-day {
      aspect-ratio: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      border-radius: 12px;
      transition: all 0.3s ease;
      background: #f8f9fa;
      border: 2px solid transparent;
      min-height: 100px;
      padding: 8px;
    }

    .calendar-day:hover {
      background: #e9ecef;
      transform: scale(1.05);
    }

    .calendar-day.other-month {
      opacity: 0.4;
      background: #ffffff;
    }

    .calendar-day.today {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      font-weight: 700;
      border-color: #667eea;
    }

    .calendar-day.selected {
      background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
      color: white;
      font-weight: 700;
      border-color: #f5576c;
      box-shadow: 0 4px 12px rgba(245, 87, 108, 0.3);
    }

    .calendar-day.holiday {
      background: linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%);
      color: white;
      font-weight: 700;
      border-color: #ff6b6b;
    }

    .calendar-day.friday {
      background: linear-gradient(135deg, #ffa726 0%, #fb8c00 100%);
      color: white;
      font-weight: 700;
      border-color: #ffa726;
    }

    .calendar-day.holiday.today,
    .calendar-day.friday.today {
      background: linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%);
      box-shadow: 0 4px 16px rgba(255, 107, 107, 0.4);
    }

    .day-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 100%;
      gap: 4px;
    }

    .day-number {
      font-size: 1.2rem;
      font-weight: 600;
      margin-bottom: 4px;
    }

    .calendar-day.today .day-number,
    .calendar-day.selected .day-number {
      color: white;
      font-weight: 700;
    }

    .day-slots {
      display: flex;
      flex-direction: column;
      gap: 3px;
      width: 100%;
      margin-top: 4px;
    }

    .slot-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 4px;
      padding: 3px 6px;
      border-radius: 5px;
      background: rgba(255, 255, 255, 0.9);
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .slot-label {
      font-size: 0.7rem;
      font-weight: 600;
      color: #495057;
    }

    .slot-value {
      font-weight: 700;
      font-size: 0.75rem;
      padding: 2px 6px;
      border-radius: 4px;
      min-width: 20px;
      text-align: center;
    }

    .slot-value.total {
      background: #667eea;
      color: white;
    }

    .slot-value.available {
      background: #28a745;
      color: white;
    }

    .slot-value.booked {
      background: #dc3545;
      color: white;
    }

    .calendar-day.today .slot-info,
    .calendar-day.selected .slot-info {
      background: rgba(255, 255, 255, 0.95);
    }
  `]
})
export class PersianCalendarComponent implements OnInit {
  @Input() selectedDate: Date | null = null;
  @Input() doctorId: number | null = null;
  @Output() dateSelected = new EventEmitter<Date>();

  selectedYear: number = 1403;
  selectedMonth: number = 1;
  calendarDays: CalendarDay[] = [];
  weekDays = ['شنبه', 'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه'];
  months = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'];

  years: number[] = [];
  allSchedules: DoctorSchedule[] = [];
  holidays: Holiday[] = [];
  appointmentCounts: AppointmentCountDto[] = [];

  constructor(
    private persianDateService: PersianDateService,
    private doctorScheduleService: DoctorScheduleService,
    private holidayService: HolidayService,
    private appointmentService: AppointmentService
  ) {
    // تولید لیست سال‌ها (از 1400 تا 1410)
    const currentYear = this.persianDateService.getCurrentPersianYear();
    for (let i = currentYear - 3; i <= currentYear + 7; i++) {
      this.years.push(i);
    }
  }

  ngOnInit() {
    const today = new Date();
    const persianDate = this.persianDateService.toPersianDate(today);
    this.selectedYear = persianDate.year;
    this.selectedMonth = persianDate.month;
    this.loadHolidays();
    this.loadSchedules();
  }

  onYearMonthChange() {
    this.loadHolidays();
    this.loadSchedules();
    // loadAppointmentCounts در loadSchedules فراخوانی می‌شود
  }

  loadHolidays() {
    // بارگذاری روزهای تعطیل
    this.holidayService.getAll({
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (result) => {
        this.holidays = result.items || [];
        this.generateCalendar();
      },
      error: (error) => {
        console.error('Error loading holidays:', error);
        this.holidays = [];
        this.generateCalendar();
      }
    });
  }

  loadSchedules() {
    if (!this.doctorId) {
      this.generateCalendar();
      return;
    }

    // بارگذاری همه برنامه‌ها برای این پزشک
    this.doctorScheduleService.getAll({
      doctorId: this.doctorId,
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (result) => {
        this.allSchedules = result.items || [];
        this.loadAppointmentCounts();
      },
      error: (error) => {
        console.error('Error loading schedules:', error);
        this.loadAppointmentCounts();
      }
    });
  }

  loadAppointmentCounts() {
    if (!this.doctorId) {
      this.appointmentCounts = [];
      this.generateCalendar();
      return;
    }

    // محاسبه تاریخ شروع و پایان ماه
    const startDate = this.persianDateService.toGregorian(this.selectedYear, this.selectedMonth, 1);
    const daysInMonth = this.getDaysInMonth(this.selectedYear, this.selectedMonth);
    const endDate = this.persianDateService.toGregorian(this.selectedYear, this.selectedMonth, daysInMonth);

    // تبدیل به فرمت yyyy-MM-dd
    const startDateStr = startDate.toISOString().split('T')[0];
    const endDateStr = endDate.toISOString().split('T')[0];

    this.appointmentService.getAppointmentCounts(this.doctorId, startDateStr, endDateStr).subscribe({
      next: (result) => {
        this.appointmentCounts = result.data || [];
        this.generateCalendar();
      },
      error: (error) => {
        console.error('Error loading appointment counts:', error);
        this.appointmentCounts = [];
        this.generateCalendar();
      }
    });
  }

  generateCalendar() {
    this.calendarDays = [];

    // محاسبه اولین روز ماه
    const firstDayOfMonth = this.getFirstDayOfMonth(this.selectedYear, this.selectedMonth);
    const daysInMonth = this.getDaysInMonth(this.selectedYear, this.selectedMonth);

    // محاسبه روز هفته اولین روز ماه (0 = یکشنبه, 6 = شنبه)
    // در تقویم شمسی: 0 = شنبه, 1 = یکشنبه, ..., 6 = جمعه
    let firstDayWeekday = firstDayOfMonth.getDay();
    // تبدیل به شمسی: شنبه = 0, یکشنبه = 1, ..., جمعه = 6
    firstDayWeekday = firstDayWeekday === 6 ? 0 : firstDayWeekday + 1;

    // اضافه کردن روزهای ماه قبل
    const prevMonth = this.selectedMonth === 1 ? 12 : this.selectedMonth - 1;
    const prevYear = this.selectedMonth === 1 ? this.selectedYear - 1 : this.selectedYear;
    const prevMonthDays = this.getDaysInMonth(prevYear, prevMonth);

    for (let i = firstDayWeekday - 1; i >= 0; i--) {
      const day = prevMonthDays - i;
      const date = this.persianDateService.toGregorian(prevYear, prevMonth, day);

      const isHoliday = this.isHoliday(date);
      const isFriday = this.isFriday(date);

      this.calendarDays.push({
        day,
        month: prevMonth,
        year: prevYear,
        isCurrentMonth: false,
        isToday: false,
        isHoliday,
        isFriday,
        date
      });
    }

    // اضافه کردن روزهای ماه جاری
    const today = new Date();
    const todayPersian = this.persianDateService.toPersianDate(today);

    for (let day = 1; day <= daysInMonth; day++) {
      const date = this.persianDateService.toGregorian(this.selectedYear, this.selectedMonth, day);
      const isToday = todayPersian.year === this.selectedYear &&
                     todayPersian.month === this.selectedMonth &&
                     todayPersian.day === day;

      // محاسبه روز هفته برای این تاریخ
      const dayOfWeek = this.getDayOfWeekFromDate(date);

      // پیدا کردن برنامه‌های مربوط به این روز هفته
      const daySchedules = this.allSchedules.filter(s => s.dayOfWeek === dayOfWeek);

      // پیدا کردن تعداد نوبت‌ها برای این تاریخ از جدول Appointments
      const dateStr = date.toISOString().split('T')[0];
      const appointmentCount = this.appointmentCounts.find(ac => {
        const acDateStr = new Date(ac.date).toISOString().split('T')[0];
        return acDateStr === dateStr;
      });

      // استفاده از تعداد واقعی از جدول Appointments
      const totalSlots = appointmentCount?.totalCount || 0;
      const bookedSlots = appointmentCount?.bookedCount || 0;
      const availableSlots = appointmentCount?.availableCount || 0;
      const isHoliday = this.isHoliday(date);
      const isFriday = this.isFriday(date);

      this.calendarDays.push({
        day,
        month: this.selectedMonth,
        year: this.selectedYear,
        isCurrentMonth: true,
        isToday,
        isHoliday,
        isFriday,
        date,
        schedules: daySchedules,
        totalSlots,
        bookedSlots,
        availableSlots
      });
    }

    // اضافه کردن روزهای ماه بعد برای کامل کردن تقویم
    const remainingDays = 42 - this.calendarDays.length; // 6 هفته * 7 روز
    const nextMonth = this.selectedMonth === 12 ? 1 : this.selectedMonth + 1;
    const nextYear = this.selectedMonth === 12 ? this.selectedYear + 1 : this.selectedYear;

    for (let day = 1; day <= remainingDays; day++) {
      const date = this.persianDateService.toGregorian(nextYear, nextMonth, day);

      const isHoliday = this.isHoliday(date);
      const isFriday = this.isFriday(date);

      this.calendarDays.push({
        day,
        month: nextMonth,
        year: nextYear,
        isCurrentMonth: false,
        isToday: false,
        isHoliday,
        isFriday,
        date
      });
    }
  }

  getFirstDayOfMonth(year: number, month: number): Date {
    return this.persianDateService.toGregorian(year, month, 1);
  }

  getDaysInMonth(year: number, month: number): number {
    // محاسبه تعداد روزهای ماه شمسی
    if (month <= 6) {
      return 31;
    } else if (month <= 11) {
      return 30;
    } else {
      // اسفند: 29 روز در سال عادی، 30 روز در سال کبیسه
      return this.isLeapYear(year) ? 30 : 29;
    }
  }

  isLeapYear(year: number): boolean {
    // محاسبه سال کبیسه شمسی
    const a = (year + 2346) * 683;
    const b = Math.floor(a / 2820);
    return a - b * 2820 < 683;
  }

  getMonthName(month: number): string {
    return this.months[month - 1];
  }

  selectDay(day: CalendarDay) {
    this.selectedDate = day.date;
    this.dateSelected.emit(day.date);
  }

  isSelected(day: CalendarDay): boolean {
    if (!this.selectedDate) return false;
    return day.date.toDateString() === this.selectedDate.toDateString();
  }

  getDayOfWeekFromDate(date: Date): DayOfWeek {
    // تبدیل روز هفته میلادی به شمسی
    // 0 = یکشنبه, 1 = دوشنبه, ..., 6 = شنبه
    // در شمسی: 0 = شنبه, 1 = یکشنبه, ..., 6 = جمعه
    const day = date.getDay();
    return day === 6 ? DayOfWeek.Saturday : (day + 1) as DayOfWeek;
  }

  isHoliday(date: Date): boolean {
    // بررسی اینکه آیا این تاریخ در لیست تعطیلات است
    const dateStr = date.toISOString().split('T')[0];
    return this.holidays.some(holiday => {
      const holidayDateStr = new Date(holiday.date).toISOString().split('T')[0];
      return holidayDateStr === dateStr;
    });
  }

  isFriday(date: Date): boolean {
    // بررسی اینکه آیا این روز جمعه است (در تقویم شمسی)
    const dayOfWeek = this.getDayOfWeekFromDate(date);
    return dayOfWeek === DayOfWeek.Friday;
  }
}
