import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/paged-result.model';
import { Province, CreateProvinceRequest, UpdateProvinceRequest } from '../models/province.model';

export interface ProvinceQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProvinceService extends BaseApiService {
  getAll(params?: ProvinceQueryParams): Observable<PagedResult<Province>> {
    return this.get<PagedResult<Province>>('Provinces', params);
  }

  getById(id: number): Observable<Province> {
    return this.get<Province>(`Provinces/${id}`);
  }

  create(province: CreateProvinceRequest): Observable<Province> {
    return this.post<Province>('Provinces', province);
  }

  update(province: UpdateProvinceRequest): Observable<Province> {
    return this.put<Province>(`Provinces/${province.id}`, province);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`Provinces/${id}`);
  }

  // متد عمومی بدون نیاز به احراز هویت (برای صفحه اصلی)
  getAllPublic(params?: ProvinceQueryParams): Observable<PagedResult<Province>> {
    return this.get<PagedResult<Province>>('Provinces/public', params);
  }
}
