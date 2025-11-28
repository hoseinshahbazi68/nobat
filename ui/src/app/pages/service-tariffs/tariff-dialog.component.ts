import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ServiceTariff } from '../../models/service-tariff.model';

export interface TariffDialogData {
  tariff: ServiceTariff | null;
  services: any[];
  insurances: any[];
  doctors: any[];
  clinics?: any[];
  selectedClinicId?: number;
  selectedDoctorId?: number;
}

@Component({
  selector: 'app-tariff-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.tariff?.id != null ? 'ویرایش تعرفه' : 'افزودن تعرفه جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="tariffForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field">
              <label>خدمت</label>
              <select formControlName="serviceId" required>
                <option value="">انتخاب کنید</option>
                <option *ngFor="let service of data?.services || []" [value]="service.id">{{ service.name }}</option>
              </select>
              <span class="error" *ngIf="tariffForm.get('serviceId')?.hasError('required') && tariffForm.get('serviceId')?.touched">لطفا خدمت را انتخاب کنید</span>
            </div>

            <div class="form-field">
              <label>بیمه</label>
              <select formControlName="insuranceId" required>
                <option value="">انتخاب کنید</option>
                <option *ngFor="let insurance of data?.insurances || []" [value]="insurance.id">{{ insurance.name }}</option>
              </select>
              <span class="error" *ngIf="tariffForm.get('insuranceId')?.hasError('required') && tariffForm.get('insuranceId')?.touched">لطفا بیمه را انتخاب کنید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>قیمت (تومان)</label>
              <input type="number" formControlName="price" required>
              <span class="error" *ngIf="tariffForm.get('price')?.hasError('required') && tariffForm.get('price')?.touched">لطفا قیمت را وارد نمایید</span>
              <span class="error" *ngIf="tariffForm.get('price')?.hasError('min') && tariffForm.get('price')?.touched">قیمت باید بیشتر از صفر باشد</span>
            </div>

            <div class="form-field">
              <label>مدت زمان ویزیت (دقیقه)</label>
              <input type="number" formControlName="visitDuration" min="1">
              <span class="error" *ngIf="tariffForm.get('visitDuration')?.hasError('min') && tariffForm.get('visitDuration')?.touched">مدت زمان باید بیشتر از صفر باشد</span>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="tariffForm.invalid">
          {{ data?.tariff?.id != null ? 'ذخیره تغییرات' : 'افزودن' }}
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

    .form-field .suffix {
      position: absolute;
      left: 16px;
      top: 50%;
      transform: translateY(-50%);
      color: var(--text-muted);
      font-size: 0.9rem;
    }

    .form-field input[readonly] {
      background: var(--bg-tertiary);
      cursor: not-allowed;
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
export class TariffDialogComponent implements OnInit {
  tariffForm: FormGroup;
  data: TariffDialogData = { tariff: null, services: [], insurances: [], doctors: [], clinics: [] };
  dialogRef: any = null;

  constructor(
    private fb: FormBuilder
  ) {
    this.tariffForm = this.fb.group({
      serviceId: ['', Validators.required],
      insuranceId: ['', Validators.required],
      doctorId: [null],
      clinicId: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0)]],
      visitDuration: [null, [Validators.min(1)]]
    });
  }

  ngOnInit() {
    if (this.data?.tariff) {
      this.tariffForm.patchValue({
        serviceId: this.data.tariff.serviceId,
        insuranceId: this.data.tariff.insuranceId,
        doctorId: this.data.tariff.doctorId || null,
        clinicId: this.data.tariff.clinicId,
        price: this.data.tariff.price,
        visitDuration: this.data.tariff.visitDuration || null
      });
    } else {
      // تنظیم خودکار clinicId و doctorId از data
      if (this.data?.selectedClinicId) {
        this.tariffForm.patchValue({
          clinicId: this.data.selectedClinicId
        });
      }
      if (this.data?.selectedDoctorId !== undefined) {
        this.tariffForm.patchValue({
          doctorId: this.data.selectedDoctorId
        });
      }
    }
  }

  onSave() {
    if (this.tariffForm.valid) {
      const formValue = this.tariffForm.value;

      const service = (this.data?.services || []).find(s => s.id === formValue.serviceId);
      const insurance = (this.data?.insurances || []).find(i => i.id === formValue.insuranceId);
      const doctor = formValue.doctorId ? (this.data?.doctors || []).find(d => d.id === formValue.doctorId) : null;
      const clinic = (this.data?.clinics || []).find(c => c.id === formValue.clinicId);

      const result: ServiceTariff = {
        ...formValue,
        doctorId: formValue.doctorId || undefined,
        finalPrice: formValue.price,
        serviceName: service?.name || '',
        insuranceName: insurance?.name || '',
        doctorName: doctor && doctor.user ? `${doctor.user.firstName} ${doctor.user.lastName}` : undefined,
        clinicName: clinic?.name || ''
      };

      if (this.data?.tariff?.id != null) {
        result.id = this.data.tariff.id;
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
