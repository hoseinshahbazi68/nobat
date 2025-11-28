import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ImageCropperComponent, ImageCropperData } from '../../components/image-cropper/image-cropper.component';
import { City } from '../../models/city.model';
import { Clinic } from '../../models/clinic.model';
import { Doctor, DoctorPrefix, DoctorSpecialty, ScientificDegree } from '../../models/doctor.model';
import { MedicalCondition } from '../../models/medical-condition.model';
import { Specialty } from '../../models/specialty.model';
import { Gender, User } from '../../models/user.model';
import { CityService } from '../../services/city.service';
import { ClinicService } from '../../services/clinic.service';
import { DialogService } from '../../services/dialog.service';
import { DoctorService } from '../../services/doctor.service';
import { MedicalConditionService } from '../../services/medical-condition.service';
import { SnackbarService } from '../../services/snackbar.service';
import { SpecialtyService } from '../../services/specialty.service';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'app-doctor-form',
    templateUrl: './doctor-form.component.html',
    styleUrls: ['./doctor-form.component.scss']
})
export class DoctorFormComponent implements OnInit, OnDestroy {
    doctorForm: FormGroup;
    isEditMode = false;
    doctorId: number | null = null;
    availableSpecialties: Specialty[] = [];
    selectedSpecialties: { specialtyId: number; specialty: Specialty; sortOrder: number }[] = [];
    availableMedicalConditions: MedicalCondition[] = [];
    filteredMedicalConditions: MedicalCondition[] = [];
    selectedMedicalConditions: number[] = [];
    medicalConditionSearch: string = '';
    conditionsFromSpecialties: Set<number> = new Set<number>();
    userSearchValue: string = '';
    foundUser: User | null = null;
    isSearchingUser: boolean = false;
    userNotFound: boolean = false;
    availableClinics: Clinic[] = [];
    foundDoctor: Doctor | null = null;
    isSearchingDoctor: boolean = false;
    doctorPrefixes = DoctorPrefix;
    scientificDegrees = ScientificDegree;
    prefixOptions = [
        { value: DoctorPrefix.None, label: 'نامشخص' },
        { value: DoctorPrefix.Doctor, label: 'دکتر' },
        { value: DoctorPrefix.Bachelor, label: 'کارشناس' },
        { value: DoctorPrefix.Master, label: 'کارشناس ارشد' }
    ];
    scientificDegreeOptions = [
        { value: ScientificDegree.None, label: 'نامشخص' },
        { value: ScientificDegree.Fellowship, label: 'فلوشیپ' },
        { value: ScientificDegree.Subspecialty, label: 'فوق تخصص' },
        { value: ScientificDegree.ProfessionalDoctorate, label: 'دکترای تخصصی' },
        { value: ScientificDegree.Specialist, label: 'متخصص' },
        { value: ScientificDegree.Doctorate, label: 'دکتری' },
        { value: ScientificDegree.Master, label: 'کارشناس ارشد' },
        { value: ScientificDegree.Bachelor, label: 'کارشناس' }
    ];
    private clinicSubscription?: Subscription;
    cities: City[] = [];
    filteredCities: City[] = [];
    citySearchText: string = '';
    showCityDropdown: boolean = false;
    isSearchingCities: boolean = false;
    private citySearchSubject = new Subject<string>();
    private medicalConditionSearchSubject = new Subject<string>();
    isSearchingMedicalConditions: boolean = false;
    genderOptions = [
        { value: Gender.Male, label: 'مرد' },
        { value: Gender.Female, label: 'زن' }
    ];
    selectedFile: File | null = null;
    profilePicturePreview: string | null = null;
    apiUrl = environment.apiUrl;

    get profilePictureUrl(): string {
        if (this.profilePicturePreview) {
            return this.profilePicturePreview;
        }
        if (this.foundDoctor?.profilePicture) {
            const profilePicture = this.foundDoctor.profilePicture;
            // اگر URL کامل است، مستقیماً استفاده کن
            if (profilePicture.startsWith('http://') || profilePicture.startsWith('https://')) {
                return profilePicture;
            }
            // اگر URL نسبی است، base URL را از apiUrl استخراج کن (بدون /api/v1)
            const baseUrl = this.apiUrl.replace('/api/v1', '').replace('/api', '');
            return profilePicture.startsWith('/')
                ? `${baseUrl}${profilePicture}`
                : `${baseUrl}/${profilePicture}`;
        }
        if (this.foundUser?.profilePicture) {
            const profilePicture = this.foundUser.profilePicture;
            // اگر URL کامل است، مستقیماً استفاده کن
            if (profilePicture.startsWith('http://') || profilePicture.startsWith('https://')) {
                return profilePicture;
            }
            // اگر URL نسبی است، base URL را از apiUrl استخراج کن (بدون /api/v1)
            const baseUrl = this.apiUrl.replace('/api/v1', '').replace('/api', '');
            return profilePicture.startsWith('/')
                ? `${baseUrl}${profilePicture}`
                : `${baseUrl}/${profilePicture}`;
        }
        return this.getDefaultProfilePicture();
    }

