import { Component, OnInit } from '@angular/core';
import { Clinic } from '../../models/clinic.model';
import { DialogService } from '../../services/dialog.service';
import { ClinicService } from '../../services/clinic.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ClinicDialogComponent } from './clinic-dialog.component';

@Component({
  selector: 'app-clinics',
  templateUrl: './clinics.component.html',
  styleUrls: ['./clinics.component.scss']
})
export class ClinicsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'address', 'phone', 'email', 'cityName', 'isActive', 'actions'];
  clinics: Clinic[] = [];
  filteredClinics: Clinic[] = [];
  filterValue: string = '';
  isLoading = false;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private clinicService: ClinicService
  ) { }

  ngOnInit() {
    this.loadClinics();
  }

  loadClinics() {
    this.isLoading = true;
    this.clinicService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.clinics = result.items;
          this.filteredClinics = [...this.clinics];
        } else {
          this.clinics = [];
          this.filteredClinics = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری کلینیک‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  openAddDialog() {
    this.dialogService.open(ClinicDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result) {
        this.saveClinic(result);
      }
    });
  }

  editClinic(clinic: Clinic) {
    this.dialogService.open(ClinicDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: clinic
    }).subscribe(result => {
      if (result) {
        this.saveClinic(result, clinic.id);
      }
    });
  }

  deleteClinic(clinic: Clinic) {
    if (clinic.id) {
      this.dialogService.confirm({
        title: 'حذف کلینیک',
        message: `آیا از حذف کلینیک "${clinic.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.clinicService.delete(clinic.id!).subscribe({
            next: () => {
              this.clinics = this.clinics.filter(c => c.id !== clinic.id);
              this.filteredClinics = [...this.clinics];
              this.applyFilter();
              this.snackbarService.success('کلینیک با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف کلینیک';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveClinic(clinicData: any, id?: number) {
    if (id) {
      const updateData = { ...clinicData, id };
      this.clinicService.update(updateData).subscribe({
        next: () => {
          this.snackbarService.success('کلینیک با موفقیت ویرایش شد', 'بستن', 3000);
          this.loadClinics();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش کلینیک';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.clinicService.create(clinicData).subscribe({
        next: () => {
          this.snackbarService.success('کلینیک با موفقیت اضافه شد', 'بستن', 3000);
          this.loadClinics();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن کلینیک';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredClinics = [...this.clinics];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredClinics = this.clinics.filter(clinic =>
      clinic.name.toLowerCase().includes(filter) ||
      (clinic.address && clinic.address.toLowerCase().includes(filter)) ||
      (clinic.phone && clinic.phone.toLowerCase().includes(filter)) ||
      (clinic.email && clinic.email.toLowerCase().includes(filter)) ||
      (clinic.cityName && clinic.cityName.toLowerCase().includes(filter))
    );
  }
}
