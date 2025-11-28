import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { ServiceTariff, CreateServiceTariffRequest, UpdateServiceTariffRequest } from '../models/service-tariff.model';
import { PagedResult } from '../models/paged-result.model';

export interface ServiceTariffQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
  serviceId?: number;
  insuranceId?: number;
  clinicId?: number;
  doctorId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ServiceTariffService extends BaseApiService {
  getAll(params?: ServiceTariffQueryParams): Observable<PagedResult<ServiceTariff>> {
    return this.get<PagedResult<ServiceTariff>>('ServiceTariffs', params);
  }

  getById(id: number): Observable<ServiceTariff> {
    return this.get<ServiceTariff>(`ServiceTariffs/${id}`);
  }

  create(tariff: CreateServiceTariffRequest): Observable<ServiceTariff> {
    return this.post<ServiceTariff>('ServiceTariffs', tariff);
  }

  update(tariff: UpdateServiceTariffRequest): Observable<ServiceTariff> {
    return this.put<ServiceTariff>(`ServiceTariffs/${tariff.id}`, tariff);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`ServiceTariffs/${id}`);
  }
}
