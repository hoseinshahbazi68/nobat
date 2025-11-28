import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { Shift, CreateShiftRequest, UpdateShiftRequest } from '../models/shift.model';
import { PagedResult } from '../models/paged-result.model';

export interface ShiftQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ShiftService extends BaseApiService {
  getAll(params?: ShiftQueryParams): Observable<PagedResult<Shift>> {
    return this.get<PagedResult<Shift>>('shifts', params);
  }

  getById(id: number): Observable<Shift> {
    return this.get<Shift>(`shifts/${id}`);
  }

  create(shift: CreateShiftRequest): Observable<Shift> {
    return this.post<Shift>('shifts', shift);
  }

  update(shift: UpdateShiftRequest): Observable<Shift> {
    return this.put<Shift>(`shifts/${shift.id}`, shift);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`shifts/${id}`);
  }
}

