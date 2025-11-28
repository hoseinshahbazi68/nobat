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
              <div class="searchable-dropdown-container">
                <input type="text" [(ngModel)]="serviceSearchText" [ngModelOptions]="{standalone: true}"
                  (input)="onServiceSearch()" (focus)="onServiceInputFocus()" (blur)="onServiceInputBlur()"
                  placeholder="جستجو و انتخاب خدمت..." class="searchable-dropdown-input">
                <span class="search-icon">🔍</span>
                <div class="searchable-dropdown" *ngIf="showServiceDropdown && filteredServices.length > 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="dropdown-item" *ngFor="let service of filteredServices" (click)="selectService(service)">
                    {{ service.name }}
                  </div>
                </div>
                <div class="searchable-dropdown empty"
                  *ngIf="showServiceDropdown && filteredServices.length === 0 && (data?.services || []).length > 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="dropdown-item">خدمتی یافت نشد</div>
                </div>
              </div>
              <span class="error" *ngIf="tariffForm.get('serviceId')?.hasError('required') && tariffForm.get('serviceId')?.touched">لطفا خدمت را انتخاب کنید</span>
            </div>

            <div class="form-field">
              <label>بیمه</label>
              <div class="searchable-dropdown-container">
                <input type="text" [(ngModel)]="insuranceSearchText" [ngModelOptions]="{standalone: true}"
                  (input)="onInsuranceSearch()" (focus)="onInsuranceInputFocus()" (blur)="onInsuranceInputBlur()"
                  placeholder="جستجو و انتخاب بیمه..." class="searchable-dropdown-input">
                <span class="search-icon">🔍</span>
                <div class="searchable-dropdown" *ngIf="showInsuranceDropdown && filteredInsurances.length > 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="dropdown-item" *ngFor="let insurance of filteredInsurances" (click)="selectInsurance(insurance)">
                    {{ insurance.name }}
                  </div>
                </div>
                <div class="searchable-dropdown empty"
                  *ngIf="showInsuranceDropdown && filteredInsurances.length === 0 && (data?.insurances || []).length > 0"
                  (mousedown)="$event.preventDefault()">
                  <div class="dropdown-item">بیمه‌ای یافت نشد</div>
                </div>
              </div>
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

    .searchable-dropdown-container {
      position: relative;
      width: 100%;
    }

    .searchable-dropdown-input {
      width: 100%;
      padding: 12px 40px 12px 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: var(--bg-secondary);
      transition: all var(--transition-base);
    }

    .searchable-dropdown-input:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .search-icon {
      position: absolute;
      left: 16px;
      top: 50%;
      transform: translateY(-50%);
      pointer-events: none;
      font-size: 1rem;
      color: var(--text-muted);
    }

    .searchable-dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: var(--bg-secondary);
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      margin-top: 4px;
      max-height: 200px;
      overflow-y: auto;
      z-index: 1000;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .searchable-dropdown.empty {
      border-color: var(--border-light);
    }

    .dropdown-item {
      padding: 12px 16px;
      cursor: pointer;
      transition: background-color var(--transition-base);
      font-family: 'Vazirmatn', sans-serif;
      font-size: 0.95rem;
      color: var(--text-primary);
    }

    .dropdown-item:hover {
      background-color: var(--bg-tertiary);
    }

    .dropdown-item:first-child {
      border-top-left-radius: var(--radius-md);
      border-top-right-radius: var(--radius-md);
    }

    .dropdown-item:last-child {
      border-bottom-left-radius: var(--radius-md);
      border-bottom-right-radius: var(--radius-md);
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

  // Service search properties
  serviceSearchText: string = '';
  filteredServices: any[] = [];
  showServiceDropdown: boolean = false;

  // Insurance search properties
  insuranceSearchText: string = '';
  filteredInsurances: any[] = [];
  showInsuranceDropdown: boolean = false;

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
    // Initialize filtered lists
    this.filteredServices = this.data?.services || [];
    this.filteredInsurances = this.data?.insurances || [];

    if (this.data?.tariff) {
      this.tariffForm.patchValue({
        serviceId: this.data.tariff.serviceId,
        insuranceId: this.data.tariff.insuranceId,
        doctorId: this.data.tariff.doctorId || null,
        clinicId: this.data.tariff.clinicId,
        price: this.data.tariff.price,
        visitDuration: this.data.tariff.visitDuration || null
      });

      // Set search text for selected items
      const selectedService = (this.data?.services || []).find(s => s.id === this.data.tariff?.serviceId);
      if (selectedService) {
        this.serviceSearchText = selectedService.name;
      }

      const selectedInsurance = (this.data?.insurances || []).find(i => i.id === this.data.tariff?.insuranceId);
      if (selectedInsurance) {
        this.insuranceSearchText = selectedInsurance.name;
      }
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

  // Service search methods
  onServiceSearch() {
    const searchText = this.serviceSearchText?.toLowerCase().trim() || '';
    if (searchText) {
      this.filteredServices = (this.data?.services || []).filter(service =>
        service.name?.toLowerCase().includes(searchText)
      );
    } else {
      this.filteredServices = this.data?.services || [];
    }
    this.showServiceDropdown = true;
  }

  selectService(service: any) {
    this.tariffForm.patchValue({ serviceId: service.id });
    this.serviceSearchText = service.name;
    this.showServiceDropdown = false;
  }

  onServiceInputFocus() {
    this.showServiceDropdown = true;
    if (!this.serviceSearchText || !this.serviceSearchText.trim()) {
      this.filteredServices = this.data?.services || [];
    }
  }

  onServiceInputBlur() {
    setTimeout(() => {
      this.showServiceDropdown = false;
      // Restore selected service name if exists
      const selectedServiceId = this.tariffForm.get('serviceId')?.value;
      if (selectedServiceId) {
        const selectedService = (this.data?.services || []).find(s => s.id === selectedServiceId);
        if (selectedService) {
          this.serviceSearchText = selectedService.name;
        }
      }
    }, 200);
  }

  // Insurance search methods
  onInsuranceSearch() {
    const searchText = this.insuranceSearchText?.toLowerCase().trim() || '';
    if (searchText) {
      this.filteredInsurances = (this.data?.insurances || []).filter(insurance =>
        insurance.name?.toLowerCase().includes(searchText)
      );
    } else {
      this.filteredInsurances = this.data?.insurances || [];
    }
    this.showInsuranceDropdown = true;
  }

  selectInsurance(insurance: any) {
    this.tariffForm.patchValue({ insuranceId: insurance.id });
    this.insuranceSearchText = insurance.name;
    this.showInsuranceDropdown = false;
  }

  onInsuranceInputFocus() {
    this.showInsuranceDropdown = true;
    if (!this.insuranceSearchText || !this.insuranceSearchText.trim()) {
      this.filteredInsurances = this.data?.insurances || [];
    }
  }

  onInsuranceInputBlur() {
    setTimeout(() => {
      this.showInsuranceDropdown = false;
      // Restore selected insurance name if exists
      const selectedInsuranceId = this.tariffForm.get('insuranceId')?.value;
      if (selectedInsuranceId) {
        const selectedInsurance = (this.data?.insurances || []).find(i => i.id === selectedInsuranceId);
        if (selectedInsurance) {
          this.insuranceSearchText = selectedInsurance.name;
        }
      }
    }, 200);
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
