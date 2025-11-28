import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DoctorSchedule, DayOfWeek } from '../../models/doctor-schedule.model';
import { PersianDateService } from '../../services/persian-date.service';
import { getAllDaysOfWeek } from '../../utils/day-of-week.util';

export interface DoctorScheduleDialogData {
  schedule: DoctorSchedule | null;
  doctors: any[];
  shifts: any[];
}

@Component({
  selector: 'app-doctor-schedule-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.schedule?.id != null ? 'ویرایش برنامه' : 'افزودن برنامه جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="scheduleForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field">
              <label>پزشک</label>
              <select formControlName="doctorId" required>
                <option value="">انتخاب کنید</option>
                <option *ngFor="let doctor of data?.doctors || []" [value]="doctor.id">{{ doctor.user ? (doctor.user.firstName + ' ' + doctor.user.lastName) : 'بدون نام' }}</option>
              </select>
              <span class="error" *ngIf="scheduleForm.get('doctorId')?.hasError('required') && scheduleForm.get('doctorId')?.touched">لطفا پزشک را انتخاب کنید</span>
            </div>

            <div class="form-field">
              <label>روز هفته</label>
              <select formControlName="dayOfWeek" required>
                <option value="">انتخاب کنید</option>
                <option *ngFor="let day of daysOfWeek" [value]="day.value">{{ day.name }}</option>
              </select>
              <span class="error" *ngIf="scheduleForm.get('dayOfWeek')?.hasError('required') && scheduleForm.get('dayOfWeek')?.touched">لطفا روز هفته را انتخاب کنید</span>
            </div>

            <div class="form-field">
              <label>شیفت</label>
              <select formControlName="shiftId" required>
                <option value="">انتخاب کنید</option>
                <option *ngFor="let shift of data?.shifts || []" [value]="shift.id">{{ shift.name }}</option>
              </select>
              <span class="error" *ngIf="scheduleForm.get('shiftId')?.hasError('required') && scheduleForm.get('shiftId')?.touched">لطفا شیفت را انتخاب کنید</span>
            </div>

            <div class="form-field">
              <label>تعداد</label>
              <input type="number" formControlName="count" min="1" required>
              <span class="error" *ngIf="scheduleForm.get('count')?.hasError('required') && scheduleForm.get('count')?.touched">لطفا تعداد را وارد نمایید</span>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="scheduleForm.invalid">
          {{ data?.schedule?.id != null ? 'ذخیره تغییرات' : 'افزودن' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-form {
      min-width: 550px;
      padding: 1rem 0;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      margin-bottom: 1.5rem;
    }

    .full-width {
      grid-column: 1 / -1;
    }

    .checkbox-container {
      display: flex;
      align-items: center;
      padding: 0.5rem 0;

      .checkbox-label {
        font-weight: 600;
        color: var(--text-secondary);
        margin-right: 0.5rem;
      }
    }

    .dialog-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .dialog-header {
      padding: 24px;
      border-bottom: 1px solid var(--border-light);
    }

    .dialog-header h2 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 800;
      background: var(--gradient-primary);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .dialog-content {
      padding: 24px;
      flex: 1;
      overflow-y: auto;
      max-height: 70vh;
    }

    .form-field {
      display: flex;
      flex-direction: column;
    }

    .form-field label {
      margin-bottom: 8px;
      font-weight: 600;
      color: var(--text-primary);
      font-size: 0.9rem;
    }

    .form-field input,
    .form-field select {
      padding: 12px 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: var(--bg-secondary);
      transition: all var(--transition-base);
    }

    .form-field input:focus,
    .form-field select:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .form-field .error {
      color: #ef4444;
      font-size: 0.85rem;
      margin-top: 4px;
    }

    .checkbox-container {
      display: flex;
      align-items: center;
      padding: 0.5rem 0;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      font-weight: 600;
    }

    .checkbox-label input[type="checkbox"] {
      width: 20px;
      height: 20px;
      cursor: pointer;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--border-light);
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      padding: 12px 24px;
      border: none;
      border-radius: var(--radius-md);
      font-weight: 700;
      font-size: 0.95rem;
      cursor: pointer;
      transition: all var(--transition-base);
      font-family: 'Vazirmatn', sans-serif;
    }

    .btn-primary {
      background: var(--gradient-primary);
      color: white;
      box-shadow: 0 4px 12px rgba(6, 182, 212, 0.3);
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(6, 182, 212, 0.4);
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
      border: 2px solid var(--border-color);
    }

    .btn-secondary:hover {
      background: var(--bg-secondary);
      border-color: var(--primary);
    }

    @media (max-width: 600px) {
      .dialog-form {
        min-width: auto;
      }

      .form-row {
        grid-template-columns: 1fr;
        gap: 1rem;
      }
    }
  `]
})
export class DoctorScheduleDialogComponent implements OnInit {
  scheduleForm: FormGroup;
  data: DoctorScheduleDialogData = { schedule: null, doctors: [], shifts: [] };
  dialogRef: any = null;
  persianDateService: PersianDateService;
  daysOfWeek = getAllDaysOfWeek();

  constructor(
    private fb: FormBuilder,
    persianDateService: PersianDateService
  ) {
    this.persianDateService = persianDateService;
    this.scheduleForm = this.fb.group({
      doctorId: ['', Validators.required],
      dayOfWeek: ['', Validators.required],
      shiftId: ['', Validators.required],
      count: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit() {
    if (this.data?.schedule) {
      this.scheduleForm.patchValue({
        doctorId: this.data.schedule.doctorId,
        dayOfWeek: this.data.schedule.dayOfWeek,
        shiftId: this.data.schedule.shiftId,
        count: this.data.schedule.count || 1
      });
    }
  }

  onSave() {
    if (this.scheduleForm.valid) {
      const formValue = this.scheduleForm.value;

      const doctor = (this.data?.doctors || []).find(d => d.id === formValue.doctorId);
      const shift = (this.data?.shifts || []).find(s => s.id === formValue.shiftId);

      const result: DoctorSchedule = {
        ...formValue,
        dayOfWeek: formValue.dayOfWeek,
        doctorName: doctor && doctor.user ? `${doctor.user.firstName} ${doctor.user.lastName}` : '',
        shiftName: shift?.name || '',
        startTime: shift?.startTime || '',
        endTime: shift?.endTime || '',
        count: formValue.count || 1
      };

      if (this.data?.schedule?.id != null) {
        result.id = this.data.schedule.id;
      }

      if (this.dialogRef) {
        this.dialogRef.close(result);
      }
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
