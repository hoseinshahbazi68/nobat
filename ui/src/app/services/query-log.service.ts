import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { QueryLog, QueryLogStatistics, QueryLogQueryParams } from '../models/query-log.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class QueryLogService extends BaseApiService {
  getAll(params?: QueryLogQueryParams): Observable<PagedResult<QueryLog>> {
    return this.get<PagedResult<QueryLog>>('querylogs', params);
  }

  getById(id: number): Observable<QueryLog> {
    return this.get<QueryLog>(`querylogs/${id}`);
  }

  getHeavyQueries(params?: QueryLogQueryParams): Observable<PagedResult<QueryLog>> {
    return this.get<PagedResult<QueryLog>>('querylogs/heavy', params);
  }

  getStatistics(): Observable<QueryLogStatistics> {
    return this.get<QueryLogStatistics>('querylogs/statistics');
  }

  deleteOldLogs(daysToKeep: number): Observable<number> {
    return this.deleteRequest<number>(`querylogs/old/${daysToKeep}`);
  }
}
