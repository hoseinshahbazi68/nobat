import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { DoctorVisitInfoService } from '../../services/doctor-visit-info.service';
import { DoctorVisitInfo } from '../../models/doctor-visit-info.model';
import { DoctorVisitInfoDialogComponent } from './doctor-visit-info-dialog.component';
import { DoctorService } from '../../services/doctor.service';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-doctor-visit-infos',
  templateUrl: './doctor-visit-infos.component.html',
  styleUrls: ['./doctor-visit-infos.component.scss']
})
export class DoctorVisitInfosComponent implements OnInit {
  displayedColumns: string[] = ['id', 'doctorName', 'medicalCode', 'about', 'clinicAddress', 'clinicPhone', 'officeHours', 'actions'];
  visitInfos: DoctorVisitInfo[] = [];
  filteredVisitInfos: DoctorVisitInfo[] = [];
  filterValue: string = '';
  isLoading = false;
  doctors: Doctor[] = [];

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  selectedDoctorId: number | null = null;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private visitInfoService: DoctorVisitInfoService,
    private doctorService: DoctorService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    // Check if doctorId is provided in query params
    this.route.queryParams.subscribe((params: any) => {
      if (params['doctorId']) {
        this.selectedDoctorId = +params['doctorId'];
      }
    });
    this.loadDoctors();
    this.loadVisitInfos();
  }

  loadDoctors() {
    this.doctorService.getAll({ page: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.doctors = result.items.map((d: any) => ({
          id: d.id,
          medicalCode: d.medicalCode,
          firstName: d.firstName,
          lastName: d.lastName,
          nationalCode: d.nationalCode
        }));
      },
      error: (error) => {
        console.error('Error loading doctors:', error);
      }
    });
  }

  loadVisitInfos() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // Add filter if filterValue exists
    if (this.filterValue && this.filterValue.trim()) {
      params.filters = `doctorName@=*${this.filterValue.trim()}*`;
    }

    // Add doctorId filter if selected
    if (this.selectedDoctorId) {
      if (params.filters) {
        params.filters += `,doctorId==${this.selectedDoctorId}`;
      } else {
        params.filters = `doctorId==${this.selectedDoctorId}`;
      }
    }

    this.visitInfoService.getAll(params).subscribe({
      next: (result) => {
        this.visitInfos = result.items;
        this.filteredVisitInfos = [...this.visitInfos];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error?.message || error?.error?.message || 'خطا در بارگذاری اطلاعات ویزیت';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadVisitInfos();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadVisitInfos();
  }

  openAddDialog() {
    this.dialogService.open(DoctorVisitInfoDialogComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: { visitInfo: null, doctors: this.doctors }
    }).subscribe(result => {
      if (result && typeof result === 'object' && result.doctorId) {
        this.saveVisitInfo(result);
      }
    });
  }

  editVisitInfo(visitInfo: DoctorVisitInfo) {
    this.dialogService.open(DoctorVisitInfoDialogComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: { visitInfo, doctors: this.doctors }
    }).subscribe(result => {
      if (result) {
        this.saveVisitInfo(result, visitInfo.id);
      }
    });
  }

  deleteVisitInfo(visitInfo: DoctorVisitInfo) {
    if (visitInfo.id) {
      this.dialogService.confirm({
        title: 'حذف اطلاعات ویزیت',
        message: `آیا از حذف اطلاعات ویزیت پزشک "${visitInfo.doctorName || visitInfo.medicalCode}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.visitInfoService.delete(visitInfo.id).subscribe({
            next: () => {
              this.visitInfos = this.visitInfos.filter(vi => vi.id !== visitInfo.id);
              this.filteredVisitInfos = [...this.visitInfos];
              this.applyFilter();
              this.snackbarService.success('اطلاعات ویزیت با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error?.message || error?.error?.message || 'خطا در حذف اطلاعات ویزیت';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveVisitInfo(visitInfoData: any, id?: number) {
    if (!visitInfoData || !visitInfoData.doctorId) {
      this.snackbarService.error('لطفا پزشک را انتخاب کنید', 'بستن', 3000);
      return;
    }

    if (id) {
      const updateData = { ...visitInfoData, id };
      this.visitInfoService.update(updateData).subscribe({
        next: (updatedVisitInfo) => {
          const index = this.visitInfos.findIndex(vi => vi.id === id);
          if (index !== -1) {
            this.visitInfos[index] = updatedVisitInfo;
            this.filteredVisitInfos = [...this.visitInfos];
            this.applyFilter();
          } else {
            this.loadVisitInfos();
          }
          this.snackbarService.success('اطلاعات ویزیت با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در ویرایش اطلاعات ویزیت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.visitInfoService.create(visitInfoData).subscribe({
        next: (newVisitInfo) => {
          this.visitInfos = [...this.visitInfos, newVisitInfo];
          this.filteredVisitInfos = [...this.visitInfos];
          this.applyFilter();
          this.snackbarService.success('اطلاعات ویزیت با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در افزودن اطلاعات ویزیت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadVisitInfos();
  }

  getDoctorName(visitInfo: DoctorVisitInfo): string {
    if (visitInfo.doctorName) {
      return visitInfo.doctorName;
    }
    const doctor = this.doctors.find(d => d.id === visitInfo.doctorId);
    if (doctor) {
      return `${doctor.firstName || ''} ${doctor.lastName || ''}`.trim() || visitInfo.medicalCode || '-';
    }
    return visitInfo.medicalCode || '-';
  }
}
