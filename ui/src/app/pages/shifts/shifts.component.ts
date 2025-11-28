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
  filteredShifts: Shift[] = [];
  filterValue: string = '';
  isLoading = false;

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
    this.shiftService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.shifts = result.items;
        this.filteredShifts = [...this.shifts];
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری شیفت‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
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
              this.shifts = this.shifts.filter(s => s.id !== shift.id);
              this.filteredShifts = [...this.shifts];
              this.applyFilter();
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
        next: (updatedShift) => {
          const index = this.shifts.findIndex(s => s.id === id);
          if (index !== -1) {
            this.shifts[index] = updatedShift;
            this.filteredShifts = [...this.shifts];
            this.applyFilter();
          }
          this.snackbarService.success('شیفت با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در ویرایش شیفت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      // افزودن شیفت جدید
      this.shiftService.create(shiftData).subscribe({
        next: (newShift) => {
          this.shifts = [...this.shifts, newShift];
          this.filteredShifts = [...this.shifts];
          this.applyFilter();
          this.snackbarService.success('شیفت با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error.error?.message || 'خطا در افزودن شیفت';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredShifts = [...this.shifts];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredShifts = this.shifts.filter(shift =>
      shift.name.toLowerCase().includes(filter) ||
      shift.startTime.includes(filter) ||
      shift.endTime.includes(filter) ||
      (shift.description && shift.description.toLowerCase().includes(filter))
    );
  }
}
