import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { City } from '../../models/city.model';
import { UpdateProfileRequest, User, Gender } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { CityService } from '../../services/city.service';
import { SnackbarService } from '../../services/snackbar.service';
import { DialogService } from '../../services/dialog.service';
import { ImageCropperComponent, ImageCropperData } from '../../components/image-cropper/image-cropper.component';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, OnDestroy {
  profileForm: FormGroup;
  user: User | null = null;
  isLoading = false;
  isSaving = false;
  cities: City[] = [];
  filteredCities: City[] = [];
  citySearchText: string = '';
  showCityDropdown: boolean = false;
  isSearchingCities: boolean = false;
  private citySearchSubject = new Subject<string>();
  profilePictureUrl: string | null = null;
  selectedFile: File | null = null;
  genderOptions = [
    { value: Gender.Male, label: 'مرد' },
    { value: Gender.Female, label: 'زن' }
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackbarService: SnackbarService,
    private cityService: CityService,
    private dialogService: DialogService
  ) {
    this.profileForm = this.fb.group({
      nationalCode: [{ value: '', disabled: true }],
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      phoneNumber: [''],
      cityId: [null],
      gender: [null, [Validators.required]],
      birthDate: [null]
    });
  }

  ngOnInit() {
    this.setupCitySearch();
    this.loadUserProfile();
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
    this.profileForm.patchValue({ cityId: city.id });
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

  loadUserProfile() {
    this.isLoading = true;
    this.authService.getCurrentUserFromApi().subscribe({
      next: (user) => {
        this.user = user;
        const formData: any = {
          nationalCode: user.nationalCode,
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
          phoneNumber: user.phoneNumber || '',
          cityId: user.cityId,
          gender: user.gender,
          birthDate: user.birthDate ? new Date(user.birthDate).toISOString().split('T')[0] : null
        };

        // تنظیم عکس پروفایل از دیتابیس
        if (user.profilePicture) {
          // اگر URL کامل است، استفاده کن، در غیر این صورت URL کامل بساز
          if (user.profilePicture.startsWith('http://') || user.profilePicture.startsWith('https://')) {
            this.profilePictureUrl = user.profilePicture;
          } else {
            // ساخت URL کامل برای عکس پروفایل
            const baseUrl = environment.apiUrl.replace('/api/v1', '');
            this.profilePictureUrl = user.profilePicture.startsWith('/')
              ? `${baseUrl}${user.profilePicture}`
              : `${baseUrl}/${user.profilePicture}`;
          }
        } else {
          // اگر عکس وجود نداشت، از عکس پیش‌فرض استفاده کن
          this.profilePictureUrl = this.getDefaultProfilePicture();
        }

        // اگر شهر انتخاب شده، نام شهر را نمایش بده
        if (user.cityId) {
          this.cityService.getById(user.cityId).subscribe({
            next: (city) => {
              this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
              this.profileForm.patchValue(formData);
            },
            error: () => {
              this.profileForm.patchValue(formData);
            }
          });
        } else {
          this.profileForm.patchValue(formData);
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری اطلاعات پروفایل';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
        // Fallback to localStorage
        const localUser = this.authService.getCurrentUser();
        if (localUser) {
          this.user = localUser;
          const formData: any = {
            nationalCode: localUser.nationalCode,
            email: localUser.email,
            firstName: localUser.firstName,
            lastName: localUser.lastName,
            phoneNumber: localUser.phoneNumber || '',
            cityId: localUser.cityId,
            gender: localUser.gender,
            birthDate: localUser.birthDate ? new Date(localUser.birthDate).toISOString().split('T')[0] : null
          };

          // تنظیم عکس پروفایل از localStorage
          if (localUser.profilePicture) {
            if (localUser.profilePicture.startsWith('http://') || localUser.profilePicture.startsWith('https://')) {
              this.profilePictureUrl = localUser.profilePicture;
            } else {
              const baseUrl = environment.apiUrl.replace('/api/v1', '');
              this.profilePictureUrl = localUser.profilePicture.startsWith('/')
                ? `${baseUrl}${localUser.profilePicture}`
                : `${baseUrl}/${localUser.profilePicture}`;
            }
          } else {
            this.profilePictureUrl = null;
          }

          if (localUser.cityId) {
            this.cityService.getById(localUser.cityId).subscribe({
              next: (city) => {
                this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
                this.profileForm.patchValue(formData);
              },
              error: () => {
                this.profileForm.patchValue(formData);
              }
            });
          } else {
            this.profileForm.patchValue(formData);
          }
        } else {
          // اگر کاربری در localStorage هم نبود، عکس را null کن
          this.profilePictureUrl = null;
        }
      }
    });
  }

  saveProfile() {
    if (this.profileForm.invalid) {
      this.markFormGroupTouched(this.profileForm);
      return;
    }

    this.isSaving = true;

    // اگر عکس انتخاب شده، ابتدا عکس را آپلود می‌کنیم
    if (this.selectedFile) {
      this.authService.uploadProfilePicture(this.selectedFile).subscribe({
        next: (user) => {
          // بعد از آپلود موفق عکس، اطلاعات پروفایل را به‌روزرسانی می‌کنیم
          this.user = user;
          if (user.profilePicture) {
            this.profilePictureUrl = user.profilePicture.startsWith('http')
              ? user.profilePicture
              : `${environment.apiUrl.replace('/api/v1', '')}${user.profilePicture}`;
          } else {
            this.profilePictureUrl = this.getDefaultProfilePicture();
          }
          this.selectedFile = null;
          this.updateProfileData();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در آپلود عکس پروفایل';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isSaving = false;
        }
      });
    } else {
      // اگر عکسی انتخاب نشده، فقط اطلاعات پروفایل را به‌روزرسانی می‌کنیم
      this.updateProfileData();
    }
  }

  private updateProfileData() {
    const genderValue = this.profileForm.value.gender;
    const updateData: UpdateProfileRequest = {
      email: this.profileForm.value.email,
      firstName: this.profileForm.value.firstName,
      lastName: this.profileForm.value.lastName,
      phoneNumber: this.profileForm.value.phoneNumber || undefined,
      cityId: this.profileForm.value.cityId || undefined,
      gender: typeof genderValue === 'string' ? parseInt(genderValue, 10) as Gender : genderValue as Gender, // الزامی است
      birthDate: this.profileForm.value.birthDate || undefined
    };

    this.authService.updateProfile(updateData).subscribe({
      next: (user) => {
        this.user = user;

        // به‌روزرسانی عکس پروفایل اگر از سرور آمده باشد
        if (user.profilePicture) {
          this.profilePictureUrl = user.profilePicture.startsWith('http')
            ? user.profilePicture
            : `${environment.apiUrl.replace('/api/v1', '')}${user.profilePicture}`;
        } else {
          this.profilePictureUrl = this.getDefaultProfilePicture();
        }

        // به‌روزرسانی نام شهر در input اگر شهر انتخاب شده باشد
        if (user.cityId) {
          this.cityService.getById(user.cityId).subscribe({
            next: (city) => {
              this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
            },
            error: () => {
              // اگر خطا در دریافت شهر بود، فقط cityId را به‌روزرسانی می‌کنیم
              this.profileForm.patchValue({ cityId: user.cityId });
            }
          });
        } else {
          this.citySearchText = '';
          this.profileForm.patchValue({ cityId: null });
        }

        this.snackbarService.success('پروفایل با موفقیت به‌روزرسانی شد', 'بستن', 3000);
        this.isSaving = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در به‌روزرسانی پروفایل';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isSaving = false;
      }
    });
  }

  markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  getFullName(): string {
    if (!this.user) return '';
    return `${this.user.firstName} ${this.user.lastName}`.trim();
  }

  getRoles(): string {
    if (!this.user || !this.user.roles || this.user.roles.length === 0) {
      return 'بدون نقش';
    }
    return this.user.roles.join(', ');
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // باز کردن dialog کراپ
      const cropperData: ImageCropperData = {
        imageFile: file,
        aspectRatio: 1, // مربع
        maxWidth: 800,
        maxHeight: 800,
        quality: 0.85,
        maxSizeKB: 200 // حداکثر 200 کیلوبایت
      };

      this.dialogService.open(ImageCropperComponent, {
        width: '90vw',
        maxWidth: '1200px',
        data: cropperData
      }).subscribe((croppedFile: File | null) => {
        if (croppedFile) {
          this.selectedFile = croppedFile;
          // ایجاد preview
          const reader = new FileReader();
          reader.onload = (e: any) => {
            this.profilePictureUrl = e.target.result;
          };
          reader.readAsDataURL(this.selectedFile);
        }
        // پاک کردن input برای امکان انتخاب مجدد همان فایل
        input.value = '';
      });
    }
  }

  removeProfilePicture() {
    this.selectedFile = null;
    // اگر عکس از سرور بود، آن را نگه دار، در غیر این صورت null کن
    if (this.user?.profilePicture && !this.profilePictureUrl?.startsWith('data:')) {
      // عکس از سرور است، فقط selectedFile را null می‌کنیم تا در saveProfile عکس جدیدی آپلود نشود
      // برای حذف عکس از سرور، باید endpoint جداگانه‌ای داشته باشیم
    } else {
      // عکس محلی است (data URL)، آن را حذف می‌کنیم
      this.profilePictureUrl = this.getDefaultProfilePicture();
    }
  }

  getDefaultProfilePicture(): string {
    // استفاده از یک placeholder image یا SVG
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2U1ZTdlYiIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iNDAiIGZpbGw9IiM5Y2EzYWYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj7wn5GkPC90ZXh0Pjwvc3ZnPg==';
  }

  getProfilePictureUrl(): string {
    return this.profilePictureUrl || this.getDefaultProfilePicture();
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = this.getDefaultProfilePicture();
    }
  }

  ngOnDestroy() {
    this.citySearchSubject.complete();
  }

}
