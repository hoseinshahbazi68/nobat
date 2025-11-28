import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { PagedResult } from '../models/paged-result.model';
import { City, CreateCityRequest, UpdateCityRequest } from '../models/city.model';

export interface CityQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
  provinceId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class CityService extends BaseApiService {
  getAll(params?: CityQueryParams): Observable<PagedResult<City>> {
    return this.get<PagedResult<City>>('Cities', params);
  }

  getById(id: number): Observable<City> {
    return this.get<City>(`Cities/${id}`);
  }

  getByProvinceId(provinceId: number): Observable<City[]> {
    return this.get<City[]>(`Cities/province/${provinceId}`);
  }

  create(city: CreateCityRequest): Observable<City> {
    return this.post<City>('Cities', city);
  }

  update(city: UpdateCityRequest): Observable<City> {
    return this.put<City>(`Cities/${city.id}`, city);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`Cities/${id}`);
  }

  // متدهای عمومی بدون نیاز به احراز هویت (برای صفحه ثبت‌نام)
  getAllPublic(params?: CityQueryParams): Observable<PagedResult<City>> {
    return this.get<PagedResult<City>>('Cities/public', params);
  }

  getByIdPublic(id: number): Observable<City> {
    return this.get<City>(`Cities/public/${id}`);
  }
}
