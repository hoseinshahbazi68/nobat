import { Component, OnInit } from '@angular/core';
import { DatabaseChangeLogService } from '../../services/database-change-log.service';
import { SnackbarService } from '../../services/snackbar.service';
import { DatabaseChangeLog } from '../../models/database-change-log.model';
import { PersianDateService } from '../../services/persian-date.service';
import { PagedResult } from '../../models/paged-result.model';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-database-change-logs',
  templateUrl: './database-change-logs.component.html',
  styleUrls: ['./database-change-logs.component.scss']
})
export class DatabaseChangeLogsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'tableName', 'recordId', 'changeType', 'changeTime', 'ipAddress', 'actions'];
  changeLogs: DatabaseChangeLog[] = [];
  filteredChangeLogs: DatabaseChangeLog[] = [];
  filterValue: string = '';
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // فیلترها
  selectedTableName: string = '';
  selectedChangeType: string = '';
  tableNames: string[] = [];
  changeTypes: string[] = ['Added', 'Modified', 'Deleted'];

  constructor(
    private databaseChangeLogService: DatabaseChangeLogService,
    private snackbarService: SnackbarService,
    private persianDateService: PersianDateService
  ) {}

  ngOnInit() {
    this.loadChangeLogs();
  }

  loadChangeLogs() {
    this.isLoading = true;
    const params: any = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    let request: any;
    if (this.selectedTableName) {
      request = this.databaseChangeLogService.getByTableName(this.selectedTableName, params);
    } else if (this.selectedChangeType) {
      request = this.databaseChangeLogService.getByChangeType(this.selectedChangeType, params);
    } else {
      request = this.databaseChangeLogService.getAll(params);
    }

    request.subscribe({
      next: (result: PagedResult<DatabaseChangeLog>) => {
        if (result && result.items) {
          this.changeLogs = result.items;
          this.filteredChangeLogs = [...this.changeLogs];
          this.totalCount = result.totalCount || 0;
          this.totalPages = result.totalPages || Math.ceil(this.totalCount / this.pageSize);

          // استخراج نام جداول منحصر به فرد
          this.tableNames = [...new Set(this.changeLogs.map(log => log.tableName))].sort();
        } else {
          this.changeLogs = [];
          this.filteredChangeLogs = [];
          this.totalCount = 0;
          this.totalPages = 0;
        }
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری لاگ تغییرات';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredChangeLogs = [...this.changeLogs];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredChangeLogs = this.changeLogs.filter(log =>
      log.tableName.toLowerCase().includes(filter) ||
      log.recordId.toLowerCase().includes(filter) ||
      log.changeType.toLowerCase().includes(filter)
    );
  }

  onTableNameFilterChange() {
    this.currentPage = 1;
    this.loadChangeLogs();
  }

  onChangeTypeFilterChange() {
    this.currentPage = 1;
    this.loadChangeLogs();
  }

  clearFilters() {
    this.selectedTableName = '';
    this.selectedChangeType = '';
    this.filterValue = '';
    this.currentPage = 1;
    this.loadChangeLogs();
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadChangeLogs();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadChangeLogs();
  }

  getChangeTypeLabel(changeType: string): string {
    const labels: { [key: string]: string } = {
      'Added': 'افزودن',
      'Modified': 'ویرایش',
      'Deleted': 'حذف'
    };
    return labels[changeType] || changeType;
  }

  getChangeTypeClass(changeType: string): string {
    const classes: { [key: string]: string } = {
      'Added': 'change-type-added',
      'Modified': 'change-type-modified',
      'Deleted': 'change-type-deleted'
    };
    return classes[changeType] || '';
  }

  formatDate(dateString: string): string {
    return this.persianDateService.formatDateTime(dateString);
  }

  viewDetails(changeLog: DatabaseChangeLog) {
    // نمایش جزئیات در یک dialog یا modal
    const details = `
      جدول: ${changeLog.tableName}
      شناسه رکورد: ${changeLog.recordId}
      نوع تغییر: ${this.getChangeTypeLabel(changeLog.changeType)}
      زمان: ${this.formatDate(changeLog.changeTime)}
      IP: ${changeLog.ipAddress || 'نامشخص'}
      ${changeLog.changedColumns ? `ستون‌های تغییر یافته: ${changeLog.changedColumns}` : ''}
    `;
    alert(details);
  }
}
