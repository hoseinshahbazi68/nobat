import { Component, OnInit } from '@angular/core';
import { Clinic } from '../../models/clinic.model';
import { User } from '../../models/user.model';
import { ClinicService } from '../../services/clinic.service';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-clinic-users-dialog',
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <h2>کاربران کلینیک: {{ clinic?.name }}</h2>
      </div>
      <div class="dialog-content">
        <div class="users-section">
          <h3>کاربران اختصاص داده شده</h3>
          <div class="users-list" *ngIf="clinicUsers.length > 0; else noUsers">
            <div class="user-item" *ngFor="let user of clinicUsers">
              <div class="user-info">
                <span class="user-name">{{ user.firstName }} {{ user.lastName }}</span>
                <span class="user-details">
                  <span *ngIf="user.nationalCode">کد ملی: {{ user.nationalCode }}</span>
                  <span *ngIf="user.email"> | ایمیل: {{ user.email }}</span>
                  <span *ngIf="user.phoneNumber"> | تلفن: {{ user.phoneNumber }}</span>
                </span>
                <span class="user-roles" *ngIf="user.roles && user.roles.length > 0">
                  نقش‌ها: {{ user.roles.join(', ') }}
                </span>
              </div>
            </div>
          </div>
          <ng-template #noUsers>
            <div class="empty-state">هیچ کاربری به این کلینیک اختصاص داده نشده است</div>
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

    .users-section {
      flex: 1;
    }

    .users-section h3 {
      margin: 0 0 1rem 0;
      font-size: 1.1rem;
      font-weight: 700;
      color: var(--text-primary);
    }

    .users-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 400px;
      overflow-y: auto;
    }

    .user-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      background: var(--bg-secondary);
      border: 1px solid var(--border-light);
      border-radius: var(--radius-md);
      transition: all var(--transition-base);
    }

    .user-item:hover {
      background: var(--bg-tertiary);
      border-color: var(--primary-light);
    }

    .user-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex: 1;
    }

    .user-name {
      font-weight: 600;
      color: var(--text-primary);
      font-size: 1rem;
    }

    .user-details {
      font-size: 0.85rem;
      color: var(--text-muted);
    }

    .user-roles {
      font-size: 0.85rem;
      color: var(--primary);
      font-weight: 500;
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--text-muted);
      font-size: 0.95rem;
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

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
      border: 2px solid var(--border-color);
    }

    .btn-secondary:hover {
      background: var(--bg-secondary);
      border-color: var(--primary);
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
export class ClinicUsersDialogComponent implements OnInit {
  clinic: Clinic | null = null;
  clinicUsers: User[] = [];
  isLoading = false;
  dialogRef: any = null;

  constructor(
    private clinicService: ClinicService,
    private snackbarService: SnackbarService
  ) { }

  ngOnInit() {
    // DialogService sets clinic property directly, so we can access it immediately
    // Use setTimeout to ensure clinic is set before checking
    setTimeout(() => {
      if (this.clinic?.id) {
        this.loadClinicUsers();
      } else {
        console.warn('Clinic not set in ClinicUsersDialogComponent', this.clinic);
      }
    }, 0);
  }

  loadClinicUsers() {
    if (!this.clinic?.id) return;

    this.isLoading = true;
    this.clinicService.getClinicUsers(this.clinic.id).subscribe({
      next: (users) => {
        this.clinicUsers = users;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری کاربران کلینیک';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onClose() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }
}
