import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Holiday, CreateHolidayRequest, UpdateHolidayRequest } from '../models/holiday.model';
import { PagedResult } from '../models/paged-result.model';

export interface HolidayQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class HolidayService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAll(params?: HolidayQueryParams): Observable<PagedResult<Holiday>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key as keyof HolidayQueryParams] !== null && params[key as keyof HolidayQueryParams] !== undefined) {
          httpParams = httpParams.append(key, params[key as keyof HolidayQueryParams]!.toString());
        }
      });
    }
    return this.http.get<any>(`${this.baseUrl}/Holidays`, { params: httpParams }).pipe(
      map(response => {
        console.log('Holiday API response:', response);

        // تبدیل PagedResult از PascalCase به camelCase
        let items: Holiday[] = [];
        let totalCount = 0;
        let page = 1;
        let pageSize = 10;
        let totalPages = 0;

        if (response) {
          // اگر پاسخ به صورت PascalCase است
          if ('Items' in response && Array.isArray(response.Items)) {
            items = response.Items || [];
            totalCount = response.TotalCount || 0;
            page = response.Page || 1;
            pageSize = response.PageSize || 10;
            totalPages = response.TotalPages || 0;
          }
          // اگر پاسخ به صورت camelCase است
          else if ('items' in response && Array.isArray(response.items)) {
            items = response.items || [];
            totalCount = response.totalCount || 0;
            page = response.page || 1;
            pageSize = response.pageSize || 10;
            totalPages = response.totalPages || 0;
          }
          // اگر مستقیم آرایه است
          else if (Array.isArray(response)) {
            items = response;
            totalCount = response.length;
          }
        }

        console.log('Processed holidays:', items);

        return {
          items: items,
          totalCount: totalCount,
          page: page,
          pageSize: pageSize,
          totalPages: totalPages
        };
      }),
      catchError(error => {
        console.error('Error loading holidays:', error);
        const errorMessage = error.error?.message || error.message || 'خطا در دریافت تعطیلات';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  getById(id: number): Observable<Holiday> {
    return this.http.get<Holiday>(`${this.baseUrl}/Holidays/${id}`).pipe(
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در دریافت تعطیل';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  create(holiday: CreateHolidayRequest): Observable<Holiday> {
    console.log('Creating holiday with data:', holiday);
    return this.http.post<Holiday>(`${this.baseUrl}/Holidays`, holiday, { observe: 'response' }).pipe(
      map(response => {
        console.log('Holiday created successfully, status:', response.status);
        console.log('Holiday created response body:', response.body);

        // CreatedAtAction returns the object in the response body
        if (response.body) {
          return response.body;
        }

        // اگر body خالی است، از location header استفاده کن
        const location = response.headers.get('Location');
        if (location) {
          const idMatch = location.match(/\/(\d+)$/);
          if (idMatch) {
            return { ...holiday, id: parseInt(idMatch[1]) } as Holiday;
          }
        }

        return holiday as Holiday;
      }),
      catchError(error => {
        console.error('Error creating holiday:', error);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          error: error.error,
          message: error.message
        });
        const errorMessage = error.error?.message || error.error || error.message || 'خطا در ایجاد تعطیل';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  update(holiday: UpdateHolidayRequest): Observable<Holiday> {
    return this.http.put<Holiday>(`${this.baseUrl}/Holidays/${holiday.id}`, holiday).pipe(
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در به‌روزرسانی تعطیل';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Holidays/${id}`).pipe(
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در حذف تعطیل';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }
}
