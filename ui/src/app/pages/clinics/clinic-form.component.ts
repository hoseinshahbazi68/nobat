import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { Clinic } from '../../models/clinic.model';
import { City } from '../../models/city.model';
import { CityService } from '../../services/city.service';
import { ClinicService } from '../../services/clinic.service';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-clinic-form',
  templateUrl: './clinic-form.component.html',
  styleUrls: ['./clinic-form.component.scss']
})
export class ClinicFormComponent implements OnInit, OnDestroy {
  clinicForm: FormGroup;
  isEditMode = false;
  clinicId: number | null = null;
  cities: City[] = [];
  filteredCities: City[] = [];
  citySearchText: string = '';
  showCityDropdown: boolean = false;
  isLoading = false;
  isSearchingCities: boolean = false;
  private citySearchSubject = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private cityService: CityService,
    private clinicService: ClinicService,
    private snackbarService: SnackbarService
  ) {
    this.clinicForm = this.fb.group({
      name: ['', Validators.required],
      address: [''],
      phone: [''],
      email: ['', Validators.email],
      cityId: [null],
      appointmentGenerationDays: [null, [Validators.min(0)]],
      isActive: [true]
    });
  }

  ngOnInit() {
    this.setupCitySearch();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.clinicId = +id;
      this.loadClinic(+id);
    }
  }

  setupCitySearch() {
    this.citySearchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(searchTerm => {
        this.isSearchingCities = true;
        const searchText = searchTerm?.trim() || '';
        if (searchText) {
          return this.cityService.getAll({
            page: 1,
            pageSize: 100,
            filters: `name.Contains("${searchText}") || provinceName.Contains("${searchText}")`
          }).pipe(
            catchError(error => {
              this.isSearchingCities = false;
              console.error('خطا در جستجوی شهرها:', error);
              return [];
            })
          );
        } else {
          return this.cityService.getAll({ page: 1, pageSize: 100 }).pipe(
            catchError(error => {
              this.isSearchingCities = false;
              console.error('خطا در بارگذاری شهرها:', error);
              return [];
            })
          );
        }
      })
    ).subscribe({
      next: (result) => {
        if (result && 'items' in result) {
          this.filteredCities = result.items || [];
        } else if (Array.isArray(result)) {
          this.filteredCities = result;
        } else {
          this.filteredCities = [];
        }
        this.isSearchingCities = false;
      }
    });
  }

  loadClinic(id: number) {
    this.isLoading = true;
    this.clinicService.getById(id).subscribe({
      next: (clinic) => {
        if (clinic) {
          this.clinicForm.patchValue(clinic);
          // تنظیم متن جستجوی شهر - باید بعد از بارگذاری شهرها انجام شود
          if (clinic.cityId) {
            if (this.cities.length > 0) {
              this.setCitySearchText(clinic.cityId);
            } else {
              // اگر شهرها هنوز لود نشده‌اند، منتظر بمان
              setTimeout(() => {
                this.setCitySearchText(clinic.cityId!);
              }, 500);
            }
          }
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری کلینیک';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
        this.router.navigate(['/panel/clinics']);
      }
    });
  }

  private setCitySearchText(cityId: number) {
    const selectedCity = this.cities.find(c => c.id === cityId);
    if (selectedCity) {
      this.citySearchText = `${selectedCity.name}${selectedCity.provinceName ? ' - ' + selectedCity.provinceName : ''}`;
    }
  }

  onCitySearch() {
    this.citySearchSubject.next(this.citySearchText);
    this.showCityDropdown = true;
  }

  selectCity(city: City) {
    this.clinicForm.patchValue({ cityId: city.id });
    this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
    this.showCityDropdown = false;
  }

  onCityInputFocus() {
    this.showCityDropdown = true;
    if (!this.citySearchText || !this.citySearchText.trim()) {
      this.citySearchSubject.next('');
    }
  }

  onCityInputBlur() {
    setTimeout(() => {
      this.showCityDropdown = false;
    }, 200);
  }

  saveClinic() {
    if (this.clinicForm.invalid) {
      this.clinicForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.clinicForm.value;

    if (this.isEditMode && this.clinicId) {
      const updateData = { ...formValue, id: this.clinicId };
      this.clinicService.update(updateData).subscribe({
        next: () => {
          this.snackbarService.success('کلینیک با موفقیت ویرایش شد', 'بستن', 3000);
          this.router.navigate(['/panel/clinics']);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش کلینیک';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isLoading = false;
        }
      });
    } else {
      this.clinicService.create(formValue).subscribe({
        next: () => {
          this.snackbarService.success('کلینیک با موفقیت اضافه شد', 'بستن', 3000);
          this.router.navigate(['/panel/clinics']);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن کلینیک';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isLoading = false;
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['/panel/clinics']);
  }

  ngOnDestroy() {
    this.citySearchSubject.complete();
  }
}
