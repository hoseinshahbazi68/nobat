import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { Role, CreateRoleRequest, UpdateRoleRequest } from '../models/role.model';
import { PagedResult } from '../models/paged-result.model';

export interface RoleQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RoleService extends BaseApiService {
  getAll(params?: RoleQueryParams): Observable<PagedResult<Role>> {
    return this.get<PagedResult<Role>>('roles', params);
  }

  getById(id: number): Observable<Role> {
    return this.get<Role>(`roles/${id}`);
  }

  create(role: CreateRoleRequest): Observable<Role> {
    return this.post<Role>('roles', role);
  }

  update(role: UpdateRoleRequest): Observable<Role> {
    return this.put<Role>(`roles/${role.id}`, role);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`roles/${id}`);
  }
}

