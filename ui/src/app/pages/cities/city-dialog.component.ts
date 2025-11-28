import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { City } from '../../models/city.model';
import { Province } from '../../models/province.model';

export interface CityDialogData {
  city: City | null;
  provinces: Province[];
}

@Component({
  selector: 'app-city-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.city?.id ? 'ویرایش شهر' : 'افزودن شهر جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="cityForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field full-width">
              <label>نام شهر</label>
              <input type="text" formControlName="name" required>
              <span class="error" *ngIf="cityForm.get('name')?.hasError('required') && cityForm.get('name')?.touched">لطفا نام شهر را وارد نمایید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>استان</label>
              <select formControlName="provinceId" required>
                <option value="">انتخاب کنید</option>
                <option *ngFor="let province of data?.provinces || []" [value]="province.id">{{ province.name }}</option>
              </select>
              <span class="error" *ngIf="cityForm.get('provinceId')?.hasError('required') && cityForm.get('provinceId')?.touched">لطفا استان را انتخاب کنید</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>کد شهر</label>
              <input type="text" formControlName="code">
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="cityForm.invalid">
          {{ data?.city?.id ? 'ذخیره تغییرات' : 'افزودن' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container { display: flex; flex-direction: column; height: 100%; }
    .dialog-header { padding: 24px; border-bottom: 1px solid var(--border-light); }
    .dialog-header h2 { margin: 0; font-size: 1.5rem; font-weight: 800; background: var(--gradient-primary); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; }
    .dialog-content { padding: 24px; flex: 1; overflow-y: auto; }
    .dialog-form { min-width: 500px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 1.5rem; }
    .full-width { grid-column: 1 / -1; }
    .form-field { display: flex; flex-direction: column; }
    .form-field label { margin-bottom: 8px; font-weight: 600; color: var(--text-primary); font-size: 0.9rem; }
    .form-field input, .form-field select { padding: 12px 16px; border: 2px solid var(--border-color); border-radius: var(--radius-md); font-size: 1rem; font-family: 'Vazirmatn', sans-serif; background: var(--bg-secondary); transition: all var(--transition-base); }
    .form-field input:focus, .form-field select:focus { outline: none; border-color: var(--primary); box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1); }
    .form-field .error { color: #ef4444; font-size: 0.85rem; margin-top: 4px; }
    .dialog-actions { padding: 16px 24px; border-top: 1px solid var(--border-light); display: flex; justify-content: flex-end; gap: 12px; }
    .btn { padding: 12px 24px; border: none; border-radius: var(--radius-md); font-weight: 700; font-size: 0.95rem; cursor: pointer; transition: all var(--transition-base); font-family: 'Vazirmatn', sans-serif; }
    .btn-primary { background: var(--gradient-primary); color: white; box-shadow: 0 4px 12px rgba(6, 182, 212, 0.3); }
    .btn-primary:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(6, 182, 212, 0.4); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-secondary { background: var(--bg-tertiary); color: var(--text-primary); border: 2px solid var(--border-color); }
    .btn-secondary:hover { background: var(--bg-secondary); border-color: var(--primary); }
    @media (max-width: 600px) { .dialog-form { min-width: auto; } .form-row { grid-template-columns: 1fr; gap: 1rem; } }
  `]
})
export class CityDialogComponent implements OnInit {
  cityForm: FormGroup;
  data: CityDialogData = { city: null, provinces: [] };
  dialogRef: any = null;

  constructor(private fb: FormBuilder) {
    this.cityForm = this.fb.group({
      name: ['', Validators.required],
      provinceId: ['', Validators.required],
      code: ['']
    });
  }

  ngOnInit() {
    if (this.data?.city) {
      this.cityForm.patchValue(this.data.city);
    }
  }

  onSave() {
    if (this.cityForm.valid) {
      const formValue = this.cityForm.value;
      if (this.data?.city?.id) {
        formValue.id = this.data.city.id;
      }
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
