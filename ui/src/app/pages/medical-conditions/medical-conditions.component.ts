import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { MedicalConditionService } from '../../services/medical-condition.service';
import { MedicalConditionDialogComponent } from './medical-condition-dialog.component';
import { MedicalCondition } from '../../models/medical-condition.model';

@Component({
  selector: 'app-medical-conditions',
  templateUrl: './medical-conditions.component.html',
  styleUrls: ['./medical-conditions.component.scss']
})
export class MedicalConditionsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'description', 'actions'];
  medicalConditions: MedicalCondition[] = [];
  filteredMedicalConditions: MedicalCondition[] = [];
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
    private medicalConditionService: MedicalConditionService
  ) {}

  ngOnInit() {
    this.loadMedicalConditions();
  }

  loadMedicalConditions() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // Add filter if filterValue exists
    if (this.filterValue && this.filterValue.trim()) {
      params.filters = `name@=*${this.filterValue.trim()}*`;
    }

    this.medicalConditionService.getAll(params).subscribe({
      next: (result) => {
        this.medicalConditions = result.items;
        this.filteredMedicalConditions = [...this.medicalConditions];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error?.message || error?.error?.message || 'خطا در بارگذاری علائم پزشکی';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadMedicalConditions();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadMedicalConditions();
  }

  openAddDialog() {
    this.dialogService.open(MedicalConditionDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result && typeof result === 'object' && result.name) {
        this.saveMedicalCondition(result);
      }
    });
  }

  editMedicalCondition(medicalCondition: MedicalCondition) {
    this.dialogService.open(MedicalConditionDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: medicalCondition
    }).subscribe(result => {
      if (result) {
        this.saveMedicalCondition(result, medicalCondition.id);
      }
    });
  }

  deleteMedicalCondition(medicalCondition: MedicalCondition) {
    if (medicalCondition.id) {
      this.dialogService.confirm({
        title: 'حذف علائم پزشکی',
        message: `آیا از حذف علائم پزشکی "${medicalCondition.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.medicalConditionService.delete(medicalCondition.id!).subscribe({
            next: () => {
              this.medicalConditions = this.medicalConditions.filter(mc => mc.id !== medicalCondition.id);
              this.filteredMedicalConditions = [...this.medicalConditions];
              this.applyFilter();
              this.snackbarService.success('علائم پزشکی با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error?.message || error?.error?.message || 'خطا در حذف علائم پزشکی';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveMedicalCondition(medicalConditionData: any, id?: number) {
    if (!medicalConditionData || !medicalConditionData.name) {
      this.snackbarService.error('لطفا نام علائم پزشکی را وارد نمایید', 'بستن', 3000);
      return;
    }

    if (id) {
      const updateData = { ...medicalConditionData, id };
      this.medicalConditionService.update(updateData).subscribe({
        next: (updatedMedicalCondition) => {
          const index = this.medicalConditions.findIndex(mc => mc.id === id);
          if (index !== -1) {
            this.medicalConditions[index] = updatedMedicalCondition;
            this.filteredMedicalConditions = [...this.medicalConditions];
            this.applyFilter();
          } else {
            this.loadMedicalConditions();
          }
          this.snackbarService.success('علائم پزشکی با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در ویرایش علائم پزشکی';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.medicalConditionService.create(medicalConditionData).subscribe({
        next: (newMedicalCondition) => {
          this.medicalConditions = [...this.medicalConditions, newMedicalCondition];
          this.filteredMedicalConditions = [...this.medicalConditions];
          this.applyFilter();
          this.snackbarService.success('علائم پزشکی با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در افزودن علائم پزشکی';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadMedicalConditions();
  }
}
