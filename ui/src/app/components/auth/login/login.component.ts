import { Component } from '@angular/core';
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
export class LoginComponent {
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
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const loginData: LoginRequest = {
        nationalCode: this.loginForm.value.nationalCode,
        password: this.loginForm.value.password
      };

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
