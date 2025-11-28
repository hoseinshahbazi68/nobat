import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { User, CreateUserRequest, UpdateUserRequest } from '../models/user.model';
import { Clinic } from '../models/clinic.model';
import { PagedResult } from '../models/paged-result.model';

export interface UserQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseApiService {
  getAll(params?: UserQueryParams): Observable<PagedResult<User>> {
    return this.get<PagedResult<User>>('users', params);
  }

  getById(id: number): Observable<User> {
    return this.get<User>(`users/${id}`);
  }

  create(user: CreateUserRequest): Observable<User> {
    return this.post<User>('users', user);
  }

  update(user: UpdateUserRequest): Observable<User> {
    return this.put<User>(`users/${user.id}`, user);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`users/${id}`);
  }

  searchByNationalCode(nationalCode: string): Observable<User> {
    return this.get<User>(`users/by-national-code/${encodeURIComponent(nationalCode)}`);
  }

  getUserClinics(userId: number): Observable<Clinic[]> {
    return this.get<Clinic[]>(`users/${userId}/clinics`);
  }

  assignClinicToUser(userId: number, clinicId: number): Observable<void> {
    return this.post<void>(`users/${userId}/clinics`, { clinicId });
  }

  removeClinicFromUser(userId: number, clinicId: number): Observable<void> {
    return this.deleteRequest<void>(`users/${userId}/clinics/${clinicId}`);
  }
}
