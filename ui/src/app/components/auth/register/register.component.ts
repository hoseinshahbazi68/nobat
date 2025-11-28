import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { SnackbarService } from '../../../services/snackbar.service';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest } from '../../../models/auth.model';
import { CityService } from '../../../services/city.service';
import { City } from '../../../models/city.model';
import { Gender } from '../../../models/user.model';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-10px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' }))
      ])
    ])
  ]
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  isLoading = false;
  cities: City[] = [];
  filteredCities: City[] = [];
  citySearchText: string = '';
  showCityDropdown: boolean = false;
  genderOptions = [
    { value: Gender.Male, label: 'مرد' },
    { value: Gender.Female, label: 'زن' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private snackbarService: SnackbarService,
    private authService: AuthService,
    private cityService: CityService
  ) {
    this.registerForm = this.fb.group({
      nationalCode: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(10)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      phoneNumber: [''],
      cityId: [null],
      gender: [null, [Validators.required]],
      birthDate: [null]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    this.loadAllCities();
  }

  loadAllCities() {
    // استفاده از endpoint عمومی که نیاز به احراز هویت ندارد
    this.cityService.getAllPublic({ page: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.cities = result.items;
        this.filteredCities = this.cities;
      },
      error: (error) => {
        console.error('خطا در بارگذاری شهرها:', error);
        this.cities = [];
        this.filteredCities = [];
      }
    });
  }

  onCitySearch() {
    const searchText = this.citySearchText.toLowerCase().trim();
    if (!searchText) {
      this.filteredCities = this.cities;
    } else {
      this.filteredCities = this.cities.filter(city =>
        city.name.toLowerCase().includes(searchText) ||
        (city.provinceName && city.provinceName.toLowerCase().includes(searchText))
      );
    }
    this.showCityDropdown = true;
  }

  selectCity(city: City) {
    this.registerForm.patchValue({ cityId: city.id });
    this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
    this.showCityDropdown = false;
  }

  onCityInputFocus() {
    this.showCityDropdown = true;
    if (!this.citySearchText) {
      this.filteredCities = this.cities;
    }
  }

  onCityInputBlur() {
    // Delay to allow click event on dropdown items
    setTimeout(() => {
      this.showCityDropdown = false;
    }, 200);
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit() {
    if (this.registerForm.valid) {
      this.isLoading = true;
      const nationalCode = this.registerForm.value.nationalCode;
      const email = `${nationalCode}@gmail.com`;

      // پردازش gender - چون الزامی است، همیشه باید مقدار معتبر داشته باشد
      const genderFormValue = this.registerForm.value.gender;
      const genderValue: Gender = typeof genderFormValue === 'string'
        ? parseInt(genderFormValue, 10) as Gender
        : genderFormValue as Gender;

      // ساخت object داده‌ها
      const registerData: RegisterRequest = {
        nationalCode: nationalCode,
        email: email,
        password: this.registerForm.value.password,
        firstName: this.registerForm.value.firstName,
        lastName: this.registerForm.value.lastName,
        gender: genderValue, // الزامی است
        phoneNumber: this.registerForm.value.phoneNumber || undefined,
        cityId: this.registerForm.value.cityId || undefined,
        birthDate: this.registerForm.value.birthDate || undefined
      };

      this.authService.register(registerData).subscribe({
        next: (authResponse) => {
          // ثبت‌نام موفق بود و توکن در localStorage ذخیره شده است
          this.snackbarService.success('ثبت نام با موفقیت انجام شد', 'بستن', 3000);
          // هدایت به داشبورد به جای صفحه ورود
          this.router.navigate(['/panel/dashboard']);
          this.isLoading = false;
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ثبت نام. لطفا دوباره تلاش کنید.';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isLoading = false;
        }
      });
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
