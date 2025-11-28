import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { DatabaseChangeLog, DatabaseChangeLogQueryParams } from '../models/database-change-log.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class DatabaseChangeLogService extends BaseApiService {
  getAll(params?: DatabaseChangeLogQueryParams): Observable<PagedResult<DatabaseChangeLog>> {
    return this.get<PagedResult<DatabaseChangeLog>>('databasechangelogs', params);
  }

  getById(id: number): Observable<DatabaseChangeLog> {
    return this.get<DatabaseChangeLog>(`databasechangelogs/${id}`);
  }

  getByTableName(tableName: string, params?: DatabaseChangeLogQueryParams): Observable<PagedResult<DatabaseChangeLog>> {
    return this.get<PagedResult<DatabaseChangeLog>>(`databasechangelogs/table/${tableName}`, params);
  }

  getByChangeType(changeType: string, params?: DatabaseChangeLogQueryParams): Observable<PagedResult<DatabaseChangeLog>> {
    return this.get<PagedResult<DatabaseChangeLog>>(`databasechangelogs/type/${changeType}`, params);
  }
}