    getDefaultProfilePicture(): string {
        // استفاده از یک placeholder image یا SVG
        return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2U1ZTdlYiIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iNDAiIGZpbGw9IiM5Y2EzYWYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj7wn5GkPC90ZXh0Pjwvc3ZnPg==';
    }

    onImageError(event: Event): void {
        const img = event.target as HTMLImageElement;
        if (img) {
            img.src = this.getDefaultProfilePicture();
        }
    }

    constructor(
        private fb: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private snackbarService: SnackbarService,
        private doctorService: DoctorService,
        private specialtyService: SpecialtyService,
        private userService: UserService,
        private clinicService: ClinicService,
        private medicalConditionService: MedicalConditionService,
        private cityService: CityService,
        private dialogService: DialogService
    ) {
        this.doctorForm = this.fb.group({
            medicalCode: ['', Validators.required],
            prefix: [DoctorPrefix.None],
            scientificDegree: [ScientificDegree.None],
            firstName: [{ value: '', disabled: true }],
            lastName: [{ value: '', disabled: true }],
            phoneNumber: [{ value: '', disabled: true }],
            userId: [null], // validation در saveDoctor انجام می‌شود
            clinicId: [null],
            cityId: [null],
            gender: [null],
            birthDate: [null]
        });
    }

    ngOnInit() {
        this.loadSpecialties();
        this.loadClinics();
        this.setupCitySearch();
        this.setupMedicalConditionSearch();

        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.isEditMode = true;
            this.doctorId = +id;
            this.loadDoctor(+id);
        } else {
            // در حالت جدید، کلینیک انتخاب شده از header را تنظیم کن
            this.clinicSubscription = this.clinicService.selectedClinic$.subscribe(clinic => {
                if (clinic && clinic.id) {
                    this.doctorForm.patchValue({ clinicId: clinic.id });
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
                    // جستجو از سرور با استفاده از filters
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
                    // اگر جستجو خالی است، لیست اولیه را برگردان
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
        this.doctorForm.patchValue({ cityId: city.id });
        this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
        this.showCityDropdown = false;
    }

    onCityInputFocus() {
        this.showCityDropdown = true;
        // اگر جستجو خالی است، لیست اولیه را بارگذاری کن
        if (!this.citySearchText || !this.citySearchText.trim()) {
            this.citySearchSubject.next('');
        }
    }

    onCityInputBlur() {
        setTimeout(() => {
            this.showCityDropdown = false;
        }, 200);
    }

    loadClinics() {
        this.clinicService.getAll({ page: 1, pageSize: 100 }).subscribe({
            next: (result) => {
                this.availableClinics = result.items;
            },
            error: (error) => {
                console.error('خطا در بارگذاری کلینیک‌ها:', error);
            }
        });
    }

    loadSpecialties() {
        this.specialtyService.getAll({ page: 1, pageSize: 100 }).subscribe({
            next: (result) => {
                this.availableSpecialties = result.items;
            },
            error: (error) => {
                console.error('خطا در بارگذاری تخصص‌ها:', error);
            }
        });
    }

    setupMedicalConditionSearch() {
        this.medicalConditionSearchSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            switchMap(searchTerm => {
                this.isSearchingMedicalConditions = true;
                const searchQuery = searchTerm?.trim() || '';
                if (searchQuery) {
                    return this.medicalConditionService.search(searchQuery).pipe(
                        catchError(error => {
                            this.isSearchingMedicalConditions = false;
                            console.error('خطا در جستجوی علائم پزشکی:', error);
                            return [];
                        })
                    );
                } else {
                    return this.medicalConditionService.getAll({ page: 1, pageSize: 1000 }).pipe(
                        catchError(error => {
                            this.isSearchingMedicalConditions = false;
                            console.error('خطا در بارگذاری علائم پزشکی:', error);
                            return [];
                        }),
                        switchMap(result => {
                            return [result.items || []];
                        })
                    );
                }
            })
        ).subscribe({
            next: (conditions) => {
                // فیلتر کردن علائم بر اساس تخصص‌های انتخاب شده
                const selectedSpecialtyIds = this.selectedSpecialties
                    .map(s => s.specialtyId)
                    .filter(id => id && id > 0);

                if (selectedSpecialtyIds.length > 0) {
                    // اگر تخصص‌هایی انتخاب شده، فقط علائم مرتبط با آن‌ها را نمایش بده
                    // اما علائمی که کاربر دستی انتخاب کرده را هم نگه دار
                    const manuallySelectedConditions = conditions.filter(c =>
                        c.id != null && this.selectedMedicalConditions.includes(c.id)
                    );
                    const specialtyRelatedConditions = conditions.filter(c =>
                        c.id != null && this.conditionsFromSpecialties.has(c.id)
                    );
                    this.filteredMedicalConditions = [
                        ...specialtyRelatedConditions,
                        ...manuallySelectedConditions.filter(c =>
                            !specialtyRelatedConditions.find(sc => sc.id === c.id)
                        )
                    ];
                } else {
                    this.filteredMedicalConditions = conditions;
                }
                this.availableMedicalConditions = conditions;
                this.isSearchingMedicalConditions = false;
            }
        });
    }

    loadMedicalConditions() {
        // بارگذاری اولیه - اگر تخصص‌هایی انتخاب شده‌اند، لیست را فیلتر کن
        if (this.selectedSpecialties.length > 0 &&
            this.selectedSpecialties.some(s => s.specialtyId && s.specialtyId > 0)) {
            this.updateAvailableMedicalConditions();
        } else {
            this.medicalConditionSearchSubject.next('');
        }
    }

    filterMedicalConditions() {
        this.medicalConditionSearchSubject.next(this.medicalConditionSearch);
    }

    isConditionFromSpecialty(conditionId: number): boolean {
        return this.conditionsFromSpecialties.has(conditionId);
    }

    searchUserByNationalCode(nationalCode: string) {
        if (this.isSearchingUser) {
            return;
        }

        if (!nationalCode || !nationalCode.trim()) {
            this.foundUser = null;
            this.userNotFound = false;
            this.doctorForm.patchValue({ userId: null });
            // وقتی کد ملی خالی است، فیلدها را غیرفعال کن
            this.doctorForm.get('firstName')?.disable();
            this.doctorForm.get('lastName')?.disable();
            this.doctorForm.get('phoneNumber')?.disable();
            this.citySearchText = '';
            this.showCityDropdown = false;
            return;
        }

        const trimmedCode = nationalCode.trim();

        // اگر کاربر قبلاً پیدا شده و کد ملی تغییر نکرده، جستجو نکن
        if (this.foundUser && this.foundUser.nationalCode === trimmedCode) {
            return;
        }

        this.isSearchingUser = true;
        this.userService.searchByNationalCode(trimmedCode).subscribe({
            next: (user) => {
                this.isSearchingUser = false;
                // کاربر پیدا شد - فیلدها را پر کن و غیرفعال نگه دار
                this.foundUser = user;
                this.userNotFound = false;
                this.doctorForm.patchValue({
                    userId: this.foundUser.id,
                    firstName: this.foundUser.firstName || '',
                    lastName: this.foundUser.lastName || '',
                    phoneNumber: this.foundUser.phoneNumber || '',
                    gender: this.foundUser.gender,
                    birthDate: this.foundUser.birthDate ? new Date(this.foundUser.birthDate).toISOString().split('T')[0] : null
                });
                // فیلدها را غیرفعال کن چون کاربر پیدا شد
                this.doctorForm.get('firstName')?.disable();
                this.doctorForm.get('lastName')?.disable();
                this.doctorForm.get('phoneNumber')?.disable();
                this.showCityDropdown = false;

                // اگر کاربر شهر دارد، نام شهر را نمایش بده و cityId را تنظیم کن
                if (user.cityId) {
                    this.doctorForm.patchValue({ cityId: user.cityId });
                    this.cityService.getById(user.cityId).subscribe({
                        next: (city) => {
                            this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
                        }
                    });
                } else {
                    this.citySearchText = '';
                }
                // پاک کردن preview قبلی اگر عکس جدیدی انتخاب نشده باشد
                if (!this.selectedFile) {
                    this.profilePicturePreview = null;
                }
                this.snackbarService.success('کاربر یافت شد', 'بستن', 2000);
            },
            error: (error) => {
                this.isSearchingUser = false;
                // اگر خطای 404 باشد، یعنی کاربر پیدا نشد
                if (error.status === 404) {
                    // کاربر پیدا نشد - فیلدها را فعال کن تا کاربر بتواند اطلاعات را وارد کند
                    this.foundUser = null;
                    this.userNotFound = true;
                    // پاک کردن عکس پروفایل
                    this.profilePicturePreview = null;
                    // فقط userId را null کن و فیلدها را فعال کن، اما مقادیر قبلی را نگه دار
                    this.doctorForm.patchValue({
                        userId: null
                        // مقادیر firstName، lastName، phoneNumber و غیره را حفظ می‌کنیم
                    });
                    // فیلدها را فعال کن
                    this.doctorForm.get('firstName')?.enable();
                    this.doctorForm.get('lastName')?.enable();
                    this.doctorForm.get('phoneNumber')?.enable();
                    // شهر را پاک نکن، فقط dropdown را ببند
                    this.showCityDropdown = false;
                } else {
                    // خطای دیگر
                    this.foundUser = null;
                    this.userNotFound = false;
                    // پاک کردن عکس پروفایل
                    this.profilePicturePreview = null;
                    this.doctorForm.patchValue({ userId: null });
                    // در صورت خطا، فیلدها را فعال کن
                    this.doctorForm.get('firstName')?.enable();
                    this.doctorForm.get('lastName')?.enable();
                    this.doctorForm.get('phoneNumber')?.enable();
                    console.error('خطا در جستجوی کاربر:', error);
                    this.snackbarService.error('خطا در جستجوی کاربر', 'بستن', 3000);
                }
            }
        });
    }

    clearUser() {
        this.userSearchValue = '';
        this.foundUser = null;
        this.userNotFound = false;
        // پاک کردن عکس پروفایل
        this.profilePicturePreview = null;
        this.doctorForm.patchValue({
            userId: null,
            firstName: '',
            lastName: '',
            phoneNumber: ''
        });
        // فیلدها را غیرفعال کن
        this.doctorForm.get('firstName')?.disable();
        this.doctorForm.get('lastName')?.disable();
        this.doctorForm.get('phoneNumber')?.disable();
    }

    onMedicalCodeChange() {
        if (this.isEditMode || this.isSearchingDoctor) {
            return;
        }

        const medicalCode = this.doctorForm.get('medicalCode')?.value?.trim();
        if (!medicalCode) {
            this.foundDoctor = null;
            this.foundUser = null;
            this.userSearchValue = '';
            this.selectedSpecialties = [];
            return;
        }

        // اگر پزشک قبلاً پیدا شده و کد تغییر نکرده، جستجو نکن
        if (this.foundDoctor && this.foundDoctor.medicalCode === medicalCode) {
            return;
        }

        this.isSearchingDoctor = true;
        this.doctorService.getByMedicalCode(medicalCode).subscribe({
            next: (doctor) => {
                this.isSearchingDoctor = false;
                this.foundDoctor = doctor;
                this.userSearchValue = doctor.nationalCode;
                console.log(doctor)
                this.doctorForm.patchValue({
                    medicalCode: doctor.medicalCode,
                    prefix: doctor.prefix ?? DoctorPrefix.None,
                    scientificDegree: doctor.scientificDegree ?? ScientificDegree.None,
                    userId: doctor.userId || null,
                    firstName: doctor.user?.firstName || doctor.firstName || '',
                    lastName: doctor.user?.lastName || doctor.lastName || '',
                    nationalCode: doctor.nationalCode,
                    phoneNumber: doctor.user?.phoneNumber || doctor.phone || ''
                });

                if (doctor.user) {
                    this.foundUser = doctor.user;
                    this.userSearchValue = doctor.user.nationalCode || '';
                    // اگر کاربر پیدا شد، فیلدها را غیرفعال کن
                    this.doctorForm.get('firstName')?.disable();
                    this.doctorForm.get('lastName')?.disable();
                    this.doctorForm.get('phoneNumber')?.disable();
                } else {
                    // اگر کاربری متصل نیست، کد ملی را تنظیم کن و جستجو کن
                    this.userSearchValue = doctor.nationalCode || '';
                    // اگر کد ملی وجود دارد، جستجو کن
                    if (this.userSearchValue) {
                        this.searchUserByNationalCode(this.userSearchValue);
                    } else {
                        // اگر کد ملی وجود ندارد، فیلدها را فعال کن
                        this.doctorForm.get('firstName')?.enable();
                        this.doctorForm.get('lastName')?.enable();
                        this.doctorForm.get('phoneNumber')?.enable();
                    }
                }

                if (doctor.specialties && doctor.specialties.length > 0) {
                    this.selectedSpecialties = doctor.specialties.map((ds: DoctorSpecialty, index: number) => ({
                        specialtyId: ds.specialtyId,
                        specialty: ds.specialty || this.availableSpecialties.find(s => s.id === ds.specialtyId)!,
                        sortOrder: ds.sortOrder || index
                    })).sort((a, b) => a.sortOrder - b.sortOrder);

                    // بارگذاری علائم مرتبط با تخصص‌ها
                    this.updateMedicalConditionsFromSpecialties();
                }

                // بارگذاری علائم پزشکی انتخاب شده توسط پزشک
                if (doctor.medicalConditions && doctor.medicalConditions.length > 0) {
                    const doctorConditionIds = doctor.medicalConditions
                        .map(dmc => dmc.medicalConditionId)
                        .filter(id => id !== undefined) as number[];

                    // اضافه کردن علائم انتخاب شده به لیست (بدون تکرار)
                    doctorConditionIds.forEach(id => {
                        if (!this.selectedMedicalConditions.includes(id)) {
                            this.selectedMedicalConditions.push(id);
                        }
                    });
                }

                // به‌روزرسانی لیست علائم بر اساس تخصص‌ها
                if (this.selectedSpecialties.length > 0) {
                    setTimeout(() => {
                        this.updateAvailableMedicalConditions();
                    }, 500);
                }

                this.snackbarService.success('پزشک یافت شد', 'بستن', 2000);
            },
            error: (error) => {
                this.isSearchingDoctor = false;
                this.foundDoctor = null;
                console.error('خطا در جستجوی پزشک:', error);
                if (error.status === 404) {
                    // پزشک پیدا نشد - پیغام نمایش نده، فقط اجازه بده کاربر ادامه دهد
                } else {
                    this.snackbarService.error('خطا در جستجوی پزشک', 'بستن', 3000);
                }
            }
        });
    }

    clearDoctorSearch() {
        this.foundDoctor = null;
        this.doctorForm.patchValue({
            medicalCode: '',
            firstName: '',
            lastName: '',
            phoneNumber: ''
        });
        this.foundUser = null;
        this.userSearchValue = '';
        this.selectedSpecialties = [];
        this.selectedMedicalConditions = [];
        // فیلدها را غیرفعال کن
        this.doctorForm.get('firstName')?.disable();
        this.doctorForm.get('lastName')?.disable();
        this.doctorForm.get('phoneNumber')?.disable();
    }

    loadDoctor(id: number) {
        this.doctorService.getById(id).subscribe({
            next: (doctor) => {
                // نمایش عکس پروفایل موجود
                if (doctor.profilePicture) {
                    this.profilePicturePreview = null; // برای نمایش عکس از سرور
                    this.foundDoctor = doctor; // ذخیره doctor برای نمایش عکس در template
                }

                // تنظیم کد ملی - همیشه از doctor.nationalCode استفاده کن
                this.userSearchValue = doctor.nationalCode || '';

                this.doctorForm.patchValue({
                    medicalCode: doctor.medicalCode,
                    prefix: doctor.prefix ?? DoctorPrefix.None,
                    scientificDegree: doctor.scientificDegree ?? ScientificDegree.None,
                    userId: doctor.userId || null,
                    firstName: doctor.user?.firstName || doctor.firstName || '',
                    lastName: doctor.user?.lastName || doctor.lastName || '',
                    phoneNumber: doctor.user?.phoneNumber || doctor.phone || '',
                    gender: doctor.user?.gender,
                    birthDate: doctor.user?.birthDate ? new Date(doctor.user.birthDate).toISOString().split('T')[0] : null,
                    cityId: doctor.cityId || doctor.user?.cityId || null
                });

                // ذخیره doctor برای نمایش عکس پروفایل
                this.foundDoctor = doctor;

                // اگر کاربری متصل است، اطلاعاتش را نمایش بده
                if (doctor.userId && doctor.user) {
                    this.foundUser = doctor.user;
                    // اگر کاربر پیدا شد، فیلدها را غیرفعال کن
                    this.doctorForm.get('firstName')?.disable();
                    this.doctorForm.get('lastName')?.disable();
                    this.doctorForm.get('phoneNumber')?.disable();
                    // اطمینان حاصل کن که userId در فرم تنظیم شده است
                    if (doctor.userId) {
                        this.doctorForm.patchValue({ userId: doctor.userId });
                    }
                } else {
                    // اگر کاربری متصل نیست، اگر کد ملی وجود دارد جستجو کن
                    if (this.userSearchValue) {
                        this.searchUserByNationalCode(this.userSearchValue);
                    } else {
                        // اگر کد ملی وجود ندارد، فیلدها را فعال کن
                        this.doctorForm.get('firstName')?.enable();
                        this.doctorForm.get('lastName')?.enable();
                        this.doctorForm.get('phoneNumber')?.enable();
                    }
                }

                // اگر کاربر شهر دارد، نام شهر را نمایش بده
                // اول از doctor.cityId استفاده کن، اگر نبود از doctor.user?.cityId
                const cityId = doctor.cityId || doctor.user?.cityId;
                if (cityId) {
                    this.cityService.getById(cityId).subscribe({
                        next: (city) => {
                            this.citySearchText = `${city.name}${city.provinceName ? ' - ' + city.provinceName : ''}`;
                        },
                        error: () => {
                            this.citySearchText = '';
                        }
                    });
                } else {
                    this.citySearchText = '';
                }

                // بارگذاری تخصص‌های پزشک
                if (doctor.specialties && doctor.specialties.length > 0) {
                    this.selectedSpecialties = doctor.specialties.map((ds: DoctorSpecialty, index: number) => ({
                        specialtyId: ds.specialtyId,
                        specialty: ds.specialty || this.availableSpecialties.find(s => s.id === ds.specialtyId)!,
                        sortOrder: ds.sortOrder || index
                    })).sort((a, b) => a.sortOrder - b.sortOrder);

                    // بارگذاری علائم مرتبط با تخصص‌ها
                    this.updateMedicalConditionsFromSpecialties();
                }

                // بارگذاری علائم پزشکی انتخاب شده توسط پزشک
                if (doctor.medicalConditions && doctor.medicalConditions.length > 0) {
                    const doctorConditionIds = doctor.medicalConditions
                        .map(dmc => dmc.medicalConditionId)
                        .filter(id => id !== undefined) as number[];

                    // اضافه کردن علائم انتخاب شده به لیست (بدون تکرار)
                    doctorConditionIds.forEach(id => {
                        if (!this.selectedMedicalConditions.includes(id)) {
                            this.selectedMedicalConditions.push(id);
                        }
                    });
                } else {
                    this.selectedMedicalConditions = [];
                }

                // به‌روزرسانی لیست علائم بر اساس تخصص‌ها
                if (this.selectedSpecialties.length > 0) {
                    setTimeout(() => {
                        this.updateAvailableMedicalConditions();
                    }, 500);
                }
            },
            error: (error) => {
                const errorMessage = error.error?.message || 'خطا در بارگذاری اطلاعات پزشک';
                this.snackbarService.error(errorMessage, 'بستن', 5000);
                this.router.navigate(['/panel/doctors']);
            }
        });
    }

    addSpecialty() {
        if (this.selectedSpecialties.length === 0 || this.selectedSpecialties[this.selectedSpecialties.length - 1].specialtyId) {
            const maxSortOrder = this.selectedSpecialties.length > 0
                ? Math.max(...this.selectedSpecialties.map(s => s.sortOrder)) + 1
                : 0;

            this.selectedSpecialties.push({
                specialtyId: 0,
                specialty: this.availableSpecialties[0],
                sortOrder: maxSortOrder
            });
        }
    }

    removeSpecialty(index: number) {
        const removedSpecialty = this.selectedSpecialties[index];
        this.selectedSpecialties.splice(index, 1);
        // به‌روزرسانی sortOrder
        this.selectedSpecialties.forEach((item, idx) => {
            item.sortOrder = idx;
        });

        // اگر تخصصی حذف شد، علائم مرتبط با آن را هم حذف کن (فقط اگر دیگر تخصصی نداریم که به آن علائم مرتبط باشد)
        if (removedSpecialty.specialtyId) {
            this.updateMedicalConditionsFromSpecialties();
        }
    }

    onSpecialtyChange(specialtyId: number, index: number) {
        if (!specialtyId || specialtyId === 0) {
            // اگر تخصص حذف شد، علائم را به‌روزرسانی کن
            this.updateMedicalConditionsFromSpecialties();
            return;
        }

        // تخصص انتخاب شد - علائم مرتبط با آن را اضافه کن
        this.loadMedicalConditionsForSpecialty(specialtyId);
    }

    loadMedicalConditionsForSpecialty(specialtyId: number) {
        this.specialtyService.getMedicalConditions(specialtyId).subscribe({
            next: (specialtyMedicalConditions) => {
                // اضافه کردن علائم پزشکی مرتبط به selectedMedicalConditions (بدون تکرار)
                specialtyMedicalConditions.forEach(smc => {
                    if (smc.medicalConditionId) {
                        if (!this.selectedMedicalConditions.includes(smc.medicalConditionId)) {
                            this.selectedMedicalConditions.push(smc.medicalConditionId);
                        }
                        // علامت‌گذاری علائم مرتبط با تخصص
                        this.conditionsFromSpecialties.add(smc.medicalConditionId);
                    }
                });

                // به‌روزرسانی لیست availableMedicalConditions برای نمایش علائم مرتبط
                this.updateAvailableMedicalConditions();
            },
            error: (error) => {
                console.error('خطا در بارگذاری علائم پزشکی تخصص:', error);
            }
        });
    }

    updateMedicalConditionsFromSpecialties() {
        // پاک کردن لیست علائم مرتبط با تخصص‌ها
        this.conditionsFromSpecialties.clear();

        // جمع‌آوری همه تخصص‌های انتخاب شده
        const selectedSpecialtyIds = this.selectedSpecialties
            .map(s => s.specialtyId)
            .filter(id => id && id > 0);

        if (selectedSpecialtyIds.length === 0) {
            // اگر هیچ تخصصی انتخاب نشده، لیست را به حالت اولیه برگردان
            this.loadMedicalConditions();
            return;
        }

        // بارگذاری علائم برای همه تخصص‌های انتخاب شده و اضافه کردن آن‌ها به لیست انتخاب شده
        const allConditionIds = new Set<number>();
        let completedRequests = 0;

        selectedSpecialtyIds.forEach(specialtyId => {
            this.specialtyService.getMedicalConditions(specialtyId).subscribe({
                next: (specialtyMedicalConditions) => {
                    specialtyMedicalConditions.forEach(smc => {
                        if (smc.medicalConditionId) {
                            allConditionIds.add(smc.medicalConditionId);
                            // علامت‌گذاری علائم مرتبط با تخصص
                            this.conditionsFromSpecialties.add(smc.medicalConditionId);
                        }
                    });

                    completedRequests++;
                    if (completedRequests === selectedSpecialtyIds.length) {
                        // اضافه کردن علائم مرتبط با تخصص‌ها به لیست انتخاب شده (بدون حذف موارد دستی)
                        allConditionIds.forEach(conditionId => {
                            if (!this.selectedMedicalConditions.includes(conditionId)) {
                                this.selectedMedicalConditions.push(conditionId);
                            }
                        });

                        // به‌روزرسانی لیست علائم برای نمایش
                        this.updateAvailableMedicalConditions();
                    }
                },
                error: (error) => {
                    console.error('خطا در بارگذاری علائم پزشکی تخصص:', error);
                    completedRequests++;
                    if (completedRequests === selectedSpecialtyIds.length) {
                        this.updateAvailableMedicalConditions();
                    }
                }
            });
        });
    }

    updateAvailableMedicalConditions() {
        // جمع‌آوری همه تخصص‌های انتخاب شده
        const selectedSpecialtyIds = this.selectedSpecialties
            .map(s => s.specialtyId)
            .filter(id => id && id > 0);

        if (selectedSpecialtyIds.length === 0) {
            // اگر هیچ تخصصی انتخاب نشده، همه علائم را نمایش بده
            this.loadMedicalConditions();
            return;
        }

        // بارگذاری علائم برای همه تخصص‌های انتخاب شده
        const allConditionIds = new Set<number>();
        let completedRequests = 0;

        selectedSpecialtyIds.forEach(specialtyId => {
            this.specialtyService.getMedicalConditions(specialtyId).subscribe({
                next: (specialtyMedicalConditions) => {
                    specialtyMedicalConditions.forEach(smc => {
                        if (smc.medicalConditionId) {
                            allConditionIds.add(smc.medicalConditionId);
                        }
                    });

                    completedRequests++;
                    if (completedRequests === selectedSpecialtyIds.length) {
                        // بارگذاری همه علائم و فیلتر کردن فقط علائم مرتبط با تخصص‌ها
                        this.loadAllMedicalConditions().subscribe(allConditions => {
                            // اول لیست را با علائم مرتبط با تخصص‌ها پر کن
                            this.availableMedicalConditions = allConditions.items.filter(
                                condition => condition.id && allConditionIds.has(condition.id)
                            );

                            // سپس علائمی که کاربر دستی انتخاب کرده (ولی در لیست فیلتر شده نیستند) را اضافه کن
                            const manuallySelectedConditions = allConditions.items.filter(
                                condition => condition.id &&
                                    this.selectedMedicalConditions.includes(condition.id) &&
                                    !this.availableMedicalConditions.find(c => c.id === condition.id)
                            );

                            if (manuallySelectedConditions.length > 0) {
                                this.availableMedicalConditions = [
                                    ...this.availableMedicalConditions,
                                    ...manuallySelectedConditions
                                ];
                            }

                            // به‌روزرسانی لیست فیلتر شده
                            this.filterMedicalConditions();
                        });
                    }
                },
                error: (error) => {
                    console.error('خطا در بارگذاری علائم پزشکی تخصص:', error);
                    completedRequests++;
                    if (completedRequests === selectedSpecialtyIds.length) {
                        // در صورت خطا، همه علائم را نمایش بده
                        this.loadMedicalConditions();
                    }
                }
            });
        });
    }

    loadAllMedicalConditions() {
        return this.medicalConditionService.getAll({ page: 1, pageSize: 1000 });
    }

    moveSpecialtyUp(index: number) {
        if (index > 0) {
            const temp = this.selectedSpecialties[index];
            this.selectedSpecialties[index] = this.selectedSpecialties[index - 1];
            this.selectedSpecialties[index - 1] = temp;
            this.selectedSpecialties[index].sortOrder = index;
            this.selectedSpecialties[index - 1].sortOrder = index - 1;
        }
    }

    moveSpecialtyDown(index: number) {
        if (index < this.selectedSpecialties.length - 1) {
            const temp = this.selectedSpecialties[index];
            this.selectedSpecialties[index] = this.selectedSpecialties[index + 1];
            this.selectedSpecialties[index + 1] = temp;
            this.selectedSpecialties[index].sortOrder = index;
            this.selectedSpecialties[index + 1].sortOrder = index + 1;
        }
    }

    hasInvalidSpecialties(): boolean {
        return this.selectedSpecialties.length === 0 ||
            this.selectedSpecialties.some(s => s.specialtyId === 0 || !s.specialtyId);
    }

    toggleMedicalCondition(conditionId: number) {
        const index = this.selectedMedicalConditions.indexOf(conditionId);
        if (index > -1) {
            this.selectedMedicalConditions.splice(index, 1);
        } else {
            this.selectedMedicalConditions.push(conditionId);
        }
    }

    isMedicalConditionSelected(conditionId: number): boolean {
        return this.selectedMedicalConditions.includes(conditionId);
    }

    isFormValidForSubmit(): boolean {
        // دکمه همیشه فعال است تا کاربر بتواند خطاها را ببیند
        return true;
    }

    saveDoctor() {
        // لیست خطاها برای نمایش به کاربر
        const errors: string[] = [];

        // Mark all fields as touched to show validation errors
        this.doctorForm.markAllAsTouched();
        Object.keys(this.doctorForm.controls).forEach(key => {
            this.doctorForm.get(key)?.markAsTouched();
        });

        // دریافت مقادیر فرم - استفاده از getRawValue برای فیلدهای disabled
        const formValue = this.doctorForm.getRawValue();
        const medicalCode = formValue.medicalCode?.trim();
        const userId = formValue.userId;
        const firstName = formValue.firstName?.trim();
        const lastName = formValue.lastName?.trim();
        const phoneNumber = formValue.phoneNumber?.trim();

        // بررسی کد نظام پزشکی
        if (!medicalCode) {
            errors.push('کد نظام پزشکی الزامی است');
        }

        // بررسی کد ملی
        if (!this.userSearchValue || !this.userSearchValue.trim()) {
            errors.push('کد ملی الزامی است');
        }

        // بررسی تخصص‌ها
        if (this.selectedSpecialties.length === 0) {
            errors.push('حداقل یک تخصص باید انتخاب شود');
        } else if (this.hasInvalidSpecialties()) {
            errors.push('لطفاً تمام تخصص‌های انتخاب شده را تکمیل کنید');
        }

        // اگر کاربر پیدا نشده (userId null است)، باید اطلاعات کاربر وارد شده باشد
        if (!userId) {
            if (!firstName) {
                errors.push('نام الزامی است');
            }
            if (!lastName) {
                errors.push('نام خانوادگی الزامی است');
            }
            if (!phoneNumber) {
                errors.push('شماره موبایل الزامی است');
            }
        }

        // اگر خطایی وجود دارد، نمایش بده و متوقف شو
        if (errors.length > 0) {
            // فرمت کردن خطاها با شماره برای خوانایی بهتر
            const errorMessage = errors.map((error, index) => `${index + 1}. ${error}`).join('\n');
            this.snackbarService.error(errorMessage, 'بستن', 5000);
            return;
        }

        // در حالت ویرایش، validation را نادیده بگیر اگر userId وجود دارد
        const isFormValid = this.isEditMode && userId
            ? !this.hasInvalidSpecialties()
            : this.doctorForm.valid && !this.hasInvalidSpecialties();

        if (isFormValid && this.selectedSpecialties.length > 0) {
            // استفاده از formValue که قبلاً تعریف شده
            const specialtyIds = this.selectedSpecialties
                .filter(s => s.specialtyId > 0)
                .map(s => s.specialtyId);

            // اگر کاربر پیدا نشده، کد ملی و اطلاعات را ارسال کن تا API کاربر را ایجاد کند
            const data: any = {
                medicalCode: formValue.medicalCode,
                prefix: formValue.prefix ?? DoctorPrefix.None,
                scientificDegree: formValue.scientificDegree ?? ScientificDegree.None,
                userId: formValue.userId,
                clinicId: formValue.clinicId,
                specialtyIds: specialtyIds,
                medicalConditionIds: this.selectedMedicalConditions.length > 0 ? this.selectedMedicalConditions : undefined
            };

            // ارسال کد ملی فقط اگر وجود داشته باشد
            if (this.userSearchValue && this.userSearchValue.trim()) {
                data.nationalCode = this.userSearchValue.trim();
            }

            // همیشه اطلاعات کاربر را ارسال کن (برای ایجاد یا به‌روزرسانی)
            data.firstName = formValue.firstName || '';
            data.lastName = formValue.lastName || '';
            data.phone = formValue.phoneNumber || '';
            data.cityId = formValue.cityId || undefined;
            data.gender = formValue.gender || undefined;
            data.birthDate = formValue.birthDate || undefined;

            if (this.isEditMode && this.doctorId) {
                const updateData = { ...data, id: this.doctorId };
                if (this.selectedFile) {
                    this.doctorService.updateWithFile(updateData, this.selectedFile).subscribe({
                        next: () => {
                            this.snackbarService.success('پزشک با موفقیت ویرایش شد', 'بستن', 3000);
                            this.router.navigate(['/panel/doctors']);
                        },
                        error: (error) => {
                            const errorMessage = error.error?.message || 'خطا در ویرایش پزشک';
                            this.snackbarService.error(errorMessage, 'بستن', 5000);
                        }
                    });
                } else {
                    this.doctorService.update(updateData).subscribe({
                        next: () => {
                            this.snackbarService.success('پزشک با موفقیت ویرایش شد', 'بستن', 3000);
                            this.router.navigate(['/panel/doctors']);
                        },
                        error: (error) => {
                            const errorMessage = error.error?.message || 'خطا در ویرایش پزشک';
                            this.snackbarService.error(errorMessage, 'بستن', 5000);
                        }
                    });
                }
            } else {
                if (this.selectedFile) {
                    this.doctorService.createWithFile(data, this.selectedFile).subscribe({
                        next: () => {
                            this.snackbarService.success('پزشک با موفقیت اضافه شد', 'بستن', 3000);
                            this.router.navigate(['/panel/doctors']);
                        },
                        error: (error) => {
                            const errorMessage = error.error?.message || 'خطا در افزودن پزشک';
                            this.snackbarService.error(errorMessage, 'بستن', 5000);
                        }
                    });
                } else {
                    this.doctorService.create(data).subscribe({
                        next: () => {
                            this.snackbarService.success('پزشک با موفقیت اضافه شد', 'بستن', 3000);
                            this.router.navigate(['/panel/doctors']);
                        },
                        error: (error) => {
                            const errorMessage = error.error?.message || 'خطا در افزودن پزشک';
                            this.snackbarService.error(errorMessage, 'بستن', 5000);
                        }
                    });
                }
            }
        } else {
            // این بخش نباید اجرا شود چون خطاها در ابتدای متد بررسی می‌شوند
            // اما به عنوان fail-safe نگه داشته می‌شود
            this.snackbarService.error('لطفاً تمام فیلدهای الزامی را پر کنید', 'بستن', 3000);
        }
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
                        this.profilePicturePreview = e.target.result;
                    };
                    reader.readAsDataURL(this.selectedFile);
                }
                // پاک کردن input برای امکان انتخاب مجدد همان فایل
                input.value = '';
            });
        }
    }

    removeFile() {
        this.selectedFile = null;
        this.profilePicturePreview = null;
    }

    cancel() {
        this.router.navigate(['/panel/doctors']);
    }

    ngOnDestroy() {
        if (this.clinicSubscription) {
            this.clinicSubscription.unsubscribe();
        }
        this.citySearchSubject.complete();
        this.medicalConditionSearchSubject.complete();
    }
}
