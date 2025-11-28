import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { Specialty, CreateSpecialtyRequest, UpdateSpecialtyRequest, SpecialtyMedicalCondition } from '../models/specialty.model';
import { PagedResult } from '../models/paged-result.model';

export interface SpecialtyQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SpecialtyService extends BaseApiService {
  getAll(params?: SpecialtyQueryParams): Observable<PagedResult<Specialty>> {
    return this.get<PagedResult<Specialty>>('specialties', params);
  }

  getById(id: number): Observable<Specialty> {
    return this.get<Specialty>(`specialties/${id}`);
  }

  create(specialty: CreateSpecialtyRequest): Observable<Specialty> {
    return this.post<Specialty>('specialties', specialty);
  }

  update(specialty: UpdateSpecialtyRequest): Observable<Specialty> {
    return this.put<Specialty>(`specialties/${specialty.id}`, specialty);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`specialties/${id}`);
  }

  getMedicalConditions(specialtyId: number): Observable<SpecialtyMedicalCondition[]> {
    return this.get<SpecialtyMedicalCondition[]>(`specialties/${specialtyId}/medical-conditions`);
  }

  addMedicalCondition(specialtyId: number, medicalConditionId: number): Observable<SpecialtyMedicalCondition> {
    return this.post<SpecialtyMedicalCondition>(`specialties/${specialtyId}/medical-conditions/${medicalConditionId}`, {});
  }

  removeMedicalCondition(specialtyId: number, medicalConditionId: number): Observable<void> {
    return this.deleteRequest<void>(`specialties/${specialtyId}/medical-conditions/${medicalConditionId}`);
  }
}
