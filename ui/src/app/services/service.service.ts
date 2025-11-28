import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { Service, CreateServiceRequest, UpdateServiceRequest } from '../models/service.model';
import { PagedResult } from '../models/paged-result.model';

export interface ServiceQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ServiceService extends BaseApiService {
  getAll(params?: ServiceQueryParams): Observable<PagedResult<Service>> {
    return this.get<PagedResult<Service>>('services', params);
  }

  getById(id: number): Observable<Service> {
    return this.get<Service>(`services/${id}`);
  }

  create(service: CreateServiceRequest): Observable<Service> {
    return this.post<Service>('services', service);
  }

  update(service: UpdateServiceRequest): Observable<Service> {
    return this.put<Service>(`services/${service.id}`, service);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`services/${id}`);
  }
}

