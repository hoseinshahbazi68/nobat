import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DoctorVisitInfo } from '../../models/doctor-visit-info.model';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-doctor-visit-info-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.visitInfo?.id ? 'ویرایش اطلاعات ویزیت' : 'افزودن اطلاعات ویزیت جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="visitInfoForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field full-width">
              <label>پزشک <span style="color: red;">*</span></label>
              <select formControlName="doctorId" [disabled]="!!data?.visitInfo?.id" required>
                <option [value]="null">انتخاب کنید</option>
                <option *ngFor="let doctor of doctors" [value]="doctor.id">
                  {{ getDoctorDisplayName(doctor) }}
                </option>
              </select>
              <span class="error" *ngIf="visitInfoForm.get('doctorId')?.hasError('required') && visitInfoForm.get('doctorId')?.touched">لطفا پزشک را انتخاب کنید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>درباره پزشک</label>
              <textarea formControlName="about" rows="4" placeholder="درباره پزشک را وارد کنید"></textarea>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>آدرس مطب</label>
              <input type="text" formControlName="clinicAddress" placeholder="آدرس مطب را وارد کنید">
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>تلفن مطب</label>
              <input type="text" formControlName="clinicPhone" placeholder="تلفن مطب را وارد کنید">
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>ساعت حضور در مطب</label>
              <input type="text" formControlName="officeHours" placeholder="ساعت حضور در مطب را وارد کنید">
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="visitInfoForm.invalid">
          {{ data?.visitInfo?.id ? 'ذخیره تغییرات' : 'افزودن' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-form {
      min-width: 600px;
      padding: 1rem 0;
    }

    .form-row {
      margin-bottom: 1rem;
    }

    .full-width {
      width: 100%;
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
    }

    .form-row {
      margin-bottom: 1.5rem;
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
    .form-field textarea,
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
    .form-field textarea:focus,
    .form-field select:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .form-field textarea {
      resize: vertical;
      min-height: 100px;
    }

    .form-field select[disabled] {
      background: var(--bg-tertiary);
      cursor: not-allowed;
    }

    .form-field .error {
      color: #ef4444;
      font-size: 0.85rem;
      margin-top: 4px;
    }

    .full-width {
      width: 100%;
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
    }
  `]
})
export class DoctorVisitInfoDialogComponent implements OnInit {
  visitInfoForm: FormGroup;
  data: { visitInfo: DoctorVisitInfo | null; doctors: Doctor[] } | null = null;
  doctors: Doctor[] = [];
  dialogRef: any = null;

  constructor(
    private fb: FormBuilder
  ) {
    this.visitInfoForm = this.fb.group({
      doctorId: [null, Validators.required],
      about: [''],
      clinicAddress: [''],
      clinicPhone: [''],
      officeHours: ['']
    });
  }

  ngOnInit() {
    if (this.data) {
      this.doctors = this.data.doctors || [];
      if (this.data.visitInfo) {
        this.visitInfoForm.patchValue({
          doctorId: this.data.visitInfo.doctorId,
          about: this.data.visitInfo.about || '',
          clinicAddress: this.data.visitInfo.clinicAddress || '',
          clinicPhone: this.data.visitInfo.clinicPhone || '',
          officeHours: this.data.visitInfo.officeHours || ''
        });
      }
    }
  }

  getDoctorDisplayName(doctor: Doctor): string {
    const name = `${doctor.firstName || ''} ${doctor.lastName || ''}`.trim();
    return name ? `${name} (${doctor.medicalCode})` : doctor.medicalCode;
  }

  onSave() {
    this.visitInfoForm.markAllAsTouched();

    if (this.visitInfoForm.valid) {
      const formValue = this.visitInfoForm.value;
      // Convert empty strings to undefined
      Object.keys(formValue).forEach(key => {
        if (formValue[key] === '') {
          formValue[key] = undefined;
        }
      });
      if (this.dialogRef) {
        this.dialogRef.close(formValue);
      }
    }
  }

  onCancel() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
