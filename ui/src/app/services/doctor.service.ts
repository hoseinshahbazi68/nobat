import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateDoctorRequest, Doctor, DoctorListDto, UpdateDoctorRequest } from '../models/doctor.model';
import { PagedResult } from '../models/paged-result.model';
import { BaseApiService } from './base-api.service';

export interface DoctorQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
  clinicId?: number;
}

export interface DoctorSearchParams {
  query?: string;
  clinicName?: string;
  doctorName?: string;
  page?: number;
  pageSize?: number;
}

export interface DoctorSearchByMedicalConditionParams {
  medicalConditionName: string;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class DoctorService extends BaseApiService {
  getAll(params?: DoctorQueryParams): Observable<PagedResult<DoctorListDto>> {
    return this.get<PagedResult<DoctorListDto>>('doctors', params);
  }

  getById(id: number): Observable<Doctor> {
    return this.get<Doctor>(`doctors/${id}`);
  }

  create(doctor: CreateDoctorRequest): Observable<Doctor> {
    return this.post<Doctor>('doctors', doctor);
  }

  createWithFile(doctor: CreateDoctorRequest, file: File | null): Observable<Doctor> {
    const formData = new FormData();
    Object.keys(doctor).forEach(key => {
      const value = (doctor as any)[key];
      if (value !== null && value !== undefined) {
        if (Array.isArray(value)) {
          value.forEach((item, index) => {
            formData.append(`${key}[${index}]`, item.toString());
          });
        } else if (value instanceof Date) {
          formData.append(key, value.toISOString());
        } else {
          formData.append(key, value.toString());
        }
      }
    });
    if (file) {
      formData.append('profilePictureFile', file);
    }
    return this.postWithFile<Doctor>('doctors/with-file', formData);
  }

  update(doctor: UpdateDoctorRequest): Observable<Doctor> {
    return this.put<Doctor>(`doctors/${doctor.id}`, doctor);
  }

  updateWithFile(doctor: UpdateDoctorRequest, file: File | null): Observable<Doctor> {
    const formData = new FormData();
    Object.keys(doctor).forEach(key => {
      const value = (doctor as any)[key];
      if (value !== null && value !== undefined) {
        if (Array.isArray(value)) {
          value.forEach((item, index) => {
            formData.append(`${key}[${index}]`, item.toString());
          });
        } else if (value instanceof Date) {
          formData.append(key, value.toISOString());
        } else {
          formData.append(key, value.toString());
        }
      }
    });
    if (file) {
      formData.append('profilePictureFile', file);
    }
    return this.putWithFile<Doctor>(`doctors/${doctor.id}/with-file`, formData);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`doctors/${id}`);
  }

  search(params: DoctorSearchParams): Observable<PagedResult<Doctor>> {
    return this.get<PagedResult<Doctor>>('doctors/search', params);
  }

  getByMedicalCode(medicalCode: string): Observable<Doctor> {
    return this.get<Doctor>(`doctors/by-medical-code/${encodeURIComponent(medicalCode)}`);
  }

  searchByMedicalCondition(params: DoctorSearchByMedicalConditionParams): Observable<PagedResult<Doctor>> {
    return this.get<PagedResult<Doctor>>('doctors/by-medical-condition', params);
  }
}
