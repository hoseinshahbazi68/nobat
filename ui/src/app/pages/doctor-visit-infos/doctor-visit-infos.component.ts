import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SnackbarService } from '../../services/snackbar.service';
import { DoctorVisitInfoService } from '../../services/doctor-visit-info.service';
import { DoctorVisitInfo } from '../../models/doctor-visit-info.model';
import { DoctorService } from '../../services/doctor.service';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-doctor-visit-infos',
  templateUrl: './doctor-visit-infos.component.html',
  styleUrls: ['./doctor-visit-infos.component.scss']
})
export class DoctorVisitInfosComponent implements OnInit {
  visitInfoForm: FormGroup;
  visitInfo: DoctorVisitInfo | null = null;
  doctor: Doctor | null = null;
  doctorId: number | null = null;
  isLoading = false;
  isEditMode = true; // به صورت پیش‌فرض در حالت ویرایش
  isSaving = false;

  constructor(
    private fb: FormBuilder,
    private snackbarService: SnackbarService,
    private visitInfoService: DoctorVisitInfoService,
    private doctorService: DoctorService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.visitInfoForm = this.fb.group({
      about: [''],
      clinicAddress: [''],
      clinicPhone: [''],
      officeHours: ['']
    });
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params: any) => {
      if (params['doctorId']) {
        this.doctorId = +params['doctorId'];
        this.loadDoctor();
        this.loadVisitInfo();
      } else {
        this.snackbarService.error('شناسه پزشک مشخص نشده است', 'بستن', 3000);
        this.router.navigate(['/panel/doctors']);
      }
    });
  }

  loadDoctor() {
    if (!this.doctorId) return;

    this.doctorService.getById(this.doctorId).subscribe({
      next: (doctor) => {
        this.doctor = doctor;
      },
      error: (error) => {
        console.error('Error loading doctor:', error);
        this.snackbarService.error('خطا در بارگذاری اطلاعات پزشک', 'بستن', 3000);
      }
    });
  }

  loadVisitInfo() {
    if (!this.doctorId) return;

    this.isLoading = true;
    this.visitInfoService.getByDoctorId(this.doctorId).subscribe({
      next: (result) => {
        this.visitInfo = result;
        this.visitInfoForm.patchValue({
          about: result.about || '',
          clinicAddress: result.clinicAddress || '',
          clinicPhone: result.clinicPhone || '',
          officeHours: result.officeHours || ''
        });
        this.isEditMode = true; // همیشه در حالت ویرایش
        this.isLoading = false;
      },
      error: (error) => {
        // اگر اطلاعات ویزیت وجود نداشت، فرم را خالی نگه داریم
        if (error?.status === 404) {
          this.visitInfo = null;
          this.visitInfoForm.reset();
          this.isEditMode = true; // حالت ایجاد
        } else {
          const errorMessage = error?.message || error?.error?.message || 'خطا در بارگذاری اطلاعات ویزیت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
        this.isLoading = false;
      }
    });
  }

  toggleEditMode() {
    this.isEditMode = !this.isEditMode;
    if (!this.isEditMode && this.visitInfo) {
      // اگر از حالت ویرایش خارج شدیم، مقادیر را به حالت اولیه برگردانیم
      this.visitInfoForm.patchValue({
        about: this.visitInfo.about || '',
        clinicAddress: this.visitInfo.clinicAddress || '',
        clinicPhone: this.visitInfo.clinicPhone || '',
        officeHours: this.visitInfo.officeHours || ''
      });
    }
  }

  saveVisitInfo() {
    if (!this.doctorId) {
      this.snackbarService.error('شناسه پزشک مشخص نشده است', 'بستن', 3000);
      return;
    }

    this.visitInfoForm.markAllAsTouched();
    if (this.visitInfoForm.invalid) {
      return;
    }

    this.isSaving = true;
    const formValue = this.visitInfoForm.value;

    // تبدیل رشته‌های خالی به undefined
    Object.keys(formValue).forEach(key => {
      if (formValue[key] === '') {
        formValue[key] = undefined;
      }
    });

    if (this.visitInfo?.id) {
      // به‌روزرسانی
      const updateData = { ...formValue, id: this.visitInfo.id };
      this.visitInfoService.update(updateData).subscribe({
        next: (updatedVisitInfo) => {
          this.visitInfo = updatedVisitInfo;
          this.isEditMode = true; // بعد از ذخیره هم در حالت ویرایش بماند
          this.isSaving = false;
          this.snackbarService.success('اطلاعات ویزیت با موفقیت به‌روزرسانی شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در به‌روزرسانی اطلاعات ویزیت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isSaving = false;
        }
      });
    } else {
      // ایجاد جدید
      const createData = { ...formValue, doctorId: this.doctorId };
      this.visitInfoService.create(createData).subscribe({
        next: (newVisitInfo) => {
          this.visitInfo = newVisitInfo;
          this.isEditMode = true; // بعد از ذخیره هم در حالت ویرایش بماند
          this.isSaving = false;
          this.snackbarService.success('اطلاعات ویزیت با موفقیت ایجاد شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در ایجاد اطلاعات ویزیت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isSaving = false;
        }
      });
    }
  }

  cancelEdit() {
    if (this.visitInfo) {
      this.visitInfoForm.patchValue({
        about: this.visitInfo.about || '',
        clinicAddress: this.visitInfo.clinicAddress || '',
        clinicPhone: this.visitInfo.clinicPhone || '',
        officeHours: this.visitInfo.officeHours || ''
      });
    } else {
      this.visitInfoForm.reset();
    }
    this.isEditMode = false;
  }

  getDoctorDisplayName(): string {
    if (this.doctor) {
      const name = `${this.doctor.firstName || ''} ${this.doctor.lastName || ''}`.trim();
      return name ? `${name} (${this.doctor.medicalCode})` : this.doctor.medicalCode || '';
    }
    return this.visitInfo?.doctorName || this.visitInfo?.medicalCode || '-';
  }

  goBack() {
    this.router.navigate(['/panel/doctors']);
  }
}
