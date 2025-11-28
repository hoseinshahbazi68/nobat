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
  filterValue: string = '';
  isLoading = false;

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
    this.insuranceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.insurances = result.items;
        this.filteredInsurances = [...this.insurances];
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری بیمه‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
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
              this.filteredInsurances = [...this.insurances];
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
            this.filteredInsurances = [...this.insurances];
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
          this.filteredInsurances = [...this.insurances];
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
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredInsurances = this.insurances.filter(insurance =>
      insurance.name.toLowerCase().includes(filter) ||
      insurance.code.toLowerCase().includes(filter)
    );
  }
}
