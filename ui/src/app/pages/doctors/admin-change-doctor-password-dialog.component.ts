import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { DoctorListDto } from '../../models/doctor.model';
import { SnackbarService } from '../../services/snackbar.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-admin-change-doctor-password-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>تغییر کلمه عبور پزشک: {{ doctor?.firstName }} {{ doctor?.lastName }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="changePasswordForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field full-width">
              <label>کلمه عبور جدید</label>
              <input
                type="password"
                formControlName="newPassword"
                required
                placeholder="کلمه عبور جدید را وارد کنید">
              <span class="error" *ngIf="changePasswordForm.get('newPassword')?.hasError('required') && changePasswordForm.get('newPassword')?.touched">
                لطفا کلمه عبور جدید را وارد نمایید
              </span>
              <span class="error" *ngIf="changePasswordForm.get('newPassword')?.hasError('minlength') && changePasswordForm.get('newPassword')?.touched">
                کلمه عبور باید حداقل ۶ کاراکتر باشد
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>تکرار کلمه عبور جدید</label>
              <input
                type="password"
                formControlName="confirmPassword"
                required
                placeholder="کلمه عبور جدید را دوباره وارد کنید">
              <span class="error" *ngIf="changePasswordForm.get('confirmPassword')?.hasError('required') && changePasswordForm.get('confirmPassword')?.touched">
                لطفا تکرار کلمه عبور را وارد نمایید
              </span>
              <span class="error" *ngIf="changePasswordForm.hasError('passwordMismatch') && changePasswordForm.get('confirmPassword')?.touched">
                کلمه عبور جدید و تکرار آن یکسان نیستند
              </span>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()" [disabled]="isLoading">
          انصراف
        </button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="isSaveDisabled">
          <span *ngIf="!isLoading">ذخیره</span>
          <span *ngIf="isLoading">در حال ذخیره...</span>
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
      grid-template-columns: 1fr;
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

    .form-field input {
      padding: 12px 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: var(--bg-secondary);
      transition: all var(--transition-base);
    }

    .form-field input:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .form-field .error {
      color: #ef4444;
      font-size: 0.85rem;
      margin-top: 4px;
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

    .btn-secondary:hover:not(:disabled) {
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
export class AdminChangeDoctorPasswordDialogComponent implements OnInit {
  changePasswordForm: FormGroup;
  dialogRef: any = null;
  doctor: DoctorListDto | null = null;
  userId: number | null = null;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private snackbarService: SnackbarService
  ) {
    this.changePasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    // DialogService sets doctor and userId properties directly, so they should be available here
    // Use setTimeout to ensure properties are set before checking (in case of timing issues)
    setTimeout(() => {
      if (!this.doctor) {
        console.warn('Doctor not set in AdminChangeDoctorPasswordDialogComponent', this.doctor);
      }
      // Fallback: if userId is not set by DialogService, try to get it from doctor object
      if (!this.userId && this.doctor?.userId) {
        this.userId = this.doctor.userId;
      }
      if (!this.userId) {
        console.warn('UserId not set in AdminChangeDoctorPasswordDialogComponent', {
          userId: this.userId,
          doctor: this.doctor,
          doctorUserId: this.doctor?.userId
        });
      }
    }, 0);

    // Update validation when password fields change
    this.changePasswordForm.get('newPassword')?.valueChanges.subscribe(() => {
      this.changePasswordForm.get('confirmPassword')?.updateValueAndValidity();
    });

    this.changePasswordForm.get('confirmPassword')?.valueChanges.subscribe(() => {
      this.changePasswordForm.updateValueAndValidity();
    });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const form = control as FormGroup;
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');

    if (!newPassword || !confirmPassword) {
      return null;
    }

    // اگر هر دو فیلد خالی باشند، خطایی نیست (required validator این را بررسی می‌کند)
    if (!newPassword.value || !confirmPassword.value) {
      // اگر یکی پر شده و دیگری خالی است، خطا نده (required validator این را بررسی می‌کند)
      return null;
    }

    // اگر مقادیر یکسان نیستند، خطا برگردان
    if (newPassword.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }

    return null;
  }

  get isSaveDisabled(): boolean {
    return this.changePasswordForm.invalid || this.isLoading || !this.userId;
  }

  onSave() {
    if (!this.userId) {
      this.snackbarService.error('شناسه کاربر یافت نشد', 'بستن', 5000);
      return;
    }

    if (this.changePasswordForm.valid && !this.isLoading) {
      this.isLoading = true;
      const formValue = this.changePasswordForm.value;

      // Call API to reset user password
      this.userService.changeUserPassword(this.userId, formValue.newPassword).subscribe({
        next: () => {
          this.snackbarService.success('کلمه عبور پزشک با موفقیت تغییر کرد', 'بستن', 3000);
          this.isLoading = false;
          if (this.dialogRef) {
            this.dialogRef.close(true);
          }
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در تغییر کلمه عبور';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isLoading = false;
        }
      });
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
