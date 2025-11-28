import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { ShiftService } from '../../services/shift.service';
import { ShiftDialogComponent } from './shift-dialog.component';
import { Shift } from '../../models/shift.model';

@Component({
  selector: 'app-shifts',
  templateUrl: './shifts.component.html',
  styleUrls: ['./shifts.component.scss']
})
export class ShiftsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'startTime', 'endTime', 'description', 'actions'];
  shifts: Shift[] = [];
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
    private shiftService: ShiftService
  ) {}

  ngOnInit() {
    this.loadShifts();
  }

  loadShifts() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // Add filter if exists
    if (this.filterValue && this.filterValue.trim()) {
      params.filters = this.filterValue.trim();
    }

    this.shiftService.getAll(params).subscribe({
      next: (result) => {
        if (result && result.items) {
          this.shifts = result.items;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.currentPage = result.page;
        } else {
          this.shifts = [];
          this.totalCount = 0;
          this.totalPages = 0;
        }
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری شیفت‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadShifts();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadShifts();
  }

  openAddDialog() {
    this.dialogService.open(ShiftDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result) {
        this.saveShift(result);
      }
    });
  }

  editShift(shift: Shift) {
    this.dialogService.open(ShiftDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: shift
    }).subscribe(result => {
      if (result) {
        this.saveShift(result, shift.id);
      }
    });
  }

  deleteShift(shift: Shift) {
    if (shift.id) {
      this.dialogService.confirm({
        title: 'حذف شیفت',
        message: `آیا از حذف شیفت "${shift.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.shiftService.delete(shift.id!).subscribe({
            next: () => {
              // If current page becomes empty after deletion, go to previous page
              if (this.shifts.length === 1 && this.currentPage > 1) {
                this.currentPage--;
              }
              this.loadShifts();
              this.snackbarService.success('شیفت با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف شیفت';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveShift(shiftData: any, id?: number) {
    if (id) {
      // ویرایش شیفت موجود
      const updateData = { ...shiftData, id };
      this.shiftService.update(updateData).subscribe({
        next: () => {
          this.snackbarService.success('شیفت با موفقیت ویرایش شد', 'بستن', 3000);
          this.loadShifts();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش شیفت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      // افزودن شیفت جدید
      this.shiftService.create(shiftData).subscribe({
        next: () => {
          this.snackbarService.success('شیفت با موفقیت اضافه شد', 'بستن', 3000);
          this.loadShifts();
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن شیفت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadShifts();
  }
}
