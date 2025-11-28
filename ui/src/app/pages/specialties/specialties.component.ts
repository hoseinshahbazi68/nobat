import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { SpecialtyService } from '../../services/specialty.service';
import { SpecialtyDialogComponent } from './specialty-dialog.component';
import { SpecialtyMedicalConditionsDialogComponent } from './specialty-medical-conditions-dialog.component';
import { Specialty } from '../../models/specialty.model';

@Component({
  selector: 'app-specialties',
  templateUrl: './specialties.component.html',
  styleUrls: ['./specialties.component.scss']
})
export class SpecialtiesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'description', 'actions'];
  specialties: Specialty[] = [];
  filteredSpecialties: Specialty[] = [];
  filterValue: string = '';
  isLoading = false;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private specialtyService: SpecialtyService
  ) {}

  ngOnInit() {
    this.loadSpecialties();
  }

  loadSpecialties() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // Add filter if exists
    if (this.filterValue && this.filterValue.trim()) {
      const searchTerm = this.filterValue.trim();
      // جستجو در name و description
      params.filters = `Name@=*${searchTerm}*|Description@=*${searchTerm}*`;
    }

    this.specialtyService.getAll(params).subscribe({
      next: (result) => {
        this.specialties = result.items;
        this.filteredSpecialties = [...this.specialties];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error?.message || error?.error?.message || 'خطا در بارگذاری تخصص‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadSpecialties();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadSpecialties();
  }

  openAddDialog() {
    this.dialogService.open(SpecialtyDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result && typeof result === 'object' && result.name) {
        this.saveSpecialty(result);
      }
    });
  }

  editSpecialty(specialty: Specialty) {
    this.dialogService.open(SpecialtyDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: specialty
    }).subscribe(result => {
      if (result) {
        this.saveSpecialty(result, specialty.id);
      }
    });
  }

  deleteSpecialty(specialty: Specialty) {
    if (specialty.id) {
      this.dialogService.confirm({
        title: 'حذف تخصص',
        message: `آیا از حذف تخصص "${specialty.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.specialtyService.delete(specialty.id!).subscribe({
            next: () => {
              this.specialties = this.specialties.filter(s => s.id !== specialty.id);
              this.filteredSpecialties = [...this.specialties];
              this.applyFilter();
              this.snackbarService.success('تخصص با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error?.message || error?.error?.message || 'خطا در حذف تخصص';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveSpecialty(specialtyData: any, id?: number) {
    if (!specialtyData || !specialtyData.name) {
      this.snackbarService.error('لطفا نام تخصص را وارد نمایید', 'بستن', 3000);
      return;
    }

    if (id) {
      const updateData = { ...specialtyData, id };
      this.specialtyService.update(updateData).subscribe({
        next: (updatedSpecialty) => {
          const index = this.specialties.findIndex(s => s.id === id);
          if (index !== -1) {
            this.specialties[index] = updatedSpecialty;
            this.filteredSpecialties = [...this.specialties];
            this.applyFilter();
          } else {
            this.loadSpecialties();
          }
          this.snackbarService.success('تخصص با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در ویرایش تخصص';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.specialtyService.create(specialtyData).subscribe({
        next: (newSpecialty) => {
          this.specialties = [...this.specialties, newSpecialty];
          this.filteredSpecialties = [...this.specialties];
          this.applyFilter();
          this.snackbarService.success('تخصص با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در افزودن تخصص';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadSpecialties();
  }

  openMedicalConditionsDialog(specialty: Specialty) {
    this.dialogService.open(SpecialtyMedicalConditionsDialogComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: specialty
    }).subscribe(result => {
      // Dialog closed, no action needed
    });
  }
}
