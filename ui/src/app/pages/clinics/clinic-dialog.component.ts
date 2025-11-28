import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { Clinic } from '../../models/clinic.model';
import { City } from '../../models/city.model';
import { CityService } from '../../services/city.service';

@Component({
  selector: 'app-clinic-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.id ? 'ویرایش کلینیک' : 'افزودن کلینیک جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="clinicForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field full-width">
              <label>نام کلینیک <span class="required">*</span></label>
              <input type="text" formControlName="name" required>
              <span class="error" *ngIf="clinicForm.get('name')?.hasError('required') && clinicForm.get('name')?.touched">لطفا نام کلینیک را وارد نمایید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>آدرس</label>
              <textarea formControlName="address" rows="3"></textarea>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>شماره تلفن</label>
              <input type="tel" formControlName="phone">
            </div>
            <div class="form-field">
              <label>ایمیل</label>
              <input type="email" formControlName="email">
              <span class="error" *ngIf="clinicForm.get('email')?.hasError('email') && clinicForm.get('email')?.touched">فرمت ایمیل صحیح نیست</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>تعداد روز تولید نوبت‌دهی</label>
              <input type="number" formControlName="appointmentGenerationDays" min="0" placeholder="مثال: 30">
              <span class="error" *ngIf="clinicForm.get('appointmentGenerationDays')?.hasError('min') && clinicForm.get('appointmentGenerationDays')?.touched">تعداد روز باید عدد مثبت باشد</span>
            </div>
            <div class="form-field  ">
              <label>شهر</label>
              <div class="city-search-container">
                <input type="text" [(ngModel)]="citySearchText" [ngModelOptions]="{standalone: true}"
                  (input)="onCitySearch()" (focus)="onCityInputFocus()" (blur)="onCityInputBlur()"
                  placeholder="جستجو و انتخاب شهر..." class="city-search-input">
                <span class="search-icon">🔍</span>
                <div class="city-dropdown" *ngIf="showCityDropdown && filteredCities.length > 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="city-item" *ngFor="let city of filteredCities" (click)="selectCity(city)">
                    <span class="city-name">{{ city.name }}</span>
                    <span class="province-name" *ngIf="city.provinceName">{{ city.provinceName }}</span>
                  </div>
                </div>
                <div class="city-dropdown empty"
                  *ngIf="showCityDropdown && filteredCities.length === 0 && cities.length > 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="city-item">شهری یافت نشد</div>
                </div>
                <div class="city-dropdown empty" *ngIf="showCityDropdown && cities.length === 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="city-item">در حال بارگذاری شهرها...</div>
                </div>
              </div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label class="checkbox-label">
                <input type="checkbox" formControlName="isActive">
                <span>کلینیک فعال است</span>
              </label>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="clinicForm.invalid">
          {{ data?.id ? 'ذخیره تغییرات' : 'افزودن' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .dialog-header {
      padding: 24px;
      border-bottom: 1px solid var(--border-light);
    }

    .dialog-header h2 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 800;
      background: var(--gradient-primary);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .dialog-content {
      padding: 24px;
      flex: 1;
      overflow-y: auto;
    }

    .dialog-form {
      min-width: 500px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      margin-bottom: 1.5rem;
    }

    .full-width {
      grid-column: 1 / -1;
    }

    .form-field {
      display: flex;
      flex-direction: column;
    }

    .form-field label {
      margin-bottom: 8px;
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.9rem;
    }

    .required {
      color: #ef4444;
    }

    .form-field input,
    .form-field textarea,
    .form-field select {
      padding: 12px 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: var(--bg-secondary);
      transition: all var(--transition-base);
    }

    .form-field input:focus,
    .form-field textarea:focus,
    .form-field select:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .city-search-container {
      position: relative;

      .city-search-input {
        padding-right: 40px;
      }

      .search-icon {
        position: absolute;
        right: 12px;
        top: 50%;
        transform: translateY(-50%);
        pointer-events: none;
        font-size: 1rem;
        color: var(--text-muted);
      }

      .city-dropdown {
        position: absolute;
        top: 100%;
        left: 0;
        right: 0;
        background: var(--bg-secondary);
        border: 2px solid var(--border-color);
        border-radius: var(--radius-md);
        margin-top: 4px;
        max-height: 300px;
        overflow-y: auto;
        z-index: 1000;
        box-shadow: var(--shadow-lg);

        &.empty {
          padding: 12px;
          text-align: center;
          color: var(--text-muted);
        }

        .city-item {
          padding: 12px 16px;
          cursor: pointer;
          display: flex;
          justify-content: space-between;
          align-items: center;
          transition: background var(--transition-base);
          border-bottom: 1px solid var(--border-light);

          &:last-child {
            border-bottom: none;
          }

          &:hover {
            background: var(--bg-tertiary);
          }

          .city-name {
            font-weight: 600;
            color: var(--text-primary);
          }

          .province-name {
            font-size: 0.85rem;
            color: var(--text-muted);
            margin-right: 8px;
          }
        }
      }
    }

    .form-field textarea {
      resize: vertical;
      min-height: 80px;
    }

    .form-field .error {
      color: #ef4444;
      font-size: 0.85rem;
      margin-top: 4px;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      user-select: none;
    }

    .checkbox-label input[type="checkbox"] {
      width: 20px;
      height: 20px;
      cursor: pointer;
      margin: 0;
    }

    .checkbox-label span {
      font-weight: 500;
      color: var(--text-primary);
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--border-light);
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      padding: 12px 24px;
      border: none;
      border-radius: var(--radius-md);
      font-weight: 700;
      font-size: 0.95rem;
      cursor: pointer;
      transition: all var(--transition-base);
      font-family: 'Vazirmatn', sans-serif;
    }

    .btn-primary {
      background: var(--gradient-primary);
      color: white;
      box-shadow: 0 4px 12px rgba(6, 182, 212, 0.3);
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(6, 182, 212, 0.4);
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
      border: 2px solid var(--border-color);
    }

    .btn-secondary:hover {
      background: var(--bg-secondary);
      border-color: var(--primary);
    }

    @media (max-width: 600px) {
      .dialog-form {
        min-width: auto;
      }

      .form-row {
        grid-template-columns: 1fr;
        gap: 1rem;
      }
    }
  `]
})
export class ClinicDialogComponent implements OnInit, OnDestroy {
  clinicForm: FormGroup;
  data: Clinic | null = null;
  dialogRef: any = null;
  cities: City[] = [];
  filteredCities: City[] = [];
  citySearchText: string = '';
  showCityDropdown: boolean = false;
  isSearchingCities: boolean = false;
  private citySearchSubject = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private cityService: CityService
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
    if (this.data) {
      this.clinicForm.patchValue(this.data);
    }
    this.setupCitySearch();
    // اگر شهر انتخاب شده است، متن جستجو را تنظیم کن
    const cityId = this.data?.cityId || this.clinicForm.get('cityId')?.value;
    if (cityId) {
      this.cityService.getById(cityId).subscribe({
        next: (city) => {
          this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
        }
      });
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

  onSave() {
    if (this.clinicForm.valid) {
      const formValue = this.clinicForm.value;
      if (this.data?.id) {
        formValue.id = this.data.id;
      }
      if (this.dialogRef) {
        this.dialogRef.close(formValue);
      }
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }

  ngOnDestroy() {
    this.citySearchSubject.complete();
  }
}
