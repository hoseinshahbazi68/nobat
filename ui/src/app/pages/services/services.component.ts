import { Component, OnInit } from '@angular/core';
import { Service } from '../../models/service.model';
import { DialogService } from '../../services/dialog.service';
import { ServiceService } from '../../services/service.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ServiceDialogComponent } from './service-dialog.component';

@Component({
  selector: 'app-services',
  templateUrl: './services.component.html',
  styleUrls: ['./services.component.scss']
})
export class ServicesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'description', 'actions'];
  services: Service[] = [];
  filteredServices: Service[] = [];
  filterValue: string = '';
  isLoading = false;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private serviceService: ServiceService
  ) { }

  ngOnInit() {
    this.loadServices();
  }

  loadServices() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // Add filter if exists
    if (this.filterValue && this.filterValue.trim()) {
      const searchTerm = this.filterValue.trim();
      // جستجو در name و description
      params.filters = `Name@=*${searchTerm}*|Description@=*${searchTerm}*`;
    }

    this.serviceService.getAll(params).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.services = result.items;
          this.filteredServices = [...this.services];
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.currentPage = result.page;
        } else {
          this.services = [];
          this.filteredServices = [];
          this.totalCount = 0;
          this.totalPages = 0;
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری خدمات';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadServices();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadServices();
  }

  openAddDialog() {
    this.dialogService.open(ServiceDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result) {
        this.saveService(result);
      }
    });
  }

  editService(service: Service) {
    this.dialogService.open(ServiceDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: service
    }).subscribe(result => {
      if (result) {
        this.saveService(result, service.id);
      }
    });
  }

  deleteService(service: Service) {
    if (service.id) {
      this.dialogService.confirm({
        title: 'حذف خدمت',
        message: `آیا از حذف خدمت "${service.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.serviceService.delete(service.id!).subscribe({
            next: () => {
              // If current page becomes empty after deletion, go to previous page
              if (this.services.length === 1 && this.currentPage > 1) {
                this.currentPage--;
              }
              this.loadServices();
              this.snackbarService.success('خدمت با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف خدمت';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveService(serviceData: any, id?: number) {
    if (id) {
      const updateData = { ...serviceData, id };
      this.serviceService.update(updateData).subscribe({
        next: () => {
          this.snackbarService.success('خدمت با موفقیت ویرایش شد', 'بستن', 3000);
          this.loadServices();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش خدمت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.serviceService.create(serviceData).subscribe({
        next: () => {
          this.snackbarService.success('خدمت با موفقیت اضافه شد', 'بستن', 3000);
          this.loadServices();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن خدمت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadServices();
  }
}
