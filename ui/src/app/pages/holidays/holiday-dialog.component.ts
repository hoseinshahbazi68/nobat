import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PersianDateService } from '../../services/persian-date.service';
import { Holiday } from '../../models/holiday.model';

@Component({
  selector: 'app-holiday-dialog',
  templateUrl: './holiday-dialog.component.html',
  styleUrls: ['./holiday-dialog.component.scss']
})
export class HolidayDialogComponent implements OnInit {
  holidayForm: FormGroup;

  data: Holiday | null = null;
  dialogRef: any = null;

  constructor(
    private fb: FormBuilder,
    private persianDateService: PersianDateService
  ) {
    this.holidayForm = this.fb.group({
      date: [null, Validators.required],
      name: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit() {
    if (this.data) {
      this.holidayForm.patchValue(this.data);
    }
  }

  save() {
    if (this.holidayForm.valid) {
      const formValue = this.holidayForm.value;

      // تبدیل تاریخ از Date object به string (yyyy-MM-dd)
      if (formValue.date) {
        if (formValue.date instanceof Date) {
          formValue.date = formValue.date.toISOString().split('T')[0];
        } else if (typeof formValue.date === 'string' && formValue.date.includes('/')) {
          // اگر تاریخ به صورت شمسی است، به میلادی تبدیل کن
          const gregorianDate = this.persianDateService.toGregorian(formValue.date);
          formValue.date = gregorianDate.toISOString().split('T')[0];
        }
      }

      if (this.data?.id) {
        formValue.id = this.data.id;
      }
      if (this.dialogRef) {
        this.dialogRef.close(formValue);
      }
    }
  }

  cancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
