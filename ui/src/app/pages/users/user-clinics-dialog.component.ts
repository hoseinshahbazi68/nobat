import { Component, OnInit } from '@angular/core';
import { catchError, debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { Clinic, ClinicSimple } from '../../models/clinic.model';
import { User } from '../../models/user.model';
import { ClinicService } from '../../services/clinic.service';
import { SnackbarService } from '../../services/snackbar.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-clinics-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>مدیریت مراکز کاربر: {{ user?.firstName }} {{ user?.lastName }}</h2>
      </div>
      <div class="dialog-content">
        <div class="add-clinic-section">
          <h3>افزودن مرکز جدید</h3>
          <div class="add-clinic-form">
            <div class="searchable-dropdown">
              <input
                type="text"
                [(ngModel)]="searchTerm"
                (input)="onSearchChange()"
                (focus)="showDropdown = true"
                (blur)="onBlur()"
                placeholder="جستجو و انتخاب مرکز..."
                class="clinic-search-input"
                [disabled]="isLoadingClinics">
              <div class="dropdown-list" *ngIf="showDropdown && filteredClinics.length > 0">
                <div
                  class="dropdown-item"
                  *ngFor="let clinic of filteredClinics"
                  (mousedown)="selectClinic(clinic)"
                  [class.selected]="selectedClinicId === clinic.id">
                  {{ clinic.name }}
                </div>
              </div>
              <div class="dropdown-empty" *ngIf="showDropdown && filteredClinics.length === 0 && !isLoadingClinics">
                مرکزی یافت نشد
              </div>
              <div class="dropdown-loading" *ngIf="isLoadingClinics">
                در حال بارگذاری...
              </div>
            </div>
            <button class="btn btn-primary" (click)="addClinic()" [disabled]="!selectedClinicId || isSaving">
              افزودن
            </button>
          </div>
        </div>

        <div class="clinics-section">
          <h3>مراکز اختصاص داده شده</h3>
          <div class="clinics-list" *ngIf="userClinics.length > 0; else noClinics">
            <div class="clinic-item" *ngFor="let clinic of userClinics">
              <div class="clinic-info">
                <span class="clinic-name">{{ clinic.name }}</span>
                <span class="clinic-address" *ngIf="clinic.address">{{ clinic.address }}</span>
              </div>
              <button class="btn-icon btn-delete" (click)="removeClinic(clinic)" title="حذف">
                🗑️
              </button>
            </div>
          </div>
          <ng-template #noClinics>
            <div class="empty-state">هیچ مرکزی به این کاربر اختصاص داده نشده است</div>
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

    .clinics-section, .add-clinic-section {
      flex: 1;
    }

    .clinics-section h3, .add-clinic-section h3 {
      margin: 0 0 1rem 0;
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .clinics-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 300px;
      overflow-y: auto;
    }

    .clinic-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      background: var(--bg-secondary);
      border: 1px solid var(--border-light);
      border-radius: var(--radius-md);
      transition: all var(--transition-base);
    }

    .clinic-item:hover {
      background: var(--bg-tertiary);
      border-color: var(--primary-light);
    }

    .clinic-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex: 1;
    }

    .clinic-name {
      font-weight: 600;
      color: var(--text-primary);
    }

    .clinic-address {
      font-size: 0.85rem;
      color: var(--text-muted);
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--text-muted);
      font-size: 0.95rem;
    }

    .add-clinic-form {
      display: flex;
      gap: 12px;
      align-items: flex-start;
    }

    .searchable-dropdown {
      flex: 1;
      position: relative;
    }

    .clinic-search-input {
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

    .clinic-search-input:focus {
      outline: none;
      border-color: var(--primary);
      box-shadow: 0 0 0 4px rgba(6, 182, 212, 0.1);
    }

    .clinic-search-input:disabled {
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
export class UserClinicsDialogComponent implements OnInit {
  user: User | null = null;
  userClinics: Clinic[] = [];
  availableClinics: Clinic[] = [];
  allClinics: Clinic[] = [];
  filteredClinics: ClinicSimple[] = [];
  selectedClinicId: number | null = null;
  selectedClinicName: string = '';
  searchTerm: string = '';
  showDropdown: boolean = false;
  isLoadingClinics: boolean = false;
  isSaving = false;
  dialogRef: any = null;
  private searchSubject = new Subject<string>();

  constructor(
    private clinicService: ClinicService,
    private userService: UserService,
    private snackbarService: SnackbarService
  ) { }

  ngOnInit() {
    // DialogService sets user property directly, so we can access it immediately
    // Use setTimeout to ensure user is set before checking
    setTimeout(() => {
      if (this.user?.id) {
        this.loadUserClinics();
        this.setupSearch();
      } else {
        console.warn('User not set in UserClinicsDialogComponent', this.user);
      }
    }, 0);
  }

  setupSearch() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(searchTerm => {
        this.isLoadingClinics = true;
        return this.clinicService.getSimpleList(searchTerm || undefined).pipe(
          catchError(error => {
            this.isLoadingClinics = false;
            console.error('Error loading clinics:', error);
            const errorMessage = error.error?.message || error.message || 'خطا در بارگذاری لیست مراکز';
            this.snackbarService.error(errorMessage, 'بستن', 5000);
            return [];
          })
        );
      })
    ).subscribe({
      next: (clinics) => {
        console.log('Clinics loaded:', clinics);
        // Filter out clinics that user already has access to
        const userClinicIds = this.userClinics.map(c => c.id).filter(id => id != null);
        this.filteredClinics = clinics.filter(c => !userClinicIds.includes(c.id));
        this.isLoadingClinics = false;
      }
    });

    // Initial load - wait a bit to ensure subscription is ready
    setTimeout(() => {
      this.searchSubject.next('');
    }, 200);
  }

  onSearchChange() {
    this.searchSubject.next(this.searchTerm);
  }

  selectClinic(clinic: ClinicSimple) {
    this.selectedClinicId = clinic.id;
    this.selectedClinicName = clinic.name;
    this.searchTerm = clinic.name;
    this.showDropdown = false;
  }

  onBlur() {
    // Delay to allow click event to fire
    setTimeout(() => {
      this.showDropdown = false;
    }, 200);
  }

  loadUserClinics() {
    if (!this.user?.id) return;

    this.userService.getUserClinics(this.user.id).subscribe({
      next: (clinics) => {
        this.userClinics = clinics;
        // Refresh search results to filter out assigned clinics
        this.searchSubject.next(this.searchTerm);
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری مراکز کاربر';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
      }
    });
  }


  addClinic() {
    if (!this.selectedClinicId || !this.user?.id || this.isSaving) return;

    this.isSaving = true;
    this.userService.assignClinicToUser(this.user.id, this.selectedClinicId).subscribe({
      next: () => {
        this.snackbarService.success('مرکز با موفقیت به کاربر اختصاص داده شد', 'بستن', 3000);
        this.selectedClinicId = null;
        this.selectedClinicName = '';
        this.searchTerm = '';
        this.loadUserClinics();
        // Refresh search results
        this.searchSubject.next(this.searchTerm);
        this.isSaving = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در اختصاص مرکز به کاربر';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isSaving = false;
      }
    });
  }

  removeClinic(clinic: Clinic) {
    if (!clinic.id || !this.user?.id) return;

    this.userService.removeClinicFromUser(this.user.id, clinic.id).subscribe({
      next: () => {
        this.snackbarService.success('دسترسی کاربر به مرکز با موفقیت حذف شد', 'بستن', 3000);
        this.loadUserClinics();
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در حذف دسترسی کاربر به مرکز';
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
