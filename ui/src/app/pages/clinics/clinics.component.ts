import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Clinic } from '../../models/clinic.model';
import { DialogService } from '../../services/dialog.service';
import { ClinicService } from '../../services/clinic.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ClinicUsersDialogComponent } from './clinic-users-dialog.component';

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

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private router: Router,
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private clinicService: ClinicService
  ) { }

  ngOnInit() {
    this.loadClinics();
  }

  loadClinics() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // اضافه کردن فیلتر SieveModel اگر filterValue وجود دارد
    if (this.filterValue && this.filterValue.trim()) {
      const searchTerm = this.filterValue.trim();
      // جستجو در name, address, phone, email, cityName
      params.filters = `Name@=*${searchTerm}*|Address@=*${searchTerm}*|Phone@=*${searchTerm}*|Email@=*${searchTerm}*`;
    }

    this.clinicService.getAll(params).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.clinics = result.items;
          this.filteredClinics = [...this.clinics];
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.currentPage = result.page;
        } else {
          this.clinics = [];
          this.filteredClinics = [];
          this.totalCount = 0;
          this.totalPages = 0;
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

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadClinics();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadClinics();
  }

  openAddDialog() {
    this.router.navigate(['/panel/clinics/new']);
  }

  editClinic(clinic: Clinic) {
    if (clinic.id) {
      this.router.navigate(['/panel/clinics', clinic.id]);
    }
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
              // اگر صفحه فعلی خالی شد و صفحه قبلی وجود دارد، به صفحه قبلی برو
              if (this.clinics.length === 1 && this.currentPage > 1) {
                this.currentPage--;
              }
              this.loadClinics();
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


  applyFilter() {
    // بازنشانی به صفحه اول هنگام جستجو
    this.currentPage = 1;
    this.loadClinics();
  }

  openClinicUsersDialog(clinic: Clinic) {
    this.dialogService.open(ClinicUsersDialogComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: clinic
    }).subscribe(result => {
      // Dialog closed
    });
  }
}
