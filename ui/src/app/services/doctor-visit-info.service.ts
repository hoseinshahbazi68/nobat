import { Injectable } from '@angular/core';
import { BaseApiService } from './base-api.service';
import { Observable } from 'rxjs';
import { DoctorVisitInfo, CreateDoctorVisitInfoRequest, UpdateDoctorVisitInfoRequest } from '../models/doctor-visit-info.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class DoctorVisitInfoService extends BaseApiService {
  getAll(params?: any): Observable<PagedResult<DoctorVisitInfo>> {
    return this.get<PagedResult<DoctorVisitInfo>>('doctor-visit-infos', params);
  }

  getById(id: number): Observable<DoctorVisitInfo> {
    return this.get<DoctorVisitInfo>(`doctor-visit-infos/${id}`);
  }

  getByDoctorId(doctorId: number): Observable<DoctorVisitInfo> {
    return this.get<DoctorVisitInfo>(`doctor-visit-infos/by-doctor/${doctorId}`);
  }

  create(data: CreateDoctorVisitInfoRequest): Observable<DoctorVisitInfo> {
    return this.post<DoctorVisitInfo>('doctor-visit-infos', data);
  }

  update(data: UpdateDoctorVisitInfoRequest): Observable<DoctorVisitInfo> {
    return this.put<DoctorVisitInfo>(`doctor-visit-infos/${data.id}`, data);
  }

  delete(id: number): Observable<boolean> {
    return this.deleteRequest<boolean>(`doctor-visit-infos/${id}`);
  }
}
