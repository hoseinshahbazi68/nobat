import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ServiceTariffService } from '../../services/service-tariff.service';
import { ServiceService } from '../../services/service.service';
import { InsuranceService } from '../../services/insurance.service';
import { DoctorService } from '../../services/doctor.service';
import { ClinicService } from '../../services/clinic.service';
import { TariffDialogComponent, TariffDialogData } from '../service-tariffs/tariff-dialog.component';
import { ServiceTariff } from '../../models/service-tariff.model';
import { Service } from '../../models/service.model';
import { Insurance } from '../../models/insurance.model';
import { Doctor } from '../../models/doctor.model';
import { Clinic } from '../../models/clinic.model';

@Component({
  selector: 'app-doctor-tariffs',
  templateUrl: './doctor-tariffs.component.html',
  styleUrls: ['./doctor-tariffs.component.scss']
})
export class DoctorTariffsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'serviceName', 'insuranceName', 'price', 'visitDuration', 'actions'];
  allTariffs: ServiceTariff[] = [];
  filteredTariffs: ServiceTariff[] = [];
  services: Service[] = [];
  insurances: Insurance[] = [];
  doctors: Doctor[] = [];
  clinics: Clinic[] = [];
  selectedClinic: Clinic | null = null;
  selectedDoctor: Doctor | null = null;
  doctorId: number | null = null;
  isLoading = false;
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

    // دریافت doctorId از query params
    this.route.queryParams.subscribe(params => {
      if (params['doctorId']) {
        this.doctorId = +params['doctorId'];
        this.loadDoctor(this.doctorId);
      } else {
        this.snackbarService.error('شناسه پزشک مشخص نشده است', 'بستن', 5000);
        this.router.navigate(['/panel/doctors']);
      }
    });
  }

  loadDoctor(id: number) {
    this.doctorService.getById(id).subscribe({
      next: (doctor) => {
        this.selectedDoctor = doctor;
        if (this.selectedClinic) {
          this.loadTariffs();
        }
      },
      error: (error) => {
        console.error('Error loading doctor:', error);
        this.snackbarService.error('خطا در بارگذاری اطلاعات پزشک', 'بستن', 5000);
        this.router.navigate(['/panel/doctors']);
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
      if (this.selectedClinic && this.doctorId) {
        this.loadTariffs();
      } else if (!this.selectedClinic) {
        this.snackbarService.error('لطفاً ابتدا یک کلینیک انتخاب کنید', 'بستن', 5000);
      }
    });
  }

  loadTariffs() {
    if (!this.selectedClinic?.id || !this.doctorId) {
      this.allTariffs = [];
      this.filteredTariffs = [];
      return;
    }
    this.isLoading = true;
    this.serviceTariffService.getAll({
      page: 1,
      pageSize: 100,
      clinicId: this.selectedClinic.id,
      doctorId: this.doctorId
    }).subscribe({
      next: (result) => {
        this.allTariffs = result.items.filter(t =>
          t.clinicId === this.selectedClinic?.id &&
          t.doctorId === this.doctorId
        );
        this.filteredTariffs = [...this.allTariffs];
        this.applyFilter();
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری تعرفه‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredTariffs = [...this.allTariffs];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredTariffs = this.allTariffs.filter(tariff =>
      (tariff.serviceName && tariff.serviceName.toLowerCase().includes(filter)) ||
      (tariff.insuranceName && tariff.insuranceName.toLowerCase().includes(filter))
    );
  }

  openAddDialog() {
    if (!this.selectedClinic?.id || !this.doctorId) {
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
        selectedDoctorId: this.doctorId
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
        selectedClinicId: this.selectedClinic?.id,
        selectedDoctorId: this.doctorId
      } as TariffDialogData
    }).subscribe(result => {
      if (result) {
        this.saveTariff(result, tariff.id);
      }
    });
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
              this.applyFilter();
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
    // اطمینان از اینکه clinicId و doctorId تنظیم شده‌اند
    if (!tariffData.clinicId && this.selectedClinic?.id) {
      tariffData.clinicId = this.selectedClinic.id;
    }
    if (!tariffData.doctorId && this.doctorId) {
      tariffData.doctorId = this.doctorId;
    }

    if (id) {
      const updateData = { ...tariffData, id };
      this.serviceTariffService.update(updateData).subscribe({
        next: (updatedTariff) => {
          const index = this.allTariffs.findIndex(t => t.id === id);
          if (index !== -1) {
            this.allTariffs[index] = updatedTariff;
          }
          this.filteredTariffs = [...this.allTariffs];
          this.applyFilter();
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
          this.filteredTariffs = [...this.allTariffs];
          this.applyFilter();
          this.snackbarService.success('تعرفه با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن تعرفه';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  goBack() {
    this.router.navigate(['/panel/doctors']);
  }

  getDoctorDisplayName(): string {
    if (this.selectedDoctor?.user?.firstName && this.selectedDoctor?.user?.lastName) {
      return `${this.selectedDoctor.user.firstName} ${this.selectedDoctor.user.lastName}`;
    }
    return 'در حال بارگذاری...';
  }
}
