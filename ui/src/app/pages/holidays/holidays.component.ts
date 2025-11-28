import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { HolidayService } from '../../services/holiday.service';
import { HolidayDialogComponent } from './holiday-dialog.component';
import { PersianDateService } from '../../services/persian-date.service';
import { Holiday } from '../../models/holiday.model';

@Component({
  selector: 'app-holidays',
  templateUrl: './holidays.component.html',
  styleUrls: ['./holidays.component.scss']
})
export class HolidaysComponent implements OnInit {
  displayedColumns: string[] = ['id', 'date', 'name', 'description', 'actions'];
  holidays: Holiday[] = [];
  filteredHolidays: Holiday[] = [];
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
    private holidayService: HolidayService,
    public persianDateService: PersianDateService
  ) {}

  ngOnInit() {
    this.loadHolidays();
  }

  loadHolidays() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // Add filter if exists
    if (this.filterValue && this.filterValue.trim()) {
      // Format filter for Sieve: Name@=*{value}*|Description@=*{value}* (OR condition)
      params.filters = `Name@=*${this.filterValue.trim()}*|Description@=*${this.filterValue.trim()}*`;
    }

    this.holidayService.getAll(params).subscribe({
      next: (result) => {
        console.log('Holidays loaded:', result);
        this.holidays = result.items || [];
        this.filteredHolidays = [...this.holidays];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading holidays:', error);
        const errorMessage = error?.error?.message || error?.message || 'خطا در بارگذاری تعطیلات';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadHolidays();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadHolidays();
  }

  openAddDialog() {
    this.dialogService.open(HolidayDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result) {
        this.saveHoliday(result);
      }
    });
  }

  editHoliday(holiday: Holiday) {
    this.dialogService.open(HolidayDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: holiday
    }).subscribe(result => {
      if (result) {
        this.saveHoliday(result, holiday.id);
      }
    });
  }

  deleteHoliday(holiday: Holiday) {
    if (holiday.id) {
      this.dialogService.confirm({
        title: 'حذف تعطیل',
        message: `آیا از حذف تعطیل "${holiday.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.holidayService.delete(holiday.id!).subscribe({
            next: () => {
              // If current page becomes empty after deletion, go to previous page
              if (this.holidays.length === 1 && this.currentPage > 1) {
                this.currentPage--;
              }
              this.loadHolidays();
              this.snackbarService.success('تعطیل با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف تعطیل';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveHoliday(holidayData: any, id?: number) {
    console.log('saveHoliday called with:', holidayData, 'id:', id);

    if (id) {
      const updateData = { ...holidayData, id };
      console.log('Updating holiday:', updateData);
      this.holidayService.update(updateData).subscribe({
        next: (updatedHoliday) => {
          console.log('Holiday updated:', updatedHoliday);
          this.snackbarService.success('تعطیل با موفقیت ویرایش شد', 'بستن', 3000);
          this.loadHolidays();
        },
        error: (error) => {
          console.error('Error updating holiday:', error);
          const errorMessage = error?.error?.message || error?.message || error?.error || 'خطا در ویرایش تعطیل';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      console.log('Creating holiday:', holidayData);
      this.holidayService.create(holidayData).subscribe({
        next: (newHoliday) => {
          console.log('Holiday created:', newHoliday);
          this.snackbarService.success('تعطیل با موفقیت اضافه شد', 'بستن', 3000);
          this.loadHolidays();
        },
        error: (error) => {
          console.error('Error creating holiday:', error);
          const errorMessage = error?.error?.message || error?.message || error?.error || 'خطا در افزودن تعطیل';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadHolidays();
  }
}
