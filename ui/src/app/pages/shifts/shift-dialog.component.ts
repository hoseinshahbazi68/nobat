import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Shift } from '../../models/shift.model';

@Component({
  selector: 'app-shift-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>{{ data?.id ? 'ویرایش شیفت' : 'افزودن شیفت جدید' }}</h2>
      </div>
      <div class="dialog-content">
        <form [formGroup]="shiftForm" class="dialog-form">
          <div class="form-row">
            <div class="form-field full-width">
              <label>نام شیفت</label>
              <input type="text" formControlName="name" required>
              <span class="error" *ngIf="shiftForm.get('name')?.hasError('required') && shiftForm.get('name')?.touched">
                لطفا نام شیفت را وارد نمایید
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label>ساعت شروع</label>
              <input type="text" formControlName="startTime" appTimeMask placeholder="00:00" maxlength="5" required>
              <span class="error" *ngIf="shiftForm.get('startTime')?.hasError('required') && shiftForm.get('startTime')?.touched">
                لطفا ساعت شروع را وارد نمایید
              </span>
            </div>

            <div class="form-field">
              <label>ساعت پایان</label>
              <input type="text" formControlName="endTime" appTimeMask placeholder="00:00" maxlength="5" required>
              <span class="error" *ngIf="shiftForm.get('endTime')?.hasError('required') && shiftForm.get('endTime')?.touched">
                لطفا ساعت پایان را وارد نمایید
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-field full-width">
              <label>توضیحات</label>
              <textarea formControlName="description" rows="3"></textarea>
            </div>
          </div>
        </form>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">انصراف</button>
        <button type="button" class="btn btn-primary" (click)="onSave()" [disabled]="shiftForm.invalid">
          {{ data?.id ? 'ذخیره تغییرات' : 'افزودن' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
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

    .dialog-form {
      min-width: 550px;
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
    .form-field textarea {
      padding: 12px 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: var(--bg-secondary);
      transition: all var(--transition-base);
    }

    .form-field input[type="text"][apptimemask] {
      direction: ltr;
      text-align: center;
      letter-spacing: 2px;
    }

    .form-field input:focus,
    .form-field textarea:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .form-field textarea {
      resize: vertical;
      min-height: 80px;
    }

    .form-field .error {
      color: #ef4444;
      font-size: 0.85rem;
      margin-top: 4px;
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
export class ShiftDialogComponent implements OnInit {
  shiftForm: FormGroup;
  data: Shift | null = null;
  dialogRef: any = null;

  constructor(
    private fb: FormBuilder
  ) {
    this.shiftForm = this.fb.group({
      name: ['', Validators.required],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit() {
    if (this.data) {
      this.shiftForm.patchValue(this.data);
    }
  }

  onSave() {
    if (this.shiftForm.valid) {
      const formValue = this.shiftForm.value;
      if (this.data?.id) {
        formValue.id = this.data.id;
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
