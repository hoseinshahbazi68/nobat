import { Component, OnInit } from '@angular/core';
import { City } from '../../models/city.model';
import { Province } from '../../models/province.model';
import { DialogService } from '../../services/dialog.service';
import { CityService } from '../../services/city.service';
import { ProvinceService } from '../../services/province.service';
import { SnackbarService } from '../../services/snackbar.service';
import { CityDialogComponent } from './city-dialog.component';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.scss']
})
export class CitiesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'provinceName', 'code', 'actions'];
  cities: City[] = [];
  filteredCities: City[] = [];
  provinces: Province[] = [];
  selectedProvinceId: number | null = null;
  filterValue: string = '';
  isLoading = false;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private cityService: CityService,
    private provinceService: ProvinceService
  ) { }

  ngOnInit() {
    this.loadProvinces();
    this.loadCities();
  }

  loadProvinces() {
    this.provinceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.provinces = result.items;
        }
      }
    });
  }

  loadCities() {
    this.isLoading = true;
    const params: any = { page: 1, pageSize: 100 };
    if (this.selectedProvinceId) {
      params.provinceId = this.selectedProvinceId;
    }
    this.cityService.getAll(params).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.cities = result.items;
          this.filteredCities = [...this.cities];
        } else {
          this.cities = [];
          this.filteredCities = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری شهرها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  filterByProvince() {
    this.loadCities();
  }

  openAddDialog() {
    this.dialogService.open(CityDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: { city: null, provinces: this.provinces }
    }).subscribe(result => {
      if (result) {
        this.saveCity(result);
      }
    });
  }

  editCity(city: City) {
    this.dialogService.open(CityDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: { city: city, provinces: this.provinces }
    }).subscribe(result => {
      if (result) {
        this.saveCity(result, city.id);
      }
    });
  }

  deleteCity(city: City) {
    if (city.id) {
      this.dialogService.confirm({
        title: 'حذف شهر',
        message: `آیا از حذف شهر "${city.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.cityService.delete(city.id!).subscribe({
            next: () => {
              this.cities = this.cities.filter(c => c.id !== city.id);
              this.filteredCities = [...this.cities];
              this.applyFilter();
              this.snackbarService.success('شهر با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف شهر';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveCity(cityData: any, id?: number) {
    if (id) {
      const updateData = { ...cityData, id };
      this.cityService.update(updateData).subscribe({
        next: () => {
          this.snackbarService.success('شهر با موفقیت ویرایش شد', 'بستن', 3000);
          this.loadCities();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش شهر';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.cityService.create(cityData).subscribe({
        next: () => {
          this.snackbarService.success('شهر با موفقیت اضافه شد', 'بستن', 3000);
          this.loadCities();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن شهر';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredCities = [...this.cities];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredCities = this.cities.filter(city =>
      city.name.toLowerCase().includes(filter) ||
      (city.provinceName && city.provinceName.toLowerCase().includes(filter)) ||
      (city.code && city.code.toLowerCase().includes(filter))
    );
  }
}
