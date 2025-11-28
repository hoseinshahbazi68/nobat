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
    return this.get<PagedResult<DoctorVisitInfo>>('DoctorVisitInfos', params);
  }

  getById(id: number): Observable<DoctorVisitInfo> {
    return this.get<DoctorVisitInfo>(`DoctorVisitInfos/${id}`);
  }

  getByDoctorId(doctorId: number): Observable<DoctorVisitInfo> {
    return this.get<DoctorVisitInfo>(`DoctorVisitInfos/by-doctor/${doctorId}`);
  }

  create(data: CreateDoctorVisitInfoRequest): Observable<DoctorVisitInfo> {
    return this.post<DoctorVisitInfo>('DoctorVisitInfos', data);
  }

  update(data: UpdateDoctorVisitInfoRequest): Observable<DoctorVisitInfo> {
    return this.put<DoctorVisitInfo>(`DoctorVisitInfos/${data.id}`, data);
  }

  delete(id: number): Observable<boolean> {
    return this.deleteRequest<boolean>(`DoctorVisitInfos/${id}`);
  }
}
