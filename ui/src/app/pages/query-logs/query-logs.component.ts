import { Component, OnInit } from '@angular/core';
import { QueryLogService } from '../../services/query-log.service';
import { SnackbarService } from '../../services/snackbar.service';
import { DialogService } from '../../services/dialog.service';
import { QueryLog, QueryLogStatistics } from '../../models/query-log.model';
import { PersianDateService } from '../../services/persian-date.service';
import { QueryLogDetailDialogComponent } from './query-log-detail-dialog.component';

@Component({
  selector: 'app-query-logs',
  templateUrl: './query-logs.component.html',
  styleUrls: ['./query-logs.component.scss']
})
export class QueryLogsComponent implements OnInit {
  displayedColumns: string[] = ['id', 'executionTime', 'executionTimeMs', 'commandType', 'controllerAction', 'isHeavy', 'actions'];
  queryLogs: QueryLog[] = [];
  filteredQueryLogs: QueryLog[] = [];
  filterValue: string = '';
  isLoading = false;
  showHeavyOnly = false;
  statistics: QueryLogStatistics | null = null;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private queryLogService: QueryLogService,
    private snackbarService: SnackbarService,
    private dialogService: DialogService,
    private persianDateService: PersianDateService
  ) {}

  ngOnInit() {
    this.loadQueryLogs();
    this.loadStatistics();
  }

  loadQueryLogs() {
    this.isLoading = true;
    const serviceCall = this.showHeavyOnly
      ? this.queryLogService.getHeavyQueries({ page: this.currentPage, pageSize: this.pageSize })
      : this.queryLogService.getAll({ page: this.currentPage, pageSize: this.pageSize });

    serviceCall.subscribe({
      next: (result) => {
        this.queryLogs = result.items;
        this.filteredQueryLogs = [...this.queryLogs];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری لاگ کوئری‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  loadStatistics() {
    this.queryLogService.getStatistics().subscribe({
      next: (result) => {
        this.statistics = result;
      },
      error: (error) => {
        // خطا در بارگذاری آمار را نادیده می‌گیریم
        console.error('خطا در بارگذاری آمار:', error);
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadQueryLogs();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadQueryLogs();
  }

  toggleHeavyOnly() {
    this.showHeavyOnly = !this.showHeavyOnly;
    this.currentPage = 1;
    this.loadQueryLogs();
  }

  viewDetails(queryLog: QueryLog) {
    this.dialogService.open(QueryLogDetailDialogComponent, {
      width: '900px',
      maxWidth: '95vw',
      data: queryLog
    });
  }

  deleteOldLogs() {
    const daysInput = prompt('تعداد روزهایی که می‌خواهید لاگ‌ها نگه داشته شوند را وارد کنید (مثلاً 30):');
    if (daysInput) {
      const daysToKeep = parseInt(daysInput, 10);
      if (daysToKeep > 0) {
        this.dialogService.confirm({
          title: 'حذف لاگ‌های قدیمی',
          message: `آیا از حذف لاگ‌های قدیمی‌تر از ${daysToKeep} روز اطمینان دارید؟`,
          confirmText: 'حذف',
          cancelText: 'انصراف',
          type: 'danger'
        }).subscribe(result => {
          if (result) {
            this.queryLogService.deleteOldLogs(daysToKeep).subscribe({
              next: (deletedCount) => {
                this.snackbarService.success(`${deletedCount} لاگ قدیمی با موفقیت حذف شد`, 'بستن', 5000);
                this.loadQueryLogs();
                this.loadStatistics();
              },
              error: (error) => {
                const errorMessage = error.error?.message || 'خطا در حذف لاگ‌های قدیمی';
                this.snackbarService.error(errorMessage, 'بستن', 5000);
              }
            });
          }
        });
      } else {
        this.snackbarService.error('لطفاً یک عدد معتبر وارد کنید', 'بستن', 3000);
      }
    }
  }

  applyFilter() {
    if (!this.filterValue.trim()) {
      this.filteredQueryLogs = [...this.queryLogs];
      return;
    }
    const filter = this.filterValue.trim().toLowerCase();
    this.filteredQueryLogs = this.queryLogs.filter(log =>
      log.queryText.toLowerCase().includes(filter) ||
      (log.commandType && log.commandType.toLowerCase().includes(filter)) ||
      (log.controllerAction && log.controllerAction.toLowerCase().includes(filter))
    );
  }

  formatExecutionTime(date: string): string {
    return this.persianDateService.formatDateTime(date);
  }

  formatExecutionTimeMs(ms: number): string {
    if (ms < 1000) {
      return `${ms} ms`;
    } else if (ms < 60000) {
      return `${(ms / 1000).toFixed(2)} s`;
    } else {
      return `${(ms / 60000).toFixed(2)} min`;
    }
  }

  getExecutionTimeClass(ms: number): string {
    if (ms > 5000) return 'execution-time-critical';
    if (ms > 1000) return 'execution-time-warning';
    return 'execution-time-normal';
  }
}
