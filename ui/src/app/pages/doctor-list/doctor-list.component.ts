import { Component, OnInit, HostListener, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Doctor } from '../../models/doctor.model';
import { Specialty } from '../../models/specialty.model';
import { Clinic } from '../../models/clinic.model';
import { Service } from '../../models/service.model';
import { Insurance } from '../../models/insurance.model';
import { MedicalCondition } from '../../models/medical-condition.model';
import { User } from '../../models/user.model';
import { DialogService } from '../../services/dialog.service';
import { AuthService } from '../../services/auth.service';
import { DoctorService } from '../../services/doctor.service';
import { SpecialtyService } from '../../services/specialty.service';
import { ClinicService } from '../../services/clinic.service';
import { ServiceService } from '../../services/service.service';
import { InsuranceService } from '../../services/insurance.service';
import { MedicalConditionService } from '../../services/medical-condition.service';
import { ChatModalComponent } from '../../components/chat-modal/chat-modal.component';
import { ChangePasswordDialogComponent } from '../../components/change-password-dialog/change-password-dialog.component';

@Component({
  selector: 'app-doctor-list',
  templateUrl: './doctor-list.component.html',
  styleUrls: ['./doctor-list.component.scss']
})
export class DoctorListComponent implements OnInit {
  doctors: Doctor[] = [];
  filteredDoctors: Doctor[] = [];
  specialties: Specialty[] = [];
  clinics: Clinic[] = [];
  medicalConditions: MedicalCondition[] = [];
  selectedMedicalCondition: string = '';
  medicalConditionSearchQuery: string = '';
  medicalConditionDropdownOpen: boolean = false;
  services: Service[] = [];
  insurances: Insurance[] = [];
  availableClinics: Clinic[] = [];
  availableCities: string[] = [];
  availableServices: Service[] = [];
  availableInsurances: Insurance[] = [];

  selectedSpecialty: number | null = null;
  selectedGender: 'male' | 'female' | null = null;
  selectedCity: string | null = null;
  selectedClinicId: number | null = null;
  selectedServiceId: number | null = null;
  selectedInsuranceId: number | null = null;
  searchQuery: string = '';

  // Dropdown states
  specialtySearchQuery: string = '';
  citySearchQuery: string = '';
  clinicSearchQuery: string = '';
  serviceSearchQuery: string = '';
  insuranceSearchQuery: string = '';
  specialtyDropdownOpen: boolean = false;
  cityDropdownOpen: boolean = false;
  clinicDropdownOpen: boolean = false;
  serviceDropdownOpen: boolean = false;
  insuranceDropdownOpen: boolean = false;

  sidebarOpen = false;
  isMobile = false;
  isLoading = false;
  currentUser: User | null = null;
  isAuthenticated = false;
  userPanelOpen = false;

  constructor(
    private router: Router,
    private dialogService: DialogService,
    private authService: AuthService,
    private doctorService: DoctorService,
    private specialtyService: SpecialtyService,
    private clinicService: ClinicService,
    private serviceService: ServiceService,
    private insuranceService: InsuranceService,
    private medicalConditionService: MedicalConditionService
  ) { }

  ngOnInit() {
    this.checkMobile();
    this.checkAuthentication();
    this.loadSpecialties();
    this.loadClinics();
    this.loadServices();
    this.loadInsurances();
    this.loadMedicalConditions();
    this.loadDoctors();
  }

