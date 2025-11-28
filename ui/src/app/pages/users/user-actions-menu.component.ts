import { Component, OnInit } from '@angular/core';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-user-actions-menu',
  template: `
    <div class="actions-menu">
      <button class="menu-item" (click)="onAction('edit')">
        <span class="icon">✏️</span>
        <span>ویرایش</span>
      </button>
      <button class="menu-item" (click)="onAction('clinics')">
        <span class="icon">🏢</span>
        <span>مراکز</span>
      </button>
      <button class="menu-item" (click)="onAction('change-password')">
        <span class="icon">🔒</span>
        <span>تغییر کلمه عبور</span>
      </button>
      <button class="menu-item delete" (click)="onAction('delete')">
        <span class="icon">🗑️</span>
        <span>حذف</span>
      </button>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      padding: 0;
      border-radius: var(--radius-md);
    }

    .actions-menu {
      display: flex;
      flex-direction: column;
      min-width: 200px;
      padding: 8px;
      background: white;
      border-radius: var(--radius-md);
    }

    .menu-item {
      display: flex;
      align-items: center;
      gap: 12px;
      width: 100%;
      padding: 12px 16px;
      border: none;
      background: white;
      text-align: right;
      cursor: pointer;
      transition: all var(--transition-base);
      font-family: 'Vazirmatn', sans-serif;
      font-size: 0.95rem;
      color: var(--text-primary);
      border-radius: var(--radius-md);

      &:hover {
        background: var(--bg-secondary);
      }

      &.delete {
        color: #ef4444;

        &:hover {
          background: rgba(239, 68, 68, 0.1);
        }
      }

      .icon {
        font-size: 1.1rem;
      }
    }
  `]
})
export class UserActionsMenuComponent implements OnInit {
  data: User | null = null;
  dialogRef: any = null;

  ngOnInit() {
    // Data is set by DialogService
  }

  onAction(action: string) {
    if (this.dialogRef && this.data) {
      this.dialogRef.close({ action, user: this.data });
    }
  }
}
