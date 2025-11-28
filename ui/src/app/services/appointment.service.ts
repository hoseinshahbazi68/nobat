import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Appointment, CreateAppointmentRequest } from '../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * تولید خودکار نوبت‌ها برای بازه زمانی مشخص
   * @param startDate تاریخ شروع (اختیاری - پیش‌فرض: امروز)
   * @param endDate تاریخ پایان (اختیاری - پیش‌فرض: 30 روز آینده)
   */
  generateAppointments(startDate?: string, endDate?: string): Observable<ApiResponse<number>> {
    let httpParams = new HttpParams();

    if (startDate) {
      httpParams = httpParams.append('startDate', startDate);
    }
    if (endDate) {
      httpParams = httpParams.append('endDate', endDate);
    }

    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/Appointments/generate`, {}, { params: httpParams }).pipe(
      map(response => {
        // تبدیل از PascalCase به camelCase
        return {
          status: response.status || (response as any).Status || false,
          statusCode: response.statusCode || (response as any).StatusCode || 200,
          message: response.message || (response as any).Message || '',
          data: response.data || (response as any).Data
        } as ApiResponse<number>;
      })
    );
  }

  /**
   * ایجاد نوبت جدید
   * @param appointment اطلاعات نوبت
   */
  create(appointment: CreateAppointmentRequest): Observable<ApiResponse<Appointment>> {
    return this.http.post<ApiResponse<Appointment>>(`${this.baseUrl}/Appointments`, appointment).pipe(
      map(response => {
        return {
          status: response.status || (response as any).Status || false,
          statusCode: response.statusCode || (response as any).StatusCode || 200,
          message: response.message || (response as any).Message || '',
          data: response.data || (response as any).Data
        } as ApiResponse<Appointment>;
      })
    );
  }

  /**
   * دریافت تعداد نوبت‌ها برای یک بازه تاریخ و پزشک مشخص
   * @param doctorId شناسه پزشک
   * @param startDate تاریخ شروع (yyyy-MM-dd)
   * @param endDate تاریخ پایان (yyyy-MM-dd)
   */
  getAppointmentCounts(doctorId: number, startDate: string, endDate: string): Observable<ApiResponse<AppointmentCountDto[]>> {
    let httpParams = new HttpParams();
    httpParams = httpParams.append('doctorId', doctorId.toString());
    httpParams = httpParams.append('startDate', startDate);
    httpParams = httpParams.append('endDate', endDate);

    return this.http.get<ApiResponse<AppointmentCountDto[]>>(`${this.baseUrl}/Appointments/counts`, { params: httpParams }).pipe(
      map(response => {
        return {
          status: response.status || (response as any).Status || false,
          statusCode: response.statusCode || (response as any).StatusCode || 200,
          message: response.message || (response as any).Message || '',
          data: response.data || (response as any).Data
        } as ApiResponse<AppointmentCountDto[]>;
      })
    );
  }
}

export interface AppointmentCountDto {
  date: string;
  totalCount: number;
  bookedCount: number;
  availableCount: number;
}
