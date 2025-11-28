import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { UserActivityLog } from '../models/user-activity-log.model';
import { PagedResult } from '../models/paged-result.model';

export interface UserActivityLogQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserActivityLogService extends BaseApiService {
  getAll(params?: UserActivityLogQueryParams): Observable<PagedResult<UserActivityLog>> {
    return this.get<PagedResult<UserActivityLog>>('useractivitylogs', params);
  }

  getById(id: number): Observable<UserActivityLog> {
    return this.get<UserActivityLog>(`useractivitylogs/${id}`);
  }

  getByUserId(userId: number, params?: UserActivityLogQueryParams): Observable<PagedResult<UserActivityLog>> {
    return this.get<PagedResult<UserActivityLog>>(`useractivitylogs/user/${userId}`, params);
  }
}
