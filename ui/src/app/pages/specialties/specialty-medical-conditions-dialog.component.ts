import { Component, OnInit } from '@angular/core';
import { catchError, debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { Specialty, SpecialtyMedicalCondition } from '../../models/specialty.model';
import { MedicalCondition } from '../../models/medical-condition.model';
import { SpecialtyService } from '../../services/specialty.service';
import { MedicalConditionService } from '../../services/medical-condition.service';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-specialty-medical-conditions-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>مدیریت علائم پزشکی: {{ specialty?.name }}</h2>
      </div>
      <div class="dialog-content">
        <div class="add-condition-section">
          <h3>افزودن علائم پزشکی جدید</h3>
          <div class="add-condition-form">
            <div class="searchable-dropdown">
              <input
                type="text"
                [(ngModel)]="searchTerm"
                (input)="onSearchChange()"
                (focus)="showDropdown = true"
                (blur)="onBlur()"
                placeholder="جستجو و انتخاب علائم پزشکی..."
                class="condition-search-input"
                [disabled]="isLoadingConditions">
              <div class="dropdown-list" *ngIf="showDropdown && filteredConditions.length > 0">
                <div
                  class="dropdown-item"
                  *ngFor="let condition of filteredConditions"
                  (mousedown)="selectCondition(condition)"
                  [class.selected]="selectedConditionId === condition.id">
                  {{ condition.name }}
                  <span class="condition-description" *ngIf="condition.description"> - {{ condition.description }}</span>
                </div>
              </div>
              <div class="dropdown-empty" *ngIf="showDropdown && filteredConditions.length === 0 && !isLoadingConditions">
                علائمی یافت نشد
              </div>
              <div class="dropdown-loading" *ngIf="isLoadingConditions">
                در حال بارگذاری...
              </div>
            </div>
            <button class="btn btn-primary" (click)="addCondition()" [disabled]="!selectedConditionId || isSaving">
              افزودن
            </button>
          </div>
        </div>

        <div class="conditions-section">
          <h3>علائم پزشکی مرتبط</h3>
          <div class="conditions-list" *ngIf="specialtyMedicalConditions.length > 0; else noConditions">
            <div class="condition-item" *ngFor="let smc of specialtyMedicalConditions">
              <div class="condition-info">
                <span class="condition-name">{{ smc.medicalCondition?.name }}</span>
                <span class="condition-description" *ngIf="smc.medicalCondition?.description">{{ smc.medicalCondition?.description }}</span>
              </div>
              <button class="btn-icon btn-delete" (click)="removeCondition(smc)" title="حذف">
                🗑️
              </button>
            </div>
          </div>
          <ng-template #noConditions>
            <div class="empty-state">هیچ علائمی به این تخصص مرتبط نشده است</div>
          </ng-template>
        </div>
      </div>
      <div class="dialog-actions">
        <button type="button" class="btn btn-secondary" (click)="onClose()">بستن</button>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container {
      display: flex;
      flex-direction: column;
      height: 100%;
      min-height: 500px;
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
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .conditions-section, .add-condition-section {
      flex: 1;
    }

    .conditions-section h3, .add-condition-section h3 {
      margin: 0 0 1rem 0;
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .conditions-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 300px;
      overflow-y: auto;
    }

    .condition-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      background: var(--bg-secondary);
      border: 1px solid var(--border-light);
      border-radius: var(--radius-md);
      transition: all var(--transition-base);
    }

    .condition-item:hover {
      background: var(--bg-tertiary);
      border-color: var(--primary-light);
    }

    .condition-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex: 1;
    }

    .condition-name {
      font-weight: 600;
      color: var(--text-primary);
    }

    .condition-description {
      font-size: 0.85rem;
      color: var(--text-muted);
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--text-muted);
      font-size: 0.95rem;
    }

    .add-condition-form {
      display: flex;
      gap: 12px;
      align-items: flex-start;
    }

    .searchable-dropdown {
      flex: 1;
      position: relative;
    }

    .condition-search-input {
      width: 100%;
      padding: 12px 16px;
      border: 2px solid var(--border-color);
      border-radius: var(--radius-md);
      font-size: 1rem;
      font-family: 'Vazirmatn', sans-serif;
      background: var(--bg-secondary);
      transition: all var(--transition-base);
      direction: rtl;
      text-align: right;
    }

    .condition-search-input:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .condition-search-input:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .dropdown-list {
      position: absolute;
      top: calc(100% + 4px);
      right: 0;
      left: 0;
      max-height: 300px;
      overflow-y: auto;
      background: white;
      border: 2px solid var(--border-light);
      border-radius: var(--radius-md);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      z-index: 1000;
      margin-top: 4px;
    }

    .dropdown-item {
      padding: 12px 16px;
      cursor: pointer;
      transition: all var(--transition-base);
      border-bottom: 1px solid var(--border-light);
      direction: rtl;
      text-align: right;
    }

    .dropdown-item:last-child {
      border-bottom: none;
    }

    .dropdown-item:hover,
    .dropdown-item.selected {
      background: var(--bg-secondary);
      color: var(--primary);
    }

    .dropdown-empty,
    .dropdown-loading {
      position: absolute;
      top: calc(100% + 4px);
      right: 0;
      left: 0;
      padding: 12px 16px;
      background: white;
      border: 2px solid var(--border-light);
      border-radius: var(--radius-md);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      z-index: 1000;
      text-align: center;
      color: var(--text-muted);
      direction: rtl;
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

    .btn-icon {
      padding: 8px 12px;
      border: none;
      border-radius: var(--radius-md);
      background: var(--bg-tertiary);
      cursor: pointer;
      transition: all var(--transition-base);
      font-size: 1.1rem;
    }

    .btn-icon.btn-delete:hover {
      background: rgba(239, 68, 68, 0.1);
      color: #ef4444;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--border-light);
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }
  `]
})
export class SpecialtyMedicalConditionsDialogComponent implements OnInit {
  specialty: Specialty | null = null;
  specialtyMedicalConditions: SpecialtyMedicalCondition[] = [];
  filteredConditions: MedicalCondition[] = [];
  selectedConditionId: number | null = null;
  selectedConditionName: string = '';
  searchTerm: string = '';
  showDropdown: boolean = false;
  isLoadingConditions: boolean = false;
  isSaving = false;
  dialogRef: any = null;
  private searchSubject = new Subject<string>();

  constructor(
    private specialtyService: SpecialtyService,
    private medicalConditionService: MedicalConditionService,
    private snackbarService: SnackbarService
  ) { }

  ngOnInit() {
    setTimeout(() => {
      if (this.specialty?.id) {
        this.loadSpecialtyMedicalConditions();
        this.setupSearch();
      } else {
        console.warn('Specialty not set in SpecialtyMedicalConditionsDialogComponent', this.specialty);
      }
    }, 0);
  }

  setupSearch() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(searchTerm => {
        this.isLoadingConditions = true;
        const searchQuery = searchTerm || '';
        if (searchQuery) {
          return this.medicalConditionService.search(searchQuery).pipe(
            catchError(error => {
              this.isLoadingConditions = false;
              console.error('Error loading medical conditions:', error);
              const errorMessage = error.error?.message || error.message || 'خطا در بارگذاری لیست علائم پزشکی';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
              return [];
            })
          );
        } else {
          return this.medicalConditionService.getAll({ page: 1, pageSize: 100 }).pipe(
            catchError(error => {
              this.isLoadingConditions = false;
              console.error('Error loading medical conditions:', error);
              const errorMessage = error.error?.message || error.message || 'خطا در بارگذاری لیست علائم پزشکی';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
              return [];
            }),
            switchMap(result => {
              return [result.items || []];
            })
          );
        }
      })
    ).subscribe({
      next: (conditions) => {
        // Filter out conditions that are already associated
        const associatedConditionIds = this.specialtyMedicalConditions
          .map(smc => smc.medicalConditionId)
          .filter(id => id != null);
        this.filteredConditions = conditions.filter(c =>
          c.id != null && !associatedConditionIds.includes(c.id)
        );
        this.isLoadingConditions = false;
      }
    });

    setTimeout(() => {
      this.searchSubject.next('');
    }, 200);
  }

  onSearchChange() {
    this.searchSubject.next(this.searchTerm);
  }

  selectCondition(condition: MedicalCondition) {
    this.selectedConditionId = condition.id || null;
    this.selectedConditionName = condition.name;
    this.searchTerm = condition.name;
    this.showDropdown = false;
  }

  onBlur() {
    setTimeout(() => {
      this.showDropdown = false;
    }, 200);
  }

  loadSpecialtyMedicalConditions() {
    if (!this.specialty?.id) return;

    this.specialtyService.getMedicalConditions(this.specialty.id).subscribe({
      next: (conditions) => {
        this.specialtyMedicalConditions = conditions;
        // Refresh search results to filter out assigned conditions
        this.searchSubject.next(this.searchTerm);
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری علائم پزشکی تخصص';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
      }
    });
  }

  addCondition() {
    if (!this.selectedConditionId || !this.specialty?.id || this.isSaving) return;

    this.isSaving = true;
    this.specialtyService.addMedicalCondition(this.specialty.id, this.selectedConditionId).subscribe({
      next: () => {
        this.snackbarService.success('علائم پزشکی با موفقیت به تخصص اضافه شد', 'بستن', 3000);
        this.selectedConditionId = null;
        this.selectedConditionName = '';
        this.searchTerm = '';
        this.loadSpecialtyMedicalConditions();
        this.searchSubject.next(this.searchTerm);
        this.isSaving = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در افزودن علائم پزشکی به تخصص';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isSaving = false;
      }
    });
  }

  removeCondition(smc: SpecialtyMedicalCondition) {
    if (!smc.medicalConditionId || !this.specialty?.id) return;

    this.specialtyService.removeMedicalCondition(this.specialty.id, smc.medicalConditionId).subscribe({
      next: () => {
        this.snackbarService.success('علائم پزشکی با موفقیت از تخصص حذف شد', 'بستن', 3000);
        this.loadSpecialtyMedicalConditions();
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در حذف علائم پزشکی از تخصص';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
      }
    });
  }

  onClose() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
