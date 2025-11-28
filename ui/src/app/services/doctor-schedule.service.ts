import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateDoctorScheduleRequest, DoctorSchedule, UpdateDoctorScheduleRequest, WeeklySchedule } from '../models/doctor-schedule.model';
import { PagedResult } from '../models/paged-result.model';
import { BaseApiService } from './base-api.service';

export interface DoctorScheduleQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
  doctorId?: number;
  date?: string;
  startDate?: string;
  endDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DoctorScheduleService extends BaseApiService {
  getAll(params?: DoctorScheduleQueryParams): Observable<PagedResult<DoctorSchedule>> {
    return this.get<PagedResult<DoctorSchedule>>('DoctorSchedules', params);
  }

  getById(id: number): Observable<DoctorSchedule> {
    return this.get<DoctorSchedule>(`DoctorSchedules/${id}`);
  }

  getWeeklySchedule(doctorId: number, weekStartDate: string): Observable<WeeklySchedule> {
    return this.get<WeeklySchedule>(`DoctorSchedules/weekly`, {
      doctorId,
      weekStartDate
    });
  }

  create(schedule: CreateDoctorScheduleRequest): Observable<DoctorSchedule> {
    return this.post<DoctorSchedule>('DoctorSchedules', schedule);
  }

  update(schedule: UpdateDoctorScheduleRequest): Observable<DoctorSchedule> {
    return this.put<DoctorSchedule>(`DoctorSchedules/${schedule.id}`, schedule);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`DoctorSchedules/${id}`);
  }

  generateSchedule(doctorId: number, startDate: string, endDate: string, shiftIds: number[]): Observable<DoctorSchedule[]> {
    return this.post<DoctorSchedule[]>('DoctorSchedules/generate', {
      doctorId,
      startDate,
      endDate,
      shiftIds
    });
  }
}
