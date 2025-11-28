import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SnackbarService } from '../../../services/snackbar.service';
import { AuthService } from '../../../services/auth.service';
import { LoginRequest } from '../../../models/auth.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;

  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private snackbarService: SnackbarService,
    private authService: AuthService
  ) {
    this.loginForm = this.fb.group({
      nationalCode: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngOnInit() {
    // بارگذاری اطلاعات ذخیره شده در صورت وجود
    const savedCredentials = this.getSavedCredentials();
    if (savedCredentials) {
      this.loginForm.patchValue({
        nationalCode: savedCredentials.nationalCode,
        password: savedCredentials.password,
        rememberMe: true
      });
    }
  }

  private getSavedCredentials(): { nationalCode: string; password: string } | null {
    try {
      const saved = localStorage.getItem('rememberedCredentials');
      if (saved) {
        return JSON.parse(saved);
      }
    } catch (error) {
      console.error('Error loading saved credentials:', error);
    }
    return null;
  }

  private saveCredentials(nationalCode: string, password: string) {
    try {
      const credentials = { nationalCode, password };
      localStorage.setItem('rememberedCredentials', JSON.stringify(credentials));
    } catch (error) {
      console.error('Error saving credentials:', error);
    }
  }

  private clearSavedCredentials() {
    localStorage.removeItem('rememberedCredentials');
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const formValue = this.loginForm.value;
      const loginData: LoginRequest = {
        nationalCode: formValue.nationalCode,
        password: formValue.password
      };

      // ذخیره یا حذف اطلاعات بر اساس انتخاب کاربر
      if (formValue.rememberMe) {
        this.saveCredentials(formValue.nationalCode, formValue.password);
      } else {
        this.clearSavedCredentials();
      }

      this.authService.login(loginData).subscribe({
        next: () => {
          this.snackbarService.success('ورود با موفقیت انجام شد', 'بستن', 3000);
          this.router.navigate(['/panel/dashboard']);
          this.isLoading = false;
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ورود. لطفا دوباره تلاش کنید.';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isLoading = false;
        }
      });
    }
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }
}
