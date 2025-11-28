import { Component, forwardRef, Input, OnInit, HostListener, ElementRef, ChangeDetectorRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { PersianDateService } from '../../services/persian-date.service';

@Component({
  selector: 'app-persian-datepicker',
  templateUrl: './persian-datepicker.component.html',
  styleUrls: ['./persian-datepicker.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PersianDatepickerComponent),
      multi: true
    }
  ]
})
export class PersianDatepickerComponent implements OnInit, ControlValueAccessor {
  @Input() placeholder: string = 'تاریخ را انتخاب کنید';
  @Input() label: string = '';
  @Input() required: boolean = false;
  @Input() disabled: boolean = false;

  value: Date | null = null;
  persianDateString: string = '';
  isCalendarOpen: boolean = false;

  currentYear: number = 1403;
  currentMonth: number = 1;
  currentDay: number = 1;

  selectedYear: number = 1403;
  selectedMonth: number = 1;
  selectedDay: number = 1;

  calendarDays: Array<{day: number, isCurrentMonth: boolean, isSelected: boolean, isToday: boolean}> = [];

  persianMonths = [
    'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
    'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
  ];

  persianWeekDays = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];

  private onChange = (value: Date | null) => {};
  private onTouched = () => {};

  constructor(
    private persianDateService: PersianDateService,
    private elementRef: ElementRef,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.initializeCurrentDate();
  }

  initializeCurrentDate() {
    const today = new Date();
    const persianToday = this.persianDateService.toPersian(today);

    if (!persianToday || persianToday === '') {
      // در صورت خطا، از تاریخ پیش‌فرض استفاده می‌کنیم
      const now = new Date();
      this.currentYear = 1403;
      this.currentMonth = 1;
      this.currentDay = 1;
    } else {
      const parts = persianToday.split('/').map((p: string) => parseInt(p.trim()));
      if (parts.length === 3 && !isNaN(parts[0]) && !isNaN(parts[1]) && !isNaN(parts[2])) {
        this.currentYear = parts[0];
        this.currentMonth = parts[1];
        this.currentDay = parts[2];
      } else {
        this.currentYear = 1403;
        this.currentMonth = 1;
        this.currentDay = 1;
      }
    }

    // اگر مقدار قبلی تنظیم نشده باشد، از تاریخ امروز استفاده می‌کنیم
    if (!this.value) {
      this.selectedYear = this.currentYear;
      this.selectedMonth = this.currentMonth;
      this.selectedDay = this.currentDay;
    }

    this.generateCalendar();
  }

  writeValue(value: Date | null): void {
    this.value = value;
    if (value) {
      this.persianDateString = this.persianDateService.toPersian(value);
      const parts = this.persianDateString.split('/').map((p: string) => parseInt(p));
      this.selectedYear = parts[0];
      this.selectedMonth = parts[1];
      this.selectedDay = parts[2];
      this.generateCalendar();
    } else {
      this.persianDateString = '';
    }
  }

  registerOnChange(fn: (value: Date | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.isCalendarOpen && !this.elementRef.nativeElement.contains(event.target)) {
      this.closeCalendar();
    }
  }

  toggleCalendar() {
    if (!this.disabled) {
      this.isCalendarOpen = !this.isCalendarOpen;
      if (this.isCalendarOpen) {
        this.generateCalendar();
      }
    }
  }

  closeCalendar() {
    this.isCalendarOpen = false;
  }

  onInputChange(event: any) {
    const value = event.target.value;
    this.persianDateString = value;

    if (value && this.isValidPersianDate(value)) {
      try {
        const gregorianDate = this.persianDateService.parsePersianDate(value);
        this.value = gregorianDate;
        const parts = value.split('/').map((p: string) => parseInt(p));
        this.selectedYear = parts[0];
        this.selectedMonth = parts[1];
        this.selectedDay = parts[2];
        this.generateCalendar();
        this.onChange(this.value);
        this.onTouched();
      } catch (e) {
        console.error('خطا در تبدیل تاریخ:', e);
      }
    } else if (!value) {
      this.value = null;
      this.onChange(null);
      this.onTouched();
    }
  }

  isValidPersianDate(dateString: string): boolean {
    const pattern = /^\d{4}\/\d{1,2}\/\d{1,2}$/;
    if (!pattern.test(dateString)) return false;

    const parts = dateString.split('/').map((p: string) => parseInt(p));
    if (parts.length !== 3) return false;

    const [year, month, day] = parts;
    if (year < 1300 || year > 1500) return false;
    if (month < 1 || month > 12) return false;
    if (day < 1 || day > 31) return false;

    return true;
  }

  selectDate(day: number, isCurrentMonth: boolean) {
    if (!isCurrentMonth) return;

    this.selectedDay = day;

    const persianDate = `${this.selectedYear}/${String(this.selectedMonth).padStart(2, '0')}/${String(day).padStart(2, '0')}`;
    this.persianDateString = persianDate;

    try {
      const gregorianDate = this.persianDateService.parsePersianDate(persianDate);
      this.value = gregorianDate;
      this.onChange(this.value);
      this.onTouched();
      this.generateCalendar();
      this.closeCalendar();
    } catch (e) {
      console.error('خطا در تبدیل تاریخ:', e);
    }
  }

  previousMonth(event?: Event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    if (this.selectedMonth > 1) {
      this.selectedMonth--;
    } else {
      this.selectedMonth = 12;
      this.selectedYear--;
    }
    this.generateCalendar();
    this.cdr.detectChanges();
  }

  nextMonth(event?: Event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    if (this.selectedMonth < 12) {
      this.selectedMonth++;
    } else {
      this.selectedMonth = 1;
      this.selectedYear++;
    }
    this.generateCalendar();
    this.cdr.detectChanges();
  }

  previousYear(event?: Event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.selectedYear--;
    this.generateCalendar();
    this.cdr.detectChanges();
  }

  nextYear(event?: Event) {
    if (event) {
      event.stopPropagation();
      event.preventDefault();
    }
    this.selectedYear++;
    this.generateCalendar();
    this.cdr.detectChanges();
  }

  goToToday() {
    const today = new Date();
    this.value = today;
    this.persianDateString = this.persianDateService.toPersian(today);
    const parts = this.persianDateString.split('/').map((p: string) => parseInt(p));
    this.selectedYear = parts[0];
    this.selectedMonth = parts[1];
    this.selectedDay = parts[2];
    this.generateCalendar();
    this.onChange(this.value);
    this.onTouched();
  }

  generateCalendar() {
    this.calendarDays = [];

    // محاسبه روز اول ماه
    const firstDayOfMonth = this.getFirstDayOfMonth(this.selectedYear, this.selectedMonth);

    // محاسبه تعداد روزهای ماه
    const daysInMonth = this.getDaysInMonth(this.selectedYear, this.selectedMonth);

    // روزهای ماه قبل (برای پر کردن تقویم)
    const prevMonth = this.selectedMonth === 1 ? 12 : this.selectedMonth - 1;
    const prevYear = this.selectedMonth === 1 ? this.selectedYear - 1 : this.selectedYear;
    const prevMonthDays = this.getDaysInMonth(prevYear, prevMonth);

    // اضافه کردن روزهای ماه قبل
    for (let i = firstDayOfMonth - 1; i >= 0; i--) {
      this.calendarDays.push({
        day: prevMonthDays - i,
        isCurrentMonth: false,
        isSelected: false,
        isToday: false
      });
    }

    // اضافه کردن روزهای ماه جاری
    const currentPersianDate = this.persianDateString;
    for (let day = 1; day <= daysInMonth; day++) {
      const dayPersianDate = `${this.selectedYear}/${String(this.selectedMonth).padStart(2, '0')}/${String(day).padStart(2, '0')}`;
      const isSelected = this.value !== null && currentPersianDate === dayPersianDate;
      const isToday = day === this.currentDay &&
                     this.selectedMonth === this.currentMonth &&
                     this.selectedYear === this.currentYear;

      this.calendarDays.push({
        day: day,
        isCurrentMonth: true,
        isSelected: isSelected,
        isToday: isToday
      });
    }

    // پر کردن باقی روزهای هفته (روزهای ماه بعد)
    const remainingDays = 42 - this.calendarDays.length;
    for (let day = 1; day <= remainingDays; day++) {
      this.calendarDays.push({
        day: day,
        isCurrentMonth: false,
        isSelected: false,
        isToday: false
      });
    }

    // اطمینان از به‌روزرسانی view
    this.cdr.markForCheck();
  }

  getFirstDayOfMonth(year: number, month: number): number {
    // محاسبه روز هفته برای اول ماه شمسی
    // تبدیل اول ماه شمسی به میلادی
    const gregorianDate = this.persianDateService.toGregorian(`${year}/${String(month).padStart(2, '0')}/01`);
    const dayOfWeek = gregorianDate.getDay();
    // تبدیل به فرمت شمسی (شنبه = 0, یکشنبه = 1, دوشنبه = 2, سه‌شنبه = 3, چهارشنبه = 4, پنج‌شنبه = 5, جمعه = 6)
    // در تقویم میلادی: یکشنبه = 0, دوشنبه = 1, سه‌شنبه = 2, چهارشنبه = 3, پنج‌شنبه = 4, جمعه = 5, شنبه = 6
    // در تقویم شمسی: شنبه = 0, یکشنبه = 1, دوشنبه = 2, سه‌شنبه = 3, چهارشنبه = 4, پنج‌شنبه = 5, جمعه = 6
    // تبدیل: شنبه میلادی (6) -> شنبه شمسی (0) = (6 + 1) % 7
    // یکشنبه میلادی (0) -> یکشنبه شمسی (1) = (0 + 1) % 7
    return (dayOfWeek + 1) % 7;
  }

  getDaysInMonth(year: number, month: number): number {
    // تعداد روزهای ماه‌های شمسی
    if (month <= 6) return 31;
    if (month <= 11) return 30;
    // اسفند
    // بررسی سال کبیسه
    return this.isLeapYear(year) ? 30 : 29;
  }

  isLeapYear(year: number): boolean {
    // الگوریتم کبیسه‌گیری شمسی
    const a = (year + 2346) % 128;
    return a < 30 && a % 4 === (a < 29 ? 0 : 1);
  }
}
