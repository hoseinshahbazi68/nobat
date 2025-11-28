import { Component, OnInit } from '@angular/core';
import { Province } from '../../models/province.model';
import { DialogService } from '../../services/dialog.service';
import { ProvinceService } from '../../services/province.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ProvinceDialogComponent } from './province-dialog.component';

@Component({
  selector: 'app-provinces',
  templateUrl: './provinces.component.html',
  styleUrls: ['./provinces.component.scss']
})
export class ProvincesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'code', 'actions'];
  provinces: Province[] = [];
  filteredProvinces: Province[] = [];
  filterValue: string = '';
  isLoading = false;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private provinceService: ProvinceService
  ) { }

  ngOnInit() {
    this.loadProvinces();
  }

  loadProvinces() {
    this.isLoading = true;
    this.provinceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.provinces = result.items;
          this.filteredProvinces = [...this.provinces];
        } else {
          this.provinces = [];
          this.filteredProvinces = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری استان‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  openAddDialog() {
    this.dialogService.open(ProvinceDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result) {
        this.saveProvince(result);
      }
    });
  }

  editProvince(province: Province) {
    this.dialogService.open(ProvinceDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: province
    }).subscribe(result => {
      if (result) {
        this.saveProvince(result, province.id);
      }
    });
  }

  deleteProvince(province: Province) {
    if (province.id) {
      this.dialogService.confirm({
        title: 'حذف استان',
        message: `آیا از حذف استان "${province.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.provinceService.delete(province.id!).subscribe({
            next: () => {
              this.provinces = this.provinces.filter(p => p.id !== province.id);
              this.filteredProvinces = [...this.provinces];
              this.applyFilter();
              this.snackbarService.success('استان با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف استان';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveProvince(provinceData: any, id?: number) {
    if (id) {
      const updateData = { ...provinceData, id };
      this.provinceService.update(updateData).subscribe({
        next: () => {
          this.snackbarService.success('استان با موفقیت ویرایش شد', 'بستن', 3000);
          this.loadProvinces();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش استان';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.provinceService.create(provinceData).subscribe({
        next: () => {
          this.snackbarService.success('استان با موفقیت اضافه شد', 'بستن', 3000);
          this.loadProvinces();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن استان';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredProvinces = [...this.provinces];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredProvinces = this.provinces.filter(province =>
      province.name.toLowerCase().includes(filter) ||
      (province.code && province.code.toLowerCase().includes(filter))
    );
  }
}
