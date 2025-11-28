import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogService } from '../../services/dialog.service';
import { Shift } from '../../models/shift.model';
import { Service } from '../../models/service.model';

export interface CreateAppointmentDialogData {
  doctorId: number;
  selectedDate: Date;
  shifts: Shift[];
  services: Service[];
}

@Component({
  selector: 'app-create-appointment-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <div class="header-icon">📅</div>
        <h2>ایجاد نوبت جدید</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="appointmentForm" class="dialog-form">
          <div class="form-section">
            <div class="form-row">
              <div class="form-field">
                <label>
                  <span class="label-icon">⚙️</span>
                  شیفت
                </label>
                <div class="select-wrapper">
                  <select formControlName="shiftId" class="custom-select" required>
                    <option value="">انتخاب شیفت</option>
                    <option *ngFor="let shift of data?.shifts || []" [value]="shift.id">
                      {{ shift.name }} ({{ shift.startTime }} - {{ shift.endTime }})
                    </option>
                  </select>
                  <span class="select-arrow">▼</span>
                </div>
                <span class="error" *ngIf="appointmentForm.get('shiftId')?.hasError('required') && appointmentForm.get('shiftId')?.touched">
                  لطفا شیفت را انتخاب کنید
                </span>
              </div>

              <div class="form-field">
                <label>
                  <span class="label-icon">🏥</span>
                  خدمت
                </label>
                <div class="select-wrapper">
                  <select formControlName="serviceId" class="custom-select" required>
                    <option value="">انتخاب خدمت</option>
                    <option *ngFor="let service of data?.services || []" [value]="service.id">
                      {{ service.name }}
                    </option>
                  </select>
                  <span class="select-arrow">▼</span>
                </div>
                <span class="error" *ngIf="appointmentForm.get('serviceId')?.hasError('required') && appointmentForm.get('serviceId')?.touched">
                  لطفا خدمت را انتخاب کنید
                </span>
              </div>
            </div>
          </div>

          <div class="form-section">
            <div class="section-title">
              <span class="section-icon">⏰</span>
              <span>زمان‌بندی</span>
            </div>
            <div class="form-row">
              <div class="form-field">
                <label>
                  <span class="label-icon">🟢</span>
                  ساعت شروع
                </label>
                <div class="time-input-wrapper">
                  <input type="time" formControlName="startTime" class="time-input" required>
                </div>
                <span class="error" *ngIf="appointmentForm.get('startTime')?.hasError('required') && appointmentForm.get('startTime')?.touched">
                  لطفا ساعت شروع را وارد نمایید
                </span>
              </div>

              <div class="form-field">
                <label>
                  <span class="label-icon">🔴</span>
                  ساعت پایان
                </label>
                <div class="time-input-wrapper">
                  <input type="time" formControlName="endTime" class="time-input" required>
                </div>
                <span class="error" *ngIf="appointmentForm.get('endTime')?.hasError('required') && appointmentForm.get('endTime')?.touched">
                  لطفا ساعت پایان را وارد نمایید
                </span>
              </div>
            </div>
          </div>

          <div class="form-section">
            <div class="form-row">
              <div class="form-field">
                <label>
                  <span class="label-icon">📊</span>
                  تعداد
                </label>
                <div class="number-input-wrapper">
                  <button type="button" class="number-btn minus" (click)="decreaseCount()">−</button>
                  <input type="number" formControlName="count" min="1" class="number-input" readonly>
                  <button type="button" class="number-btn plus" (click)="increaseCount()">+</button>
                </div>
                <p class="field-hint">تعداد نوبت‌های قابل رزرو</p>
              </div>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <div class="action-buttons">
          <button type="button" class="btn btn-secondary" (click)="onCancel()">
            <span class="btn-icon">✖️</span>
            انصراف
          </button>
          <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="appointmentForm.invalid">
            <span class="btn-icon">✓</span>
            ایجاد نوبت
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container {
      display: flex;
      flex-direction: column;
      height: auto;
      max-height: 90vh;
      background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
      border-radius: 16px;
      overflow: hidden;
    }

    .dialog-header {
      padding: 20px 28px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      text-align: center;
      position: relative;
      overflow: hidden;
    }

    .dialog-header::before {
      content: '';
      position: absolute;
      top: -50%;
      right: -50%;
      width: 200%;
      height: 200%;
      background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
      animation: pulse 3s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { transform: scale(1); opacity: 0.5; }
      50% { transform: scale(1.1); opacity: 0.8; }
    }

    .header-icon {
      font-size: 2.5rem;
      margin-bottom: 8px;
      display: block;
      filter: drop-shadow(0 4px 8px rgba(0,0,0,0.2));
    }

    .dialog-header h2 {
      margin: 0 0 6px 0;
      font-size: 1.6rem;
      font-weight: 800;
      text-shadow: 0 2px 4px rgba(0,0,0,0.2);
      position: relative;
      z-index: 1;
    }

    .dialog-content {
      padding: 24px 28px;
      flex: 1;
      overflow-y: auto;
      min-height: 0;
      background: white;
    }

    .dialog-form {
      min-width: 650px;
    }

    .form-section {
      margin-bottom: 20px;
      padding: 18px 20px;
      background: #f8f9fa;
      border-radius: 10px;
      border: 1px solid #e9ecef;
      transition: all 0.3s ease;
    }

    .form-section:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
      border-color: #667eea;
    }

    .section-title {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 14px;
      font-size: 1rem;
      font-weight: 700;
      color: #495057;
    }

    .section-icon {
      font-size: 1.2rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.2rem;
    }

    .form-field {
      display: flex;
      flex-direction: column;
    }

    .form-field label {
      display: flex;
      align-items: center;
      gap: 6px;
      margin-bottom: 8px;
      font-weight: 600;
      color: #495057;
      font-size: 0.9rem;
    }

    .label-icon {
      font-size: 1rem;
    }

    .select-wrapper {
      position: relative;
    }

    .custom-select {
      width: 100%;
      padding: 12px 40px 12px 14px;
      border: 2px solid #dee2e6;
      border-radius: 8px;
      font-size: 0.95rem;
      font-family: 'Vazirmatn', sans-serif;
      background: white;
      transition: all 0.3s ease;
      appearance: none;
      cursor: pointer;
    }

    .custom-select:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .select-arrow {
      position: absolute;
      left: 16px;
      top: 50%;
      transform: translateY(-50%);
      pointer-events: none;
      color: #6c757d;
      font-size: 0.8rem;
    }

    .time-input-wrapper {
      position: relative;
    }

    .time-input {
      width: 100%;
      padding: 12px 40px 12px 14px;
      border: 2px solid #dee2e6;
      border-radius: 8px;
      font-size: 0.95rem;
      font-family: 'Vazirmatn', sans-serif;
      background: white;
      transition: all 0.3s ease;
    }

    .time-input:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .number-input-wrapper {
      display: flex;
      align-items: center;
      gap: 6px;
      background: white;
      border: 2px solid #dee2e6;
      border-radius: 8px;
      padding: 3px;
      transition: all 0.3s ease;
    }

    .number-input-wrapper:focus-within {
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .number-btn {
      width: 36px;
      height: 36px;
      border: none;
      border-radius: 6px;
      font-size: 1.3rem;
      font-weight: 700;
      cursor: pointer;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .number-btn.plus {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .number-btn.plus:hover {
      transform: scale(1.1);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
    }

    .number-btn.minus {
      background: #f8f9fa;
      color: #495057;
      border: 2px solid #dee2e6;
    }

    .number-btn.minus:hover {
      background: #e9ecef;
      transform: scale(1.1);
    }

    .number-input {
      flex: 1;
      border: none;
      text-align: center;
      font-size: 1.1rem;
      font-weight: 700;
      font-family: 'Vazirmatn', sans-serif;
      background: transparent;
      padding: 6px;
    }

    .number-input:focus {
      outline: none;
    }

    .field-hint {
      margin-top: 6px;
      font-size: 0.8rem;
      color: #6c757d;
      font-style: italic;
    }

    .error {
      margin-top: 4px;
      font-size: 0.8rem;
      color: #f5576c;
      display: block;
    }

    .dialog-actions {
      padding: 16px 28px;
      border-top: 1px solid #e9ecef;
      display: flex !important;
      justify-content: flex-end;
      align-items: center;
      background: #f8f9fa;
      flex-shrink: 0;
      position: relative;
      z-index: 10;
      visibility: visible !important;
      opacity: 1 !important;
    }

    .action-buttons {
      display: flex;
      gap: 12px;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 8px;
      font-weight: 700;
      font-size: 0.9rem;
      cursor: pointer;
      transition: all 0.3s ease;
      font-family: 'Vazirmatn', sans-serif;
      display: flex !important;
      align-items: center;
      gap: 6px;
      box-shadow: 0 2px 6px rgba(0,0,0,0.1);
      visibility: visible !important;
      opacity: 1 !important;
    }

    .btn-icon {
      font-size: 1.1rem;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(102, 126, 234, 0.4);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .btn-secondary {
      background: white;
      color: #495057;
      border: 2px solid #dee2e6;
    }

    .btn-secondary:hover {
      background: #f8f9fa;
      border-color: #667eea;
      color: #667eea;
    }

    @media (max-width: 600px) {
      .dialog-form {
        min-width: auto;
      }

      .form-row {
        grid-template-columns: 1fr;
        gap: 1rem;
      }

      .dialog-header {
        padding: 20px;
      }

      .dialog-content {
        padding: 20px;
      }

      .form-section {
        padding: 16px;
      }
    }
  `]
})
export class CreateAppointmentDialogComponent implements OnInit {
  appointmentForm: FormGroup;
  data: CreateAppointmentDialogData = {
    doctorId: 0,
    selectedDate: new Date(),
    shifts: [],
    services: []
  };
  dialogRef: any = null;

  constructor(
    private fb: FormBuilder,
    private dialogService: DialogService
  ) {
    this.appointmentForm = this.fb.group({
      shiftId: ['', Validators.required],
      serviceId: ['', Validators.required],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      count: [1, [Validators.required, Validators.min(1)]]
    });

    // وقتی شیفت انتخاب می‌شود، زمان‌های پیش‌فرض شیفت را تنظیم کن
    this.appointmentForm.get('shiftId')?.valueChanges.subscribe(shiftId => {
      if (shiftId) {
        const shift = this.data.shifts.find(s => s.id === shiftId);
        if (shift && shift.startTime && shift.endTime) {
          this.appointmentForm.patchValue({
            startTime: shift.startTime,
            endTime: shift.endTime
          }, { emitEvent: false });
        }
      }
    });
  }

  ngOnInit() {
    // تنظیمات اولیه
  }

  increaseCount() {
    const currentValue = this.appointmentForm.get('count')?.value || 1;
    this.appointmentForm.patchValue({ count: currentValue + 1 });
  }

  decreaseCount() {
    const currentValue = this.appointmentForm.get('count')?.value || 1;
    if (currentValue > 1) {
      this.appointmentForm.patchValue({ count: currentValue - 1 });
    }
  }

  onSave() {
    if (this.appointmentForm.valid) {
      const formValue = this.appointmentForm.value;
      const result = {
        shiftId: formValue.shiftId,
        serviceId: formValue.serviceId,
        startTime: formValue.startTime,
        endTime: formValue.endTime,
        count: formValue.count || 1
      };
      this.dialogRef.close(result);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
