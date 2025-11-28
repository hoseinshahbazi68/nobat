import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { MedicalCondition, CreateMedicalConditionRequest, UpdateMedicalConditionRequest } from '../models/medical-condition.model';
import { PagedResult } from '../models/paged-result.model';

export interface MedicalConditionQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MedicalConditionService extends BaseApiService {
  getAll(params?: MedicalConditionQueryParams): Observable<PagedResult<MedicalCondition>> {
    return this.get<PagedResult<MedicalCondition>>('medicalconditions', params);
  }

  getById(id: number): Observable<MedicalCondition> {
    return this.get<MedicalCondition>(`medicalconditions/${id}`);
  }

  create(medicalCondition: CreateMedicalConditionRequest): Observable<MedicalCondition> {
    return this.post<MedicalCondition>('medicalconditions', medicalCondition);
  }

  update(medicalCondition: UpdateMedicalConditionRequest): Observable<MedicalCondition> {
    return this.put<MedicalCondition>(`medicalconditions/${medicalCondition.id}`, medicalCondition);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`medicalconditions/${id}`);
  }

  search(query: string): Observable<MedicalCondition[]> {
    return this.get<MedicalCondition[]>(`medicalconditions/search`, { query });
  }
}
