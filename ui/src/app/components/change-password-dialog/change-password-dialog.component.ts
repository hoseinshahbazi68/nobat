import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ChangePasswordRequest } from '../../models/user.model';

@Component({
  selector: 'app-change-password-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>ویرایش رمز عبور</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="changePasswordForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field full-width">
              <label>رمز عبور فعلی</label>
              <input
                type="password"
                formControlName="currentPassword"
                required
                placeholder="رمز عبور فعلی خود را وارد کنید">
              <span class="error" *ngIf="changePasswordForm.get('currentPassword')?.hasError('required') && changePasswordForm.get('currentPassword')?.touched">
                لطفا رمز عبور فعلی را وارد نمایید
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>رمز عبور جدید</label>
              <input
                type="password"
                formControlName="newPassword"
                required
                placeholder="رمز عبور جدید خود را وارد کنید">
              <span class="error" *ngIf="changePasswordForm.get('newPassword')?.hasError('required') && changePasswordForm.get('newPassword')?.touched">
                لطفا رمز عبور جدید را وارد نمایید
              </span>
              <span class="error" *ngIf="changePasswordForm.get('newPassword')?.hasError('minlength') && changePasswordForm.get('newPassword')?.touched">
                رمز عبور باید حداقل ۶ کاراکتر باشد
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>تکرار رمز عبور جدید</label>
              <input
                type="password"
                formControlName="confirmPassword"
                required
                placeholder="رمز عبور جدید را دوباره وارد کنید">
              <span class="error" *ngIf="changePasswordForm.get('confirmPassword')?.hasError('required') && changePasswordForm.get('confirmPassword')?.touched">
                لطفا تکرار رمز عبور را وارد نمایید
              </span>
              <span class="error" *ngIf="changePasswordForm.hasError('passwordMismatch') && changePasswordForm.get('confirmPassword')?.touched">
                رمز عبور جدید و تکرار آن یکسان نیستند
              </span>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()" [disabled]="isLoading">
          انصراف
        </button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="changePasswordForm.invalid || isLoading">
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
export class ChangePasswordDialogComponent implements OnInit {
  changePasswordForm: FormGroup;
  dialogRef: any = null;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackbarService: SnackbarService
  ) {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    // Component initialized
  }

  passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');

    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  onSave() {
    if (this.changePasswordForm.valid && !this.isLoading) {
      this.isLoading = true;
      const formValue = this.changePasswordForm.value;

      const changePasswordData: ChangePasswordRequest = {
        currentPassword: formValue.currentPassword,
        newPassword: formValue.newPassword,
        confirmPassword: formValue.confirmPassword
      };

      this.authService.changePassword(changePasswordData).subscribe({
        next: () => {
          this.snackbarService.success('رمز عبور با موفقیت تغییر کرد', 'بستن', 3000);
          this.isLoading = false;
          if (this.dialogRef) {
            this.dialogRef.close(true);
          }
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در تغییر رمز عبور';
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
