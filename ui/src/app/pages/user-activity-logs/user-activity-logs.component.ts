import { Component, OnInit } from '@angular/core';
import { SnackbarService } from '../../services/snackbar.service';
import { UserActivityLogService } from '../../services/user-activity-log.service';
import { UserActivityLog } from '../../models/user-activity-log.model';
import { PersianDateService } from '../../services/persian-date.service';

@Component({
  selector: 'app-user-activity-logs',
  templateUrl: './user-activity-logs.component.html',
  styleUrls: ['./user-activity-logs.component.scss']
})
export class UserActivityLogsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'user', 'action', 'controller', 'httpMethod', 'requestPath', 'ipAddress', 'statusCode', 'activityTime'];
  activityLogs: UserActivityLog[] = [];
  filteredActivityLogs: UserActivityLog[] = [];
  filterValue: string = '';
  isLoading = false;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private snackbarService: SnackbarService,
    private userActivityLogService: UserActivityLogService,
    private persianDateService: PersianDateService
  ) {}

  ngOnInit() {
    this.loadActivityLogs();
  }

  loadActivityLogs() {
    this.isLoading = true;
    this.userActivityLogService.getAll({ page: this.currentPage, pageSize: this.pageSize }).subscribe({
      next: (result) => {
        this.activityLogs = result.items;
        this.filteredActivityLogs = [...this.activityLogs];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری لاگ فعالیت‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadActivityLogs();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadActivityLogs();
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredActivityLogs = [...this.activityLogs];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredActivityLogs = this.activityLogs.filter(log =>
      (log.username && log.username.toLowerCase().includes(filter)) ||
      (log.userFullName && log.userFullName.toLowerCase().includes(filter)) ||
      (log.action && log.action.toLowerCase().includes(filter)) ||
      (log.controller && log.controller.toLowerCase().includes(filter)) ||
      (log.requestPath && log.requestPath.toLowerCase().includes(filter)) ||
      (log.ipAddress && log.ipAddress.toLowerCase().includes(filter))
    );
  }

  getUserDisplayName(log: UserActivityLog): string {
    if (log.userFullName) {
      return log.userFullName;
    }
    if (log.username) {
      return log.username;
    }
    return 'نامشخص';
  }

  getStatusBadgeClass(statusCode?: number): string {
    if (!statusCode) return 'status-unknown';
    if (statusCode >= 200 && statusCode < 300) return 'status-success';
    if (statusCode >= 400 && statusCode < 500) return 'status-warning';
    if (statusCode >= 500) return 'status-error';
    return 'status-unknown';
  }

  getStatusText(statusCode?: number): string {
    if (!statusCode) return 'نامشخص';
    return statusCode.toString();
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return this.persianDateService.toPersianFull(date);
    } catch {
      return dateString;
    }
  }

  viewDetails(log: UserActivityLog) {
    // می‌توانید یک dialog برای نمایش جزئیات بیشتر ایجاد کنید
    const details = [
      `شناسه: ${log.id}`,
      `کاربر: ${this.getUserDisplayName(log)}`,
      `فعالیت: ${log.action}`,
      `کنترلر: ${log.controller || '-'}`,
      `متد HTTP: ${log.httpMethod || '-'}`,
      `مسیر: ${log.requestPath || '-'}`,
      `IP: ${log.ipAddress || '-'}`,
      `وضعیت: ${this.getStatusText(log.statusCode)}`,
      `زمان: ${this.formatDate(log.activityTime)}`,
      log.errorMessage ? `خطا: ${log.errorMessage}` : '',
      log.userAgent ? `User Agent: ${log.userAgent}` : ''
    ].filter(d => d).join('\n');

    alert(details);
  }
}
