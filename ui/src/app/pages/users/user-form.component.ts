import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { User, Gender } from '../../models/user.model';
import { RoleService } from '../../services/role.service';
import { Role } from '../../models/role.model';
import { CityService } from '../../services/city.service';
import { City } from '../../models/city.model';
import { UserService } from '../../services/user.service';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent implements OnInit, OnDestroy {
  userForm: FormGroup;
  isEditMode = false;
  userId: number | null = null;
  availableRoles: Role[] = [];
  selectedRoleIds: number[] = [];
  isLoadingRoles = false;
  isLoading = false;
  isSaving = false;
  cities: City[] = [];
  filteredCities: City[] = [];
  citySearchText: string = '';
  showCityDropdown: boolean = false;
  isSearchingCities: boolean = false;
  private citySearchSubject = new Subject<string>();
  genderOptions = [
    { value: Gender.Male, label: 'مرد' },
    { value: Gender.Female, label: 'زن' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private roleService: RoleService,
    private cityService: CityService,
    private userService: UserService,
    private snackbarService: SnackbarService
  ) {
    this.userForm = this.fb.group({
      nationalCode: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: [''],
      gender: [null],
      birthDate: [null],
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
    this.setupCitySearch();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.userId = +id;
      this.loadUser(+id);
    } else {
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
      this.userForm.get('password')?.updateValueAndValidity();
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
    this.userForm.patchValue({ cityId: city.id });
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

  loadRoles() {
    this.isLoadingRoles = true;
    this.roleService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.availableRoles = result.items.filter(role => role.id !== undefined);
        this.isLoadingRoles = false;

        if (this.isEditMode && this.userId) {
          this.loadUserRoles();
        }
      },
      error: (error) => {
        console.error('خطا در بارگذاری نقش‌ها:', error);
        this.isLoadingRoles = false;
      }
    });
  }

  loadUser(id: number) {
    this.isLoading = true;
    this.userService.getById(id).subscribe({
      next: (user) => {
        const formData: any = {
          nationalCode: user.nationalCode,
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
          phoneNumber: user.phoneNumber || '',
          gender: user.gender,
          birthDate: user.birthDate ? user.birthDate.split('T')[0] : '',
          cityId: user.cityId,
          isActive: user.isActive
        };

        // اگر شهر انتخاب شده، نام شهر را نمایش بده
        if (user.cityId) {
          this.cityService.getById(user.cityId).subscribe({
            next: (city) => {
              this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
              this.userForm.patchValue(formData);
              this.loadUserRoles();
            },
            error: () => {
              this.userForm.patchValue(formData);
              this.loadUserRoles();
            }
          });
        } else {
          this.userForm.patchValue(formData);
          this.loadUserRoles();
        }

        this.userForm.get('password')?.clearValidators();
        this.userForm.get('password')?.updateValueAndValidity();
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری اطلاعات کاربر';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
        this.router.navigate(['/panel/users']);
      }
    });
  }

  loadUserRoles() {
    if (!this.userId) {
      return;
    }

    this.userService.getById(this.userId).subscribe({
      next: (user) => {
        if (user.roles) {
          const userRoleNames = user.roles;
          const userRoleIds = this.availableRoles
            .filter(role => role.id && userRoleNames.includes(role.name))
            .map(role => role.id!);

          this.selectedRoleIds = userRoleIds;
          this.userForm.patchValue({ roleIds: userRoleIds });
        }
      },
      error: (error) => {
        console.error('خطا در بارگذاری نقش‌های کاربر:', error);
      }
    });
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

    if (this.userForm.invalid) {
      this.markFormGroupTouched(this.userForm);
      return;
    }

    this.isSaving = true;
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

    if (this.isEditMode && this.userId) {
      userData.id = this.userId;
      this.userService.update(userData).subscribe({
        next: () => {
          this.snackbarService.success('کاربر با موفقیت ویرایش شد', 'بستن', 3000);
          this.router.navigate(['/panel/users']);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش کاربر';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isSaving = false;
        }
      });
    } else {
      this.userService.create(userData).subscribe({
        next: () => {
          this.snackbarService.success('کاربر با موفقیت اضافه شد', 'بستن', 3000);
          this.router.navigate(['/panel/users']);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن کاربر';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
          this.isSaving = false;
        }
      });
    }
  }

  onCancel() {
    this.router.navigate(['/panel/users']);
  }

  markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  ngOnDestroy() {
    this.citySearchSubject.complete();
  }
}
