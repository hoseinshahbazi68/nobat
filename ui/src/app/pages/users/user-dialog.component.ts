import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { User, Gender } from '../../models/user.model';
import { RoleService } from '../../services/role.service';
import { Role } from '../../models/role.model';
import { ProvinceService } from '../../services/province.service';
import { CityService } from '../../services/city.service';
import { Province } from '../../models/province.model';
import { City } from '../../models/city.model';

@Component({
  selector: 'app-user-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.id ? 'ویرایش کاربر' : 'افزودن کاربر جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="userForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field">
              <label>کد ملی</label>
              <input type="text" formControlName="nationalCode" required>
              <span class="error" *ngIf="userForm.get('nationalCode')?.hasError('required') && userForm.get('nationalCode')?.touched">لطفا کد ملی را وارد نمایید</span>
            </div>

            <div class="form-field">
              <label>ایمیل</label>
              <input type="email" formControlName="email" required>
              <span class="error" *ngIf="userForm.get('email')?.hasError('required') && userForm.get('email')?.touched">لطفا ایمیل را وارد نمایید</span>
              <span class="error" *ngIf="userForm.get('email')?.hasError('email') && userForm.get('email')?.touched">ایمیل معتبر نیست</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>نام</label>
              <input type="text" formControlName="firstName" required>
              <span class="error" *ngIf="userForm.get('firstName')?.hasError('required') && userForm.get('firstName')?.touched">لطفا نام را وارد نمایید</span>
            </div>

            <div class="form-field">
              <label>نام خانوادگی</label>
              <input type="text" formControlName="lastName" required>
              <span class="error" *ngIf="userForm.get('lastName')?.hasError('required') && userForm.get('lastName')?.touched">لطفا نام خانوادگی را وارد نمایید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>شماره تلفن</label>
              <input type="text" formControlName="phoneNumber">
            </div>

            <div class="form-field">
              <label>جنسیت</label>
              <select formControlName="gender">
                <option [value]="null">انتخاب کنید</option>
                <option [value]="1">مرد</option>
                <option [value]="2">زن</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>تاریخ تولد</label>
              <input type="date" formControlName="birthDate">
            </div>

            <div class="form-field">
              <label>استان</label>
              <select formControlName="provinceId" (change)="onProvinceChange()">
                <option [value]="null">انتخاب کنید</option>
                <option *ngFor="let province of provinces" [value]="province.id">{{ province.name }}</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>شهر</label>
              <select formControlName="cityId" [disabled]="!selectedProvinceId">
                <option [value]="null">ابتدا استان را انتخاب کنید</option>
                <option *ngFor="let city of cities" [value]="city.id">{{ city.name }}</option>
              </select>
            </div>

            <div class="form-field">
              <label>رمز عبور</label>
              <input type="password" formControlName="password" [required]="!data?.id">
              <span class="hint" *ngIf="data?.id">برای تغییر رمز عبور، فیلد را پر کنید</span>
              <span class="error" *ngIf="userForm.get('password')?.hasError('required') && userForm.get('password')?.touched">لطفا رمز عبور را وارد نمایید</span>
              <span class="error" *ngIf="userForm.get('password')?.hasError('minlength') && userForm.get('password')?.touched">رمز عبور باید حداقل ۶ کاراکتر باشد</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>نقش‌ها</label>
              <div class="roles-container" *ngIf="availableRoles.length > 0">
                <label class="role-checkbox-label" *ngFor="let role of availableRoles">
                  <input
                    type="checkbox"
                    [value]="role.id"
                    [checked]="isRoleSelected(role.id!)"
                    (change)="toggleRole(role.id!, $event)">
                  <span>{{ role.name }}</span>
                </label>
              </div>
              <div class="loading-roles" *ngIf="isLoadingRoles">در حال بارگذاری نقش‌ها...</div>
              <span class="error" *ngIf="selectedRoleIds.length === 0 && userForm.get('roleIds')?.touched">لطفا حداقل یک نقش را انتخاب کنید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="checkbox-container">
              <label class="checkbox-label">
                <input type="checkbox" formControlName="isActive">
                <span>فعال</span>
              </label>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="userForm.invalid">
          {{ data?.id ? 'ذخیره تغییرات' : 'افزودن' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-form {
      min-width: 600px;
      padding: 1rem 0;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .checkbox-container {
      display: flex;
      align-items: center;
      padding-top: 0.5rem;
    }

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
      min-width: 600px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      margin-bottom: 1.5rem;
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

    .form-field input,
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
    .form-field select:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .form-field .error {
      color: #ef4444;
      font-size: 0.85rem;
      margin-top: 4px;
    }

    .form-field .hint {
      color: var(--text-muted);
      font-size: 0.85rem;
      margin-top: 4px;
    }

    .checkbox-container {
      display: flex;
      align-items: center;
      padding-top: 2rem;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      font-weight: 600;
    }

    .checkbox-label input[type="checkbox"] {
      width: 20px;
      height: 20px;
      cursor: pointer;
    }

    .full-width {
      grid-column: 1 / -1;
    }

    .roles-container {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      background: var(--bg-secondary);
      max-height: 200px;
      overflow-y: auto;
    }

    .role-checkbox-label {
      display: flex;
      align-items: center;
      gap: 12px;
      cursor: pointer;
      font-weight: 500;
      padding: 8px;
      border-radius: var(--radius-sm);
      transition: background var(--transition-base);
    }

    .role-checkbox-label:hover {
      background: var(--bg-tertiary);
    }

    .role-checkbox-label input[type="checkbox"] {
      width: 20px;
      height: 20px;
      cursor: pointer;
    }

    .loading-roles {
      padding: 16px;
      text-align: center;
      color: var(--text-muted);
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

    @media (max-width: 600px) {
      .dialog-form {
        min-width: auto;
      }

      .form-row {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class UserDialogComponent implements OnInit {
  userForm: FormGroup;
  availableRoles: Role[] = [];
  selectedRoleIds: number[] = [];
  isLoadingRoles = false;
  data: User | null = null;
  dialogRef: any = null;
  provinces: Province[] = [];
  cities: City[] = [];
  selectedProvinceId: number | null = null;
  Gender = Gender;

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private provinceService: ProvinceService,
    private cityService: CityService
  ) {
    this.userForm = this.fb.group({
      nationalCode: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: [''],
      gender: [null],
      birthDate: [''],
      provinceId: [null],
      cityId: [null],
      password: ['', Validators.minLength(6)],
      roleIds: [[], this.validateRoleIds],
      isActive: [true]
    });
  }

  validateRoleIds(control: any) {
    const roleIds = control.value;
    if (!roleIds || roleIds.length === 0) {
      return { required: true };
    }
    return null;
  }

  ngOnInit() {
    this.loadRoles();
    this.loadProvinces();

    if (this.data) {
      // تبدیل User model به فرمت فرم
      const formData: any = {
        nationalCode: this.data.nationalCode,
        email: this.data.email,
        firstName: this.data.firstName,
        lastName: this.data.lastName,
        phoneNumber: this.data.phoneNumber || '',
        gender: this.data.gender,
        birthDate: this.data.birthDate ? this.data.birthDate.split('T')[0] : '',
        cityId: this.data.cityId,
        isActive: this.data.isActive
      };

      // اگر شهر انتخاب شده، باید استان را هم پیدا کنیم
      if (this.data.cityId) {
        // ابتدا شهر را پیدا می‌کنیم تا provinceId را بگیریم
        this.cityService.getById(this.data.cityId).subscribe({
          next: (city) => {
            formData.provinceId = city.provinceId;
            this.selectedProvinceId = city.provinceId;
            this.loadCities(city.provinceId);
            // منتظر می‌مانیم تا شهرها بارگذاری شوند
            setTimeout(() => {
              this.userForm.patchValue(formData);
            }, 100);
          },
          error: () => {
            this.userForm.patchValue(formData);
          }
        });
      } else {
        this.userForm.patchValue(formData);
      }

      this.userForm.get('password')?.clearValidators();
      this.userForm.get('password')?.updateValueAndValidity();

      // بارگذاری نقش‌های کاربر بعد از بارگذاری نقش‌های موجود
      this.loadUserRoles();
    } else {
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
      this.userForm.get('password')?.updateValueAndValidity();
    }
  }

  loadProvinces() {
    this.provinceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.provinces = result.items;
      },
      error: (error) => {
        console.error('خطا در بارگذاری استان‌ها:', error);
      }
    });
  }

  onProvinceChange() {
    const provinceId = this.userForm.get('provinceId')?.value;
    this.selectedProvinceId = provinceId;
    this.userForm.patchValue({ cityId: null });

    if (provinceId) {
      this.loadCities(provinceId);
    } else {
      this.cities = [];
    }
  }

  loadCities(provinceId: number) {
    this.cityService.getByProvinceId(provinceId).subscribe({
      next: (cities) => {
        this.cities = cities;
      },
      error: (error) => {
        console.error('خطا در بارگذاری شهرها:', error);
        this.cities = [];
      }
    });
  }

  loadRoles() {
    this.isLoadingRoles = true;
    this.roleService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.availableRoles = result.items.filter(role => role.id !== undefined);
        this.isLoadingRoles = false;

        // اگر در حالت ویرایش هستیم، نقش‌های کاربر را بارگذاری می‌کنیم
        if (this.data) {
          this.loadUserRoles();
        }
      },
      error: (error) => {
        console.error('خطا در بارگذاری نقش‌ها:', error);
        this.isLoadingRoles = false;
      }
    });
  }

  loadUserRoles() {
    if (!this.data || !this.data.roles) {
      return;
    }

    // پیدا کردن شناسه نقش‌های کاربر بر اساس نام
    const userRoleNames = this.data.roles;
    const userRoleIds = this.availableRoles
      .filter(role => role.id && userRoleNames.includes(role.name))
      .map(role => role.id!);

    this.selectedRoleIds = userRoleIds;
    this.userForm.patchValue({ roleIds: userRoleIds });
  }

  isRoleSelected(roleId: number): boolean {
    return this.selectedRoleIds.includes(roleId);
  }

  toggleRole(roleId: number, event: any) {
    if (event.target.checked) {
      if (!this.selectedRoleIds.includes(roleId)) {
        this.selectedRoleIds.push(roleId);
      }
    } else {
      this.selectedRoleIds = this.selectedRoleIds.filter(id => id !== roleId);
    }

    this.userForm.patchValue({ roleIds: this.selectedRoleIds });
    this.userForm.get('roleIds')?.markAsTouched();
  }

  onSave() {
    // بررسی اینکه حداقل یک نقش انتخاب شده باشد
    if (this.selectedRoleIds.length === 0) {
      this.userForm.get('roleIds')?.markAsTouched();
      this.userForm.get('roleIds')?.setErrors({ required: true });
      return;
    }

    if (this.userForm.valid) {
      const formValue = { ...this.userForm.value };

      const userData: any = {
        nationalCode: formValue.nationalCode,
        email: formValue.email,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        phoneNumber: formValue.phoneNumber || undefined,
        gender: formValue.gender ? parseInt(formValue.gender) : undefined,
        birthDate: formValue.birthDate || undefined,
        cityId: formValue.cityId || undefined,
        isActive: formValue.isActive,
        roleIds: this.selectedRoleIds
      };

      // اگر رمز عبور خالی است و در حالت ویرایش هستیم، آن را حذف می‌کنیم
      if (formValue.password) {
        userData.password = formValue.password;
      }

      if (this.data?.id) {
        userData.id = this.data.id;
      }

      if (this.dialogRef) {
        this.dialogRef.close(userData);
      }
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