  checkAuthentication() {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.currentUser = this.authService.getCurrentUser();
    }
  }

  getUserRoleDisplay(): string {
    if (!this.currentUser || !this.currentUser.roles || this.currentUser.roles.length === 0) {
      return 'کاربر';
    }
    // تبدیل نام نقش‌ها به فارسی
    const roleMap: { [key: string]: string } = {
      'Admin': 'مدیر سیستم',
      'User': 'کاربر',
      'Doctor': 'پزشک',
      'Receptionist': 'منشی'
    };
    return this.currentUser.roles.map(role => roleMap[role] || role).join('، ');
  }

  getUserFullName(): string {
    if (!this.currentUser) {
      return 'کاربر';
    }
    const firstName = this.currentUser.firstName || '';
    const lastName = this.currentUser.lastName || '';
    const fullName = `${firstName} ${lastName}`.trim();
    return fullName || this.currentUser.nationalCode || 'کاربر';
  }

  openChangePasswordDialog() {
    this.closeUserPanel();
    this.dialogService.open(ChangePasswordDialogComponent, {
      width: '500px',
      maxWidth: '90vw'
    }).subscribe();
  }

  goToProfile() {
    this.closeUserPanel();
    this.router.navigate(['/panel/profile']);
  }

  logout() {
    this.closeUserPanel();
    this.authService.logout();
    this.isAuthenticated = false;
    this.currentUser = null;
    this.router.navigate(['/home']);
  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    this.checkMobile();
    if (!this.isMobile) {
      this.sidebarOpen = false;
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    // بستن dropdown‌ها اگر کلیک بیرون از dropdown-container باشد
    if (!target.closest('.dropdown-container')) {
      this.closeAllDropdowns();
    }
    // بستن پنل کاربر اگر کلیک بیرون از user-panel باشد
    if (!target.closest('.user-panel-container')) {
      this.userPanelOpen = false;
    }
  }

  toggleUserPanel() {
    this.userPanelOpen = !this.userPanelOpen;
  }

  closeUserPanel() {
    this.userPanelOpen = false;
  }

  checkMobile() {
    this.isMobile = window.innerWidth < 768;
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  loadSpecialties() {
    this.specialtyService.getAll({
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        this.specialties = result.items || [];
      },
      error: (error) => {
        console.error('خطا در بارگذاری تخصص‌ها:', error);
        this.specialties = [];
      }
    });
  }

  loadClinics() {
    this.clinicService.getAll({
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        this.clinics = result.items || [];
        this.extractCities();
        this.extractClinics();
      },
      error: (error) => {
        console.error('خطا در بارگذاری کلینیک‌ها:', error);
        this.clinics = [];
      }
    });
  }

  loadServices() {
    this.serviceService.getAll({
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        this.services = result.items || [];
        this.extractServices();
      },
      error: (error) => {
        console.error('خطا در بارگذاری خدمات:', error);
        this.services = [];
      }
    });
  }

  loadInsurances() {
    this.insuranceService.getAll({
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        this.insurances = result.items || [];
        this.extractInsurances();
      },
      error: (error) => {
        console.error('خطا در بارگذاری بیمه‌ها:', error);
        this.insurances = [];
      }
    });
  }

  loadMedicalConditions() {
    this.medicalConditionService.getAll({
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (result: any) => {
        this.medicalConditions = result.items || [];
      },
      error: (error: any) => {
        console.error('خطا در بارگذاری علائم پزشکی:', error);
        this.medicalConditions = [];
      }
    });
  }

  extractCities() {
    const cities = new Set<string>();
    this.doctors.forEach(doctor => {
      if (doctor.clinics) {
        doctor.clinics.forEach(clinic => {
          if (clinic.address) {
            // استخراج شهر از آدرس (فرض بر اینکه شهر در ابتدای آدرس است)
            const cityMatch = clinic.address.match(/^(.*?)(?:،|,|$)/);
            if (cityMatch && cityMatch[1]) {
              const city = cityMatch[1].trim();
              if (city) {
                cities.add(city);
              }
            }
          }
        });
      }
    });
    this.availableCities = Array.from(cities).sort();
  }

  extractClinics() {
    const clinicMap = new Map<number, Clinic>();
    this.doctors.forEach(doctor => {
      if (doctor.clinics) {
        doctor.clinics.forEach(clinic => {
          if (clinic.id && !clinicMap.has(clinic.id)) {
            clinicMap.set(clinic.id, clinic);
          }
        });
      }
    });
    this.availableClinics = Array.from(clinicMap.values()).sort((a, b) =>
      (a.name || '').localeCompare(b.name || '')
    );
  }

  extractServices() {
    // از آنجایی که در Doctor model فعلی ServiceTariffs ممکن است نباشد،
    // همه خدمات را نمایش می‌دهیم و فیلتر در سمت کلاینت انجام می‌شود
    this.availableServices = [...this.services].sort((a, b) =>
      (a.name || '').localeCompare(b.name || '')
    );
  }

  extractInsurances() {
    // همه بیمه‌های فعال را نمایش می‌دهیم
    this.availableInsurances = this.insurances
      .filter(insurance => insurance.isActive)
      .sort((a, b) => (a.name || '').localeCompare(b.name || ''));
  }

  loadDoctors() {
    this.isLoading = true;

    // استفاده از API جستجو برای بارگذاری اولیه
    this.doctorService.search({
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        this.doctors = result.items || [];
        this.extractCities();
        this.extractClinics();
        this.extractServices();
        this.extractInsurances();
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('خطا در بارگذاری پزشکان:', error);
        this.doctors = [];
        this.filteredDoctors = [];
        this.isLoading = false;
      }
    });
  }

  // Filtered lists for dropdowns
  getFilteredSpecialties(): Specialty[] {
    if (!this.specialtySearchQuery.trim()) {
      return this.specialties;
    }
    const query = this.specialtySearchQuery.toLowerCase();
    return this.specialties.filter(s =>
      s.name?.toLowerCase().includes(query)
    );
  }

  getFilteredCities(): string[] {
    if (!this.citySearchQuery.trim()) {
      return this.availableCities;
    }
    const query = this.citySearchQuery.toLowerCase();
    return this.availableCities.filter(c =>
      c.toLowerCase().includes(query)
    );
  }

  getFilteredClinics(): Clinic[] {
    if (!this.clinicSearchQuery.trim()) {
      return this.availableClinics;
    }
    const query = this.clinicSearchQuery.toLowerCase();
    return this.availableClinics.filter(c =>
      c.name?.toLowerCase().includes(query)
    );
  }

  getFilteredServices(): Service[] {
    if (!this.serviceSearchQuery.trim()) {
      return this.availableServices;
    }
    const query = this.serviceSearchQuery.toLowerCase();
    return this.availableServices.filter(s =>
      s.name?.toLowerCase().includes(query)
    );
  }

  getFilteredInsurances(): Insurance[] {
    if (!this.insuranceSearchQuery.trim()) {
      return this.availableInsurances;
    }
    const query = this.insuranceSearchQuery.toLowerCase();
    return this.availableInsurances.filter(i =>
      i.name?.toLowerCase().includes(query) ||
      i.code?.toLowerCase().includes(query)
    );
  }

  // Get selected item names
  getSelectedSpecialtyName(): string {
    if (this.selectedSpecialty === null) return 'همه تخصص‌ها';
    const specialty = this.specialties.find(s => s.id === this.selectedSpecialty);
    return specialty?.name || 'همه تخصص‌ها';
  }

  getSelectedCityName(): string {
    return this.selectedCity || 'همه شهرها';
  }

  getSelectedClinicName(): string {
    if (this.selectedClinicId === null) return 'همه کلینیک‌ها';
    const clinic = this.availableClinics.find(c => c.id === this.selectedClinicId);
    return clinic?.name || 'همه کلینیک‌ها';
  }

  getSelectedServiceName(): string {
    if (this.selectedServiceId === null) return 'همه خدمات';
    const service = this.availableServices.find(s => s.id === this.selectedServiceId);
    return service?.name || 'همه خدمات';
  }

  getSelectedInsuranceName(): string {
    if (this.selectedInsuranceId === null) return 'همه بیمه‌ها';
    const insurance = this.availableInsurances.find(i => i.id === this.selectedInsuranceId);
    return insurance?.name || 'همه بیمه‌ها';
  }

  selectSpecialty(specialtyId: number | null) {
    this.selectedSpecialty = this.selectedSpecialty === specialtyId ? null : specialtyId;
    this.specialtyDropdownOpen = false;
    this.specialtySearchQuery = '';
    this.applyFilters();
  }

  selectGender(gender: 'male' | 'female' | null) {
    this.selectedGender = this.selectedGender === gender ? null : gender;
    this.applyFilters();
  }

  selectCity(city: string | null) {
    this.selectedCity = this.selectedCity === city ? null : city;
    this.cityDropdownOpen = false;
    this.citySearchQuery = '';
    this.applyFilters();
  }

  selectClinic(clinicId: number | null) {
    this.selectedClinicId = this.selectedClinicId === clinicId ? null : clinicId;
    this.clinicDropdownOpen = false;
    this.clinicSearchQuery = '';
    this.applyFilters();
  }

  selectService(serviceId: number | null) {
    this.selectedServiceId = this.selectedServiceId === serviceId ? null : serviceId;
    this.serviceDropdownOpen = false;
    this.serviceSearchQuery = '';
    this.applyFilters();
  }

  selectInsurance(insuranceId: number | null) {
    this.selectedInsuranceId = this.selectedInsuranceId === insuranceId ? null : insuranceId;
    this.insuranceDropdownOpen = false;
    this.insuranceSearchQuery = '';
    this.applyFilters();
  }

  selectMedicalCondition(conditionName: string) {
    this.selectedMedicalCondition = this.selectedMedicalCondition === conditionName ? '' : conditionName;
    this.medicalConditionDropdownOpen = false;
    this.medicalConditionSearchQuery = '';
    this.onSearch(); // جستجو را دوباره انجام بده
  }

  getFilteredMedicalConditions(): MedicalCondition[] {
    if (!this.medicalConditionSearchQuery.trim()) {
      return this.medicalConditions;
    }
    const query = this.medicalConditionSearchQuery.toLowerCase();
    return this.medicalConditions.filter(mc =>
      mc.name?.toLowerCase().includes(query) ||
      mc.description?.toLowerCase().includes(query)
    );
  }

  toggleMedicalConditionDropdown() {
    this.medicalConditionDropdownOpen = !this.medicalConditionDropdownOpen;
    if (this.medicalConditionDropdownOpen) {
      this.closeOtherDropdowns('medicalCondition');
    }
  }

  toggleSpecialtyDropdown() {
    this.specialtyDropdownOpen = !this.specialtyDropdownOpen;
    if (this.specialtyDropdownOpen) {
      this.closeOtherDropdowns('specialty');
    }
  }

  toggleCityDropdown() {
    this.cityDropdownOpen = !this.cityDropdownOpen;
    if (this.cityDropdownOpen) {
      this.closeOtherDropdowns('city');
    }
  }

  toggleClinicDropdown() {
    this.clinicDropdownOpen = !this.clinicDropdownOpen;
    if (this.clinicDropdownOpen) {
      this.closeOtherDropdowns('clinic');
    }
  }

  toggleServiceDropdown() {
    this.serviceDropdownOpen = !this.serviceDropdownOpen;
    if (this.serviceDropdownOpen) {
      this.closeOtherDropdowns('service');
    }
  }

  toggleInsuranceDropdown() {
    this.insuranceDropdownOpen = !this.insuranceDropdownOpen;
    if (this.insuranceDropdownOpen) {
      this.closeOtherDropdowns('insurance');
    }
  }

  closeOtherDropdowns(except: string) {
    if (except !== 'specialty') this.specialtyDropdownOpen = false;
    if (except !== 'city') this.cityDropdownOpen = false;
    if (except !== 'clinic') this.clinicDropdownOpen = false;
    if (except !== 'service') this.serviceDropdownOpen = false;
    if (except !== 'insurance') this.insuranceDropdownOpen = false;
    if (except !== 'medicalCondition') this.medicalConditionDropdownOpen = false;
  }

  closeAllDropdowns() {
    this.specialtyDropdownOpen = false;
    this.cityDropdownOpen = false;
    this.clinicDropdownOpen = false;
    this.serviceDropdownOpen = false;
    this.insuranceDropdownOpen = false;
    this.medicalConditionDropdownOpen = false;
  }

  onSearch() {
    this.isLoading = true;

    // اگر علائم پزشکی انتخاب شده، از جستجوی بر اساس علائم استفاده کن
    if (this.selectedMedicalCondition && this.selectedMedicalCondition.trim()) {
      this.doctorService.searchByMedicalCondition({
        medicalConditionName: this.selectedMedicalCondition.trim(),
        page: 1,
        pageSize: 100
      }).subscribe({
        next: (result) => {
          this.doctors = result.items || [];
          this.extractCities();
          this.extractClinics();
          this.applyFilters();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('خطا در جستجو بر اساس علائم:', error);
          this.isLoading = false;
        }
      });
    } else {
      // جستجوی عادی
      const searchParams: any = {
        page: 1,
        pageSize: 100
      };

      if (this.searchQuery.trim()) {
        searchParams.query = this.searchQuery.trim();
      }

      this.doctorService.search(searchParams).subscribe({
        next: (result) => {
          this.doctors = result.items || [];
          this.extractCities();
          this.extractClinics();
          this.applyFilters();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('خطا در جستجو:', error);
          this.isLoading = false;
        }
      });
    }
  }

  applyFilters() {
    let filtered = [...this.doctors];

    // فیلتر بر اساس جستجو
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.trim().toLowerCase();
      filtered = filtered.filter(doctor => {
        const name = this.getDoctorName(doctor).toLowerCase();
        const phone = (this.getDoctorPhone(doctor) || '').toLowerCase();
        const email = (this.getDoctorEmail(doctor) || '').toLowerCase();
        const medicalCode = (doctor.medicalCode || '').toLowerCase();
        const specialties = this.getSpecialtiesDisplay(doctor).toLowerCase();
        const clinics = (doctor.clinics || [])
          .map(c => c.name?.toLowerCase() || '')
          .join(' ');
        const addresses = this.getDoctorAddresses(doctor)
          .join(' ')
          .toLowerCase();
        const medicalConditions = (doctor.medicalConditions || [])
          .map(mc => mc.medicalCondition?.name?.toLowerCase() || '')
          .join(' ');

        return name.includes(query) ||
          phone.includes(query) ||
          email.includes(query) ||
          medicalCode.includes(query) ||
          specialties.includes(query) ||
          clinics.includes(query) ||
          addresses.includes(query) ||
          medicalConditions.includes(query);
      });
    }

    // فیلتر بر اساس تخصص
    if (this.selectedSpecialty !== null) {
      const specialty = this.specialties.find(s => s.id === this.selectedSpecialty);
      if (specialty) {
        filtered = filtered.filter(doctor =>
          doctor.specialties?.some(ds =>
            ds.specialty?.id === specialty.id ||
            ds.specialty?.name?.toLowerCase() === specialty.name.toLowerCase()
          ) || false
        );
      }
    }

    // فیلتر بر اساس جنسیت
    if (this.selectedGender !== null) {
      filtered = filtered.filter(doctor => {
        const firstName = (doctor.firstName || doctor.user?.firstName || '').toLowerCase();
        const gender = this.detectGender(firstName);
        return gender === this.selectedGender;
      });
    }

    // فیلتر بر اساس شهر
    if (this.selectedCity !== null) {
      filtered = filtered.filter(doctor => {
        if (!doctor.clinics || doctor.clinics.length === 0) return false;
        return doctor.clinics.some(clinic => {
          if (!clinic.address) return false;
          return clinic.address.includes(this.selectedCity!);
        });
      });
    }

    // فیلتر بر اساس کلینیک
    if (this.selectedClinicId !== null) {
      filtered = filtered.filter(doctor => {
        if (!doctor.clinics || doctor.clinics.length === 0) return false;
        return doctor.clinics.some(clinic => clinic.id === this.selectedClinicId);
      });
    }

    // فیلتر بر اساس خدمت
    // توجه: این فیلتر نیاز به ServiceTariff در مدل Doctor دارد
    // در صورت نیاز می‌توانید از API برای فیلتر کردن استفاده کنید
    if (this.selectedServiceId !== null) {
      // فعلاً فقط در جستجو لحاظ می‌شود
      // بعداً می‌توان از طریق ServiceTariff فیلتر کرد
      filtered = filtered.filter(doctor => {
        // در صورت وجود ServiceTariffs در Doctor model:
        // return doctor.serviceTariffs?.some(st => st.serviceId === this.selectedServiceId) || false;
        return true; // فعلاً همه را نمایش می‌دهد
      });
    }

    // فیلتر بر اساس بیمه
    // توجه: این فیلتر نیاز به ServiceTariff در مدل Doctor دارد
    // در صورت نیاز می‌توانید از API برای فیلتر کردن استفاده کنید
    if (this.selectedInsuranceId !== null) {
      // فعلاً فقط در جستجو لحاظ می‌شود
      // بعداً می‌توان از طریق ServiceTariff فیلتر کرد
      filtered = filtered.filter(doctor => {
        // در صورت وجود ServiceTariffs در Doctor model:
        // return doctor.serviceTariffs?.some(st => st.insuranceId === this.selectedInsuranceId) || false;
        return true; // فعلاً همه را نمایش می‌دهد
      });
    }

    this.filteredDoctors = filtered;
  }

  detectGender(firstName: string): 'male' | 'female' | null {
    const femaleNames = ['فاطمه', 'زهرا', 'مریم', 'سارا', 'نرگس', 'لیلا', 'فریبا',
      'مهسا', 'نیلوفر', 'ریحانه', 'زینب', 'محدثه', 'معصومه', 'طاهره', 'عذرا',
      'نازیلا', 'فیروزه', 'گلناز', 'گلنوش', 'مهتاب', 'ستاره', 'شیدا', 'مینا',
      'پریسا', 'نیلا', 'راضیه', 'فریده', 'طوبی', 'طیبه', 'صغری', 'کبری'];

    const lowerFirstName = firstName.toLowerCase();
    if (femaleNames.some(name => lowerFirstName.includes(name.toLowerCase()))) {
      return 'female';
    }
    return 'male';
  }

  hasActiveFilters(): boolean {
    return this.selectedSpecialty !== null ||
      this.selectedGender !== null ||
      this.selectedCity !== null ||
      this.selectedClinicId !== null ||
      this.selectedServiceId !== null ||
      this.selectedInsuranceId !== null ||
      (this.selectedMedicalCondition !== null && this.selectedMedicalCondition !== '');
  }

  clearAllFilters() {
    this.selectedSpecialty = null;
    this.selectedGender = null;
    this.selectedCity = null;
    this.selectedClinicId = null;
    this.selectedServiceId = null;
    this.selectedInsuranceId = null;
    this.selectedMedicalCondition = '';
    this.searchQuery = '';
    this.specialtySearchQuery = '';
    this.citySearchQuery = '';
    this.clinicSearchQuery = '';
    this.serviceSearchQuery = '';
    this.insuranceSearchQuery = '';
    this.medicalConditionSearchQuery = '';
    this.closeAllDropdowns();
    this.applyFilters();
  }

  clearFilters() {
    this.clearAllFilters();
  }

  getDoctorName(doctor: Doctor): string {
    if (doctor.firstName && doctor.lastName) {
      return `${doctor.firstName} ${doctor.lastName}`;
    }
    if (doctor.user) {
      return `${doctor.user.firstName} ${doctor.user.lastName}`;
    }
    return 'بدون نام';
  }

  getDoctorPhone(doctor: Doctor): string | null {
    return doctor.user?.phoneNumber || null;
  }

  getDoctorEmail(doctor: Doctor): string | null {
    return doctor.user?.email || null;
  }

  getSpecialtiesDisplay(doctor: Doctor): string {
    if (doctor.specialties && doctor.specialties.length > 0) {
      return doctor.specialties.map((ds: any) => ds.specialty?.name || '').filter(Boolean).join('، ');
    }
    return '-';
  }

  getAvatarIcon(doctor: Doctor): string {
    const firstName = (doctor.firstName || doctor.user?.firstName || '').toLowerCase();
    const gender = this.detectGender(firstName);
    return gender === 'female' ? '👩‍⚕️' : '👨‍⚕️';
  }

  getDoctorAddresses(doctor: Doctor): string[] {
    const addresses: string[] = [];
    if (doctor.clinics) {
      doctor.clinics.forEach(clinic => {
        if (clinic.address && clinic.address.trim()) {
          addresses.push(clinic.address);
        }
      });
    }
    return addresses;
  }

  getSpecialtyIcon(specialty: string): string {
    const specialtyLower = specialty.toLowerCase();
    const icons: { [key: string]: string } = {
      'قلب': '❤️',
      'عروق': '❤️',
      'قلب و عروق': '❤️',
      'مغز': '🧠',
      'اعصاب': '🧠',
      'مغز و اعصاب': '🧠',
      'عصب': '🧠',
      'پوست': '✨',
      'مو': '✨',
      'پوست و مو': '✨',
      'چشم': '👁️',
      'چشم پزشکی': '👁️',
      'ارتوپدی': '🦴',
      'داخلی': '🫀',
      'کودک': '👶',
      'کودکان': '👶',
      'زنان': '🤰',
      'زایمان': '🤰',
      'زنان و زایمان': '🤰',
      'جراحی': '🔪',
      'اورولوژی': '🔬',
      'گوارش': '🍽️',
      'غدد': '⚖️',
      'روانپزشکی': '🧘',
      'رادیولوژی': '📷',
      'آسیب شناسی': '🔬',
      'بیهوشی': '😴',
      'اورژانس': '🚑',
      'طب کار': '👷',
      'طب ورزشی': '🏃'
    };

    // جستجوی دقیق
    if (icons[specialty]) {
      return icons[specialty];
    }

    // جستجوی جزئی
    for (const [key, icon] of Object.entries(icons)) {
      if (specialtyLower.includes(key.toLowerCase())) {
        return icon;
      }
    }

    return '⚕️';
  }

  getAvatarGradient(index: number): string {
    const gradients = [
      'linear-gradient(135deg, #06b6d4 0%, #10b981 100%)',
      'linear-gradient(135deg, #8b5cf6 0%, #a78bfa 100%)',
      'linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%)',
      'linear-gradient(135deg, #ec4899 0%, #f472b6 100%)',
      'linear-gradient(135deg, #14b8a6 0%, #2dd4bf 100%)',
      'linear-gradient(135deg, #6366f1 0%, #818cf8 100%)',
      'linear-gradient(135deg, #f97316 0%, #fb923c 100%)',
      'linear-gradient(135deg, #06b6d4 0%, #22d3ee 100%)'
    ];
    return gradients[index % gradients.length];
  }

  bookAppointment(doctor: Doctor) {
    // این تابع می‌تواند به صفحه رزرو نوبت هدایت کند
    console.log('رزرو نوبت برای:', doctor);
    // می‌توانید routing را اضافه کنید:
    // this.router.navigate(['/appointment', doctor.id]);
  }

  openChat() {
    this.dialogService.open(ChatModalComponent, {
      width: '500px',
      maxWidth: '90vw'
    }).subscribe();
  }
}

