import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { PersianDateService } from '../../services/persian-date.service';
import { DoctorScheduleService } from '../../services/doctor-schedule.service';
import { DoctorService } from '../../services/doctor.service';
import { ShiftService } from '../../services/shift.service';
import { ServiceService } from '../../services/service.service';
import { AppointmentService } from '../../services/appointment.service';
import { DoctorScheduleDialogComponent, DoctorScheduleDialogData } from './doctor-schedule-dialog.component';
import { CreateAppointmentDialogComponent, CreateAppointmentDialogData } from './create-appointment-dialog.component';
import { DoctorSchedule, DayOfWeek } from '../../models/doctor-schedule.model';
import { Doctor } from '../../models/doctor.model';
import { Shift } from '../../models/shift.model';
import { Service } from '../../models/service.model';
import { getDayOfWeekPersianName } from '../../utils/day-of-week.util';

@Component({
  selector: 'app-doctor-schedules',
  templateUrl: './doctor-schedules.component.html',
  styleUrls: ['./doctor-schedules.component.scss']
})
export class DoctorSchedulesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'doctorName', 'date', 'shiftName', 'actions'];
  schedules: DoctorSchedule[] = [];
  filteredSchedules: DoctorSchedule[] = [];
  doctors: Doctor[] = [];
  shifts: Shift[] = [];
  services: Service[] = [];
  filterValue: string = '';
  isLoading = false;
  selectedDoctorId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private doctorScheduleService: DoctorScheduleService,
    private doctorService: DoctorService,
    private shiftService: ShiftService,
    private serviceService: ServiceService,
    private appointmentService: AppointmentService,
    public persianDateService: PersianDateService
  ) {}

  ngOnInit() {
    this.loadDoctors();
    this.loadShifts();
    this.loadServices();

    // بررسی query params برای فیلتر بر اساس پزشک
    this.route.queryParams.subscribe(params => {
      if (params['doctorId']) {
        this.selectedDoctorId = +params['doctorId'];
        this.loadSchedules();
      } else {
        this.selectedDoctorId = null;
        this.loadSchedules();
      }
    });
  }


  loadDoctors() {
    this.doctorService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.doctors = result.items;
      },
      error: (error) => {
        console.error('Error loading doctors:', error);
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
      }
    });
  }

  loadSchedules() {
    this.isLoading = true;
    this.doctorScheduleService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.schedules = result.items;
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری برنامه‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  openAddDialog() {
    this.dialogService.open(DoctorScheduleDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: {
        schedule: null,
        doctors: this.doctors,
        shifts: this.shifts
      } as DoctorScheduleDialogData
    }).subscribe(result => {
      if (result) {
        this.saveSchedule(result);
      }
    });
  }

  editSchedule(schedule: DoctorSchedule) {
    this.dialogService.open(DoctorScheduleDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: {
        schedule: schedule,
        doctors: this.doctors,
        shifts: this.shifts
      } as DoctorScheduleDialogData
    }).subscribe(result => {
      if (result) {
        this.saveSchedule(result, schedule.id);
      }
    });
  }

  deleteSchedule(schedule: DoctorSchedule) {
    if (schedule.id) {
      this.dialogService.confirm({
        title: 'حذف برنامه',
        message: `آیا از حذف برنامه "${schedule.doctorName || ''}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.doctorScheduleService.delete(schedule.id!).subscribe({
            next: () => {
              this.schedules = this.schedules.filter(s => s.id !== schedule.id);
              this.filteredSchedules = [...this.schedules];
              this.applyFilter();
              this.snackbarService.success('برنامه با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف برنامه';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveSchedule(scheduleData: DoctorSchedule, id?: number) {
    if (id) {
      const updateData = { ...scheduleData, id };
      this.doctorScheduleService.update(updateData).subscribe({
        next: (updatedSchedule) => {
          const index = this.schedules.findIndex(s => s.id === id);
          if (index !== -1) {
            this.schedules[index] = updatedSchedule;
            this.filteredSchedules = [...this.schedules];
            this.applyFilter();
          }
          this.snackbarService.success('برنامه با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش برنامه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.doctorScheduleService.create(scheduleData).subscribe({
        next: (newSchedule) => {
          this.schedules = [...this.schedules, newSchedule];
          this.filteredSchedules = [...this.schedules];
          this.applyFilter();
          this.snackbarService.success('برنامه با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن برنامه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    let baseSchedules = this.schedules;

    // فیلتر بر اساس پزشک اگر انتخاب شده باشد
    if (this.selectedDoctorId) {
      baseSchedules = baseSchedules.filter(s => s.doctorId === this.selectedDoctorId);
    }

    // فیلتر بر اساس متن جستجو
    if (!this.filterValue.trim()) {
      this.filteredSchedules = baseSchedules;
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredSchedules = baseSchedules.filter(schedule =>
      schedule.doctorName?.toLowerCase().includes(filter) ||
      schedule.shiftName?.toLowerCase().includes(filter) ||
      (schedule.dayOfWeekName && schedule.dayOfWeekName.toLowerCase().includes(filter))
    );
  }

  getDayOfWeekName(dayOfWeek: DayOfWeek | undefined): string {
    if (dayOfWeek === undefined) return '';
    return getDayOfWeekPersianName(dayOfWeek);
  }

  goToWeeklySchedule() {
    // خواندن doctorId از query params
    const doctorId = this.route.snapshot.queryParams['doctorId'];
    if (doctorId) {
      this.router.navigate(['/panel/weekly-schedule'], { queryParams: { doctorId: doctorId } });
    } else {
      this.router.navigate(['/panel/weekly-schedule']);
    }
  }

  onDateSelected(date: Date) {
    if (!this.selectedDoctorId) {
      this.snackbarService.error('لطفاً ابتدا پزشک را انتخاب کنید', 'بستن', 3000);
      return;
    }

    this.dialogService.open(CreateAppointmentDialogComponent, {
      width: '750px',
      maxWidth: '95vw',
      data: {
        doctorId: this.selectedDoctorId,
        selectedDate: date,
        shifts: this.shifts,
        services: this.services
      } as CreateAppointmentDialogData
    }).subscribe(result => {
      if (result) {
        this.createAppointment(date, result);
      }
    });
  }

  createAppointment(date: Date, appointmentData: any) {
    if (!this.selectedDoctorId) return;

    // محاسبه روز هفته از تاریخ
    const dayOfWeek = this.getDayOfWeekFromDate(date);

    // ایجاد doctorSchedule
    const scheduleData: DoctorSchedule = {
      doctorId: this.selectedDoctorId,
      dayOfWeek: dayOfWeek,
      shiftId: appointmentData.shiftId,
      serviceId: appointmentData.serviceId,
      startTime: appointmentData.startTime,
      endTime: appointmentData.endTime,
      count: appointmentData.count || 1
    };

    // بررسی اینکه آیا برنامه قبلی برای این روز و شیفت وجود دارد
    this.doctorScheduleService.getAll({
      doctorId: this.selectedDoctorId,
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result: any) => {
        const existingSchedule = result.items?.find((s: DoctorSchedule) =>
          s.dayOfWeek === scheduleData.dayOfWeek &&
          s.shiftId === scheduleData.shiftId
        );

        if (existingSchedule && existingSchedule.id) {
          // استفاده از برنامه موجود
          this.createAppointmentFromSchedule(existingSchedule.id, date, appointmentData);
        } else {
          // ایجاد برنامه جدید
          this.doctorScheduleService.create(scheduleData).subscribe({
            next: (newSchedule) => {
              if (newSchedule.id) {
                this.createAppointmentFromSchedule(newSchedule.id, date, appointmentData);
              }
            },
            error: (error: any) => {
              const errorMessage = error.error?.message || 'خطا در ایجاد برنامه';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      },
      error: (error: any) => {
        // در صورت خطا، سعی می‌کنیم برنامه جدید ایجاد کنیم
        this.doctorScheduleService.create(scheduleData).subscribe({
          next: (newSchedule) => {
            if (newSchedule.id) {
              this.createAppointmentFromSchedule(newSchedule.id, date, appointmentData);
            }
          },
          error: (err: any) => {
            const errorMessage = err.error?.message || 'خطا در ایجاد برنامه';
            this.snackbarService.error(errorMessage, 'بستن', 5000);
          }
        });
      }
    });
  }

  createAppointmentFromSchedule(doctorScheduleId: number, date: Date, appointmentData: any) {
    // تبدیل زمان شروع به TimeSpan
    const [startHours, startMinutes] = appointmentData.startTime.split(':').map(Number);
    const [endHours, endMinutes] = appointmentData.endTime.split(':').map(Number);

    const appointmentDateTime = new Date(date);
    appointmentDateTime.setHours(startHours, startMinutes, 0, 0);

    const expireDateTime = new Date(date);
    expireDateTime.setHours(endHours, endMinutes, 0, 0);

    // محاسبه فاصله زمانی بین نوبت‌ها
    const timeSlotDuration = (endHours * 60 + endMinutes) - (startHours * 60 + startMinutes);
    const slotDuration = timeSlotDuration / appointmentData.count;

    // ایجاد نوبت‌ها بر اساس Count
    let createdCount = 0;
    const appointments: Promise<any>[] = [];

    for (let i = 0; i < appointmentData.count; i++) {
      const slotStartMinutes = startHours * 60 + startMinutes + (slotDuration * i);
      const slotEndMinutes = slotStartMinutes + slotDuration;

      const slotStartHours = Math.floor(slotStartMinutes / 60);
      const slotStartMins = slotStartMinutes % 60;
      const slotEndHours = Math.floor(slotEndMinutes / 60);
      const slotEndMins = slotEndMinutes % 60;

      const slotAppointmentDateTime = new Date(date);
      slotAppointmentDateTime.setHours(slotStartHours, slotStartMins, 0, 0);

      const slotExpireDateTime = new Date(date);
      slotExpireDateTime.setHours(slotEndHours, slotEndMins, 0, 0);

      const appointmentRequest = {
        doctorId: this.selectedDoctorId!,
        doctorScheduleId: doctorScheduleId,
        appointmentDateTime: slotAppointmentDateTime.toISOString(),
        expireDateTime: slotExpireDateTime.toISOString(),
        startTime: `${String(slotStartHours).padStart(2, '0')}:${String(slotStartMins).padStart(2, '0')}`,
        endTime: `${String(slotEndHours).padStart(2, '0')}:${String(slotEndMins).padStart(2, '0')}`
      };

      appointments.push(
        firstValueFrom(this.appointmentService.create(appointmentRequest))
      );
    }

    Promise.all(appointments).then(() => {
      this.snackbarService.success(`تعداد ${appointmentData.count} نوبت با موفقیت ایجاد شد`, 'بستن', 3000);
      this.loadSchedules();
    }).catch((error) => {
      const errorMessage = error.error?.message || 'خطا در ایجاد نوبت‌ها';
      this.snackbarService.error(errorMessage, 'بستن', 5000);
    });
  }

  getDayOfWeekFromDate(date: Date): DayOfWeek {
    const day = date.getDay();
    // تبدیل به روز هفته شمسی: Saturday=6 -> 0, Sunday=0 -> 1, Monday=1 -> 2, ...
    return day === 6 ? DayOfWeek.Saturday : (day + 1) as DayOfWeek;
  }
}
