import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Doctor, DoctorListDto } from '../../models/doctor.model';
import { ClinicService } from '../../services/clinic.service';
import { DialogService } from '../../services/dialog.service';
import { DoctorService } from '../../services/doctor.service';
import { SnackbarService } from '../../services/snackbar.service';
import { DoctorActionsMenuComponent } from './doctor-actions-menu.component';

@Component({
  selector: 'app-doctors',
  templateUrl: './doctors.component.html',
  styleUrls: ['./doctors.component.scss']
})
export class DoctorsComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = ['id', 'firstName', 'lastName', 'nationalCode', 'phone', 'medicalCode', 'actions'];
  doctors: DoctorListDto[] = [];
  filteredDoctors: DoctorListDto[] = [];
  filterValue: string = '';
  selectedClinicId: number | null = null;
  isLoading = false;
  private clinicSubscription?: Subscription;

  constructor(
    private router: Router,
    private snackbarService: SnackbarService,
    private doctorService: DoctorService,
    private dialogService: DialogService,
    private clinicService: ClinicService
  ) { }

  ngOnInit() {
    this.loadDoctors();

    // Subscribe to selected clinic from header dropdown
    this.clinicSubscription = this.clinicService.selectedClinic$.subscribe(clinic => {
      if (clinic) {
        this.selectedClinicId = clinic.id || null;
        this.loadDoctors(); // بارگذاری مجدد پزشکان با فیلتر کلینیک
      } else {
        this.selectedClinicId = null;
        this.loadDoctors(); // بارگذاری مجدد پزشکان بدون فیلتر
      }
    });
  }

  ngOnDestroy() {
    if (this.clinicSubscription) {
      this.clinicSubscription.unsubscribe();
    }
  }

  loadClinics() {
    // دیگر نیازی به بارگذاری لیست کلینیک‌ها نیست
    // چون از dropdown در header استفاده می‌شود
  }

  loadDoctors() {
    this.isLoading = true;
    const params: any = { page: 1, pageSize: 100 };

    // اگر کلینیک انتخاب شده باشد، clinicId را اضافه کن
    if (this.selectedClinicId) {
      params.clinicId = this.selectedClinicId;
    }

    this.doctorService.getAll(params).subscribe({
      next: (result) => {
        this.doctors = result.items;
        this.filteredDoctors = [...this.doctors];
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری پزشکان';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  openActionsMenu(event: Event, doctor: DoctorListDto) {
    event.stopPropagation();

    // Get button position
    const button = event.currentTarget as HTMLElement;
    const rect = button.getBoundingClientRect();

    this.dialogService.open(DoctorActionsMenuComponent, {
      width: 'auto',
      maxWidth: '90vw',
      data: doctor,
      position: {
        top: `${rect.bottom + 8}px`,
        right: `${window.innerWidth - rect.right}px`
      },
      transparentOverlay: true
    }).subscribe(result => {
      if (result && result.action) {
        this.handleMenuAction(result.action, result.doctor);
      }
    });
  }

  handleMenuAction(action: string, doctor: DoctorListDto) {
    if (action === 'edit') {
      this.editDoctor(doctor);
    } else if (action === 'tariff') {
      this.viewDoctorTariffs(doctor);
    } else if (action === 'schedule') {
      this.viewDoctorSchedule(doctor);
    } else if (action === 'visit-info') {
      this.viewDoctorVisitInfo(doctor);
    } else if (action === 'delete') {
      this.deleteDoctor(doctor);
    }
  }

  viewDoctorVisitInfo(doctor: DoctorListDto) {
    if (doctor.id) {
      this.router.navigate(['/panel/doctor-visit-infos'], {
        queryParams: { doctorId: doctor.id }
      });
    }
  }

  viewDoctorTariffs(doctor: DoctorListDto) {
    if (doctor.id) {
      this.router.navigate(['/panel/doctor-tariffs'], {
        queryParams: { doctorId: doctor.id }
      });
    }
  }

  viewDoctorSchedule(doctor: DoctorListDto) {
    if (doctor.id) {
      this.router.navigate(['/panel/doctor-schedules'], {
        queryParams: { doctorId: doctor.id }
      });
    }
  }

  openAddDialog() {
    this.router.navigate(['/panel/doctors/new']);
  }

  editDoctor(doctor: DoctorListDto) {
    if (doctor.id) {
      this.router.navigate(['/panel/doctors', doctor.id]);
    }
  }

  goToWeeklySchedule() {
    this.router.navigate(['/panel/weekly-schedule']);
  }

  viewWeeklySchedule(doctor: DoctorListDto) {
    // هدایت به صفحه برنامه هفتگی با انتخاب خودکار پزشک
    this.router.navigate(['/panel/weekly-schedule'], {
      queryParams: { doctorId: doctor.id }
    });
  }

  viewMonthlySchedule(doctor: DoctorListDto) {
    // هدایت به صفحه برنامه ماهانه
    this.router.navigate(['/panel/monthly-schedule'], {
      queryParams: { doctorId: doctor.id }
    });
  }

  viewServiceTariffs(doctor: DoctorListDto) {
    // هدایت به صفحه تعرفه خدمات با فیلتر پزشک
    this.router.navigate(['/panel/service-tariffs'], {
      queryParams: { doctorId: doctor.id }
    });
  }

  getDoctorDisplayName(doctor: DoctorListDto): string {
    return `${doctor.firstName} ${doctor.lastName}`;
  }

  deleteDoctor(doctor: DoctorListDto) {
    if (doctor.id) {
      const doctorName = this.getDoctorDisplayName(doctor);
      this.dialogService.confirm({
        title: 'حذف پزشک',
        message: `آیا از حذف پزشک "${doctorName}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe((result: boolean) => {
        if (result) {
          this.doctorService.delete(doctor.id!).subscribe({
            next: () => {
              this.doctors = this.doctors.filter(d => d.id !== doctor.id);
              this.filteredDoctors = [...this.doctors];
              this.applyFilter();
              this.snackbarService.success('پزشک با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف پزشک';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  applyFilter() {
    let filtered = [...this.doctors];

    // فیلتر بر اساس متن جستجو
    if (this.filterValue.trim()) {
      const filter = this.filterValue.trim().toLowerCase();
      filtered = filtered.filter(doctor => {
        const firstName = doctor.firstName?.toLowerCase() || '';
        const lastName = doctor.lastName?.toLowerCase() || '';
        const phone = doctor.phone || '';
        const medicalCode = doctor.medicalCode?.toLowerCase() || '';
        const nationalCode = doctor.nationalCode || '';

        return firstName.includes(filter) ||
          lastName.includes(filter) ||
          phone.includes(filter) ||
          medicalCode.includes(filter) ||
          nationalCode.includes(filter);
      });
    }

    this.filteredDoctors = filtered;
  }

}
