import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ServiceTariffService } from '../../services/service-tariff.service';
import { ServiceService } from '../../services/service.service';
import { InsuranceService } from '../../services/insurance.service';
import { DoctorService } from '../../services/doctor.service';
import { ClinicService } from '../../services/clinic.service';
import { TariffDialogComponent, TariffDialogData } from './tariff-dialog.component';
import { ServiceTariff } from '../../models/service-tariff.model';
import { Service } from '../../models/service.model';
import { Insurance } from '../../models/insurance.model';
import { Doctor } from '../../models/doctor.model';
import { Clinic } from '../../models/clinic.model';

@Component({
  selector: 'app-service-tariffs',
  templateUrl: './service-tariffs.component.html',
  styleUrls: ['./service-tariffs.component.scss']
})
export class ServiceTariffsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'serviceName', 'insuranceName', 'doctorName', 'clinicName', 'price', 'visitDuration', 'actions'];
  allTariffs: ServiceTariff[] = [];
  filteredTariffs: ServiceTariff[] = [];
  services: Service[] = [];
  insurances: Insurance[] = [];
  doctors: Doctor[] = [];
  clinics: Clinic[] = [];
  selectedClinic: Clinic | null = null;
  isLoading = false;
  selectedDoctorId: number | null = null;
  selectedDoctorName: string = '';
  showAllTariffs: boolean = true;
  filterValue: string = '';

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private route: ActivatedRoute,
    private router: Router,
    private serviceTariffService: ServiceTariffService,
    private serviceService: ServiceService,
    private insuranceService: InsuranceService,
    private doctorService: DoctorService,
    private clinicService: ClinicService
  ) {}

  ngOnInit() {
    this.loadServices();
    this.loadInsurances();
    this.loadDoctors();
    this.loadClinics();
    this.loadSelectedClinic();

    // بررسی query params برای فیلتر بر اساس پزشک
    this.route.queryParams.subscribe(params => {
      if (params['doctorId']) {
        this.selectedDoctorId = +params['doctorId'];
        const doctor = this.doctors.find(d => d.id === this.selectedDoctorId);
        if (doctor && doctor.user) {
          this.selectedDoctorName = `${doctor.user.firstName} ${doctor.user.lastName}`;
        } else {
          this.selectedDoctorName = '';
        }
        this.showAllTariffs = false;
        this.filterByDoctor(this.selectedDoctorId);
      } else {
        this.selectedDoctorId = null;
        this.selectedDoctorName = '';
        this.showAllTariffs = true;
        this.loadTariffs();
      }
    });
  }

  loadClinics() {
    this.clinicService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.clinics = result.items;
      },
      error: (error) => {
        console.error('Error loading clinics:', error);
      }
    });
  }

  loadSelectedClinic() {
    this.clinicService.selectedClinic$.subscribe(clinic => {
      this.selectedClinic = clinic;
      if (this.selectedClinic) {
        this.loadTariffs();
      }
    });
  }


  loadServices() {
    this.serviceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.services = result.items;
      },
      error: (error) => {
        console.error('Error loading services:', error);
      }
    });
  }

  loadInsurances() {
    this.insuranceService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.insurances = result.items;
      },
      error: (error) => {
        console.error('Error loading insurances:', error);
      }
    });
  }

  loadDoctors() {
    this.doctorService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.doctors = result.items;
      },
      error: (error) => {
        console.error('Error loading doctors:', error);
      }
    });
  }

  loadTariffs() {
    if (!this.selectedClinic?.id) {
      this.allTariffs = [];
      this.filteredTariffs = [];
      return;
    }
    this.isLoading = true;
    this.serviceTariffService.getAll({ page: 1, pageSize: 100, clinicId: this.selectedClinic.id }).subscribe({
      next: (result) => {
        this.allTariffs = result.items.filter(t => t.clinicId === this.selectedClinic?.id);
        this.filteredTariffs = [...this.allTariffs];
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری تعرفه‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  filterByDoctor(doctorId: number | null) {
    if (doctorId === null) {
      this.filteredTariffs = [...this.allTariffs];
      this.applyFilters();
      return;
    }
    // فیلتر داده‌ها بر اساس پزشک - نمایش تعرفه‌های این پزشک و تعرفه‌های عمومی (بدون پزشک)
    this.filteredTariffs = this.allTariffs.filter(t =>
      !t.doctorId || t.doctorId === doctorId
    );
    this.applyFilters();
  }

  applyFilters() {
    this.applyFilter();
  }

  openAddDialog() {
    if (!this.selectedClinic?.id) {
      this.snackbarService.error('لطفاً ابتدا یک کلینیک انتخاب کنید', 'بستن', 5000);
      return;
    }
    this.dialogService.open(TariffDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: {
        tariff: null,
        services: this.services,
        insurances: this.insurances,
        doctors: this.doctors,
        clinics: this.clinics,
        selectedClinicId: this.selectedClinic.id,
        selectedDoctorId: undefined // برای تعرفه عمومی
      } as TariffDialogData
    }).subscribe(result => {
      if (result) {
        this.saveTariff(result);
      }
    });
  }

  editTariff(tariff: ServiceTariff) {
    this.dialogService.open(TariffDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: {
        tariff: tariff,
        services: this.services,
        insurances: this.insurances,
        doctors: this.doctors,
        clinics: this.clinics,
        selectedClinicId: this.selectedClinic?.id || tariff.clinicId,
        selectedDoctorId: tariff.doctorId
      } as TariffDialogData
    }).subscribe(result => {
      if (result) {
        this.saveTariff(result, tariff.id);
      }
    });
  }

  clearDoctorFilter() {
    this.selectedDoctorId = null;
    this.showAllTariffs = true;
    this.router.navigate(['/panel/service-tariffs']);
    this.loadTariffs();
  }

  deleteTariff(tariff: ServiceTariff) {
    if (tariff.id) {
      this.dialogService.confirm({
        title: 'حذف تعرفه',
        message: `آیا از حذف تعرفه "${tariff.serviceName || ''} - ${tariff.insuranceName || ''}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.serviceTariffService.delete(tariff.id!).subscribe({
            next: () => {
              this.allTariffs = this.allTariffs.filter(t => t.id !== tariff.id);
              this.filteredTariffs = [...this.allTariffs];
              this.applyFilters();
              this.snackbarService.success('تعرفه با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف تعرفه';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveTariff(tariffData: ServiceTariff, id?: number) {
    // اطمینان از اینکه clinicId از کوکی تنظیم شده است
    if (!tariffData.clinicId && this.selectedClinic?.id) {
      tariffData.clinicId = this.selectedClinic.id;
    }

    if (id) {
      const updateData = { ...tariffData, id };
      this.serviceTariffService.update(updateData).subscribe({
        next: (updatedTariff) => {
          const index = this.allTariffs.findIndex(t => t.id === id);
          if (index !== -1) {
            this.allTariffs[index] = updatedTariff;
          }
          if (this.selectedDoctorId) {
            this.filterByDoctor(this.selectedDoctorId);
          } else {
            this.filteredTariffs = [...this.allTariffs];
            this.applyFilters();
          }
          this.snackbarService.success('تعرفه با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش تعرفه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.serviceTariffService.create(tariffData).subscribe({
        next: (newTariff) => {
          this.allTariffs = [...this.allTariffs, newTariff];
          if (this.selectedDoctorId) {
            this.filterByDoctor(this.selectedDoctorId);
          } else {
            this.filteredTariffs = [...this.allTariffs];
            this.applyFilters();
          }
          this.snackbarService.success('تعرفه با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن تعرفه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredTariffs = this.selectedDoctorId
        ? this.allTariffs.filter(t => !t.doctorId || t.doctorId === this.selectedDoctorId)
        : [...this.allTariffs];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
      let baseData = this.selectedDoctorId
      ? this.allTariffs.filter(t => !t.doctorId || t.doctorId === this.selectedDoctorId)
      : [...this.allTariffs];
    this.filteredTariffs = baseData.filter(tariff =>
      (tariff.serviceName && tariff.serviceName.toLowerCase().includes(filter)) ||
      (tariff.insuranceName && tariff.insuranceName.toLowerCase().includes(filter)) ||
      (tariff.doctorName && tariff.doctorName.toLowerCase().includes(filter))
    );
  }
}
