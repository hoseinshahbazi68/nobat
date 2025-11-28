import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { InsuranceService } from '../../services/insurance.service';
import { InsuranceDialogComponent } from './insurance-dialog.component';
import { Insurance } from '../../models/insurance.model';

@Component({
  selector: 'app-insurances',
  templateUrl: './insurances.component.html',
  styleUrls: ['./insurances.component.scss']
})
export class InsurancesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'code', 'isActive', 'actions'];
  insurances: Insurance[] = [];
  filteredInsurances: Insurance[] = [];
  paginatedInsurances: Insurance[] = [];
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
    private insuranceService: InsuranceService
  ) {}

  ngOnInit() {
    this.loadInsurances();
  }

  loadInsurances() {
    this.isLoading = true;
    this.insuranceService.getAll({ page: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.insurances = result.items;
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری بیمه‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  updatePagination() {
    this.totalCount = this.filteredInsurances.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize) || 1;

    // Ensure currentPage is within valid range
    if (this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages || 1;
    }

    // Calculate paginated items
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedInsurances = this.filteredInsurances.slice(startIndex, endIndex);
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.updatePagination();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.updatePagination();
  }

  openAddDialog() {
    this.dialogService.open(InsuranceDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result) {
        this.saveInsurance(result);
      }
    });
  }

  editInsurance(insurance: Insurance) {
    this.dialogService.open(InsuranceDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: insurance
    }).subscribe(result => {
      if (result) {
        this.saveInsurance(result, insurance.id);
      }
    });
  }

  deleteInsurance(insurance: Insurance) {
    if (insurance.id) {
      this.dialogService.confirm({
        title: 'حذف بیمه',
        message: `آیا از حذف بیمه "${insurance.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.insuranceService.delete(insurance.id!).subscribe({
            next: () => {
              this.insurances = this.insurances.filter(i => i.id !== insurance.id);
              this.applyFilter();
              this.snackbarService.success('بیمه با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف بیمه';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveInsurance(insuranceData: any, id?: number) {
    if (id) {
      const updateData = { ...insuranceData, id };
      this.insuranceService.update(updateData).subscribe({
        next: (updatedInsurance) => {
          const index = this.insurances.findIndex(i => i.id === id);
          if (index !== -1) {
            this.insurances[index] = updatedInsurance;
            this.applyFilter();
          }
          this.snackbarService.success('بیمه با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش بیمه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.insuranceService.create(insuranceData).subscribe({
        next: (newInsurance) => {
          this.insurances = [...this.insurances, newInsurance];
          this.applyFilter();
          this.snackbarService.success('بیمه با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن بیمه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredInsurances = [...this.insurances];
    } else {
      const filter = this.filterValue.trim().toLowerCase();
      this.filteredInsurances = this.insurances.filter(insurance =>
        insurance.name.toLowerCase().includes(filter) ||
        insurance.code.toLowerCase().includes(filter)
      );
    }
    // Reset to first page when filtering
    this.currentPage = 1;
    this.updatePagination();
  }
}
