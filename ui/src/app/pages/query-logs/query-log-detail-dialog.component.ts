import { Component } from '@angular/core';
import { QueryLog } from '../../models/query-log.model';
import { PersianDateService } from '../../services/persian-date.service';

@Component({
  selector: 'app-query-log-detail-dialog',
  templateUrl: './query-log-detail-dialog.component.html',
  styleUrls: ['./query-log-detail-dialog.component.scss']
})
export class QueryLogDetailDialogComponent {
  data: QueryLog = {
    id: 0,
    queryText: '',
    executionTimeMs: 0,
    executionTime: '',
    isHeavy: false,
    createdAt: ''
  };
  dialogRef: any = null;

  constructor(private persianDateService: PersianDateService) {}

  close() {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }

  formatDate(date: string): string {
    return this.persianDateService.formatDateTime(date);
  }

  formatTime(ms: number): string {
    if (ms < 1000) {
      return `${ms} ms`;
    } else if (ms < 60000) {
      return `${(ms / 1000).toFixed(2)} s`;
    } else {
      return `${(ms / 60000).toFixed(2)} min`;
    }
  }

  getTimeClass(ms: number): string {
    if (ms > 5000) return 'execution-time-critical';
    if (ms > 1000) return 'execution-time-warning';
    return 'execution-time-normal';
  }

  formatParameters(params?: string): string {
    if (!params) return '';
    try {
      const parsed = JSON.parse(params);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return params;
    }
  }
}
