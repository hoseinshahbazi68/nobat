import { Component } from '@angular/core';

export interface ConfirmDialogData {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'danger' | 'warning' | 'info';
}

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {
  data: ConfirmDialogData = {
    message: '',
    title: 'تأیید عملیات',
    confirmText: 'تأیید',
    cancelText: 'انصراف',
    type: 'danger'
  };
  dialogRef: any = null;

  constructor() {
    // Set defaults will be handled by DialogService
  }

  ngOnInit() {
    // Set defaults if not provided
    if (!this.data.title) {
      this.data.title = 'تأیید عملیات';
    }
    if (!this.data.confirmText) {
      this.data.confirmText = 'تأیید';
    }
    if (!this.data.cancelText) {
      this.data.cancelText = 'انصراف';
    }
    if (!this.data.type) {
      this.data.type = 'danger';
    }
  }

  onConfirm(): void {
    if (this.dialogRef) {
      this.dialogRef.close(true);
    }
  }

  onCancel(): void {
    if (this.dialogRef) {
      this.dialogRef.close(false);
    }
  }
}
