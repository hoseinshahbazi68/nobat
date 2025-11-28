import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { Insurance, CreateInsuranceRequest, UpdateInsuranceRequest } from '../models/insurance.model';
import { PagedResult } from '../models/paged-result.model';

export interface InsuranceQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class InsuranceService extends BaseApiService {
  getAll(params?: InsuranceQueryParams): Observable<PagedResult<Insurance>> {
    return this.get<PagedResult<Insurance>>('insurances', params);
  }

  getById(id: number): Observable<Insurance> {
    return this.get<Insurance>(`insurances/${id}`);
  }

  create(insurance: CreateInsuranceRequest): Observable<Insurance> {
    return this.post<Insurance>('insurances', insurance);
  }

  update(insurance: UpdateInsuranceRequest): Observable<Insurance> {
    return this.put<Insurance>(`insurances/${insurance.id}`, insurance);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`insurances/${id}`);
  }
}

