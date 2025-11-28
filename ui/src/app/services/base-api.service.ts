import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class BaseApiService {
  protected baseUrl = environment.apiUrl;

  constructor(protected http: HttpClient) { }

  protected get<T>(endpoint: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.append(key, params[key].toString());
        }
      });
    }
    return this.http.get<any>(`${this.baseUrl}/${endpoint}`, { params: httpParams }).pipe(
      map(response => {
        // بررسی اینکه آیا response به صورت مستقیم PagedResult است (بدون ApiResponse wrapper)
        if (this.isPagedResult(response)) {
          return this.mapPagedResult(response) as T;
        }

        // بررسی اینکه آیا response در قالب ApiResponse است
        if (response && typeof response === 'object' && 'status' in response) {
          if (!response.status) {
            throw new Error(response.message || 'خطا در دریافت داده');
          }
          // تبدیل PagedResult از Items به items
          if (response.data && this.isPagedResult(response.data)) {
            return this.mapPagedResult(response.data) as T;
          }
          // اگر data یک array است، آن را مستقیماً تبدیل می‌کنیم
          if (Array.isArray(response.data)) {
            return response.data.map((item: any) => this.convertToCamelCase(item)) as T;
          }
          // تبدیل فیلدها از PascalCase به camelCase برای داده‌های غیر PagedResult
          return this.convertToCamelCase(response.data) as T;
        }

        // اگر response به صورت مستقیم داده است (نه ApiResponse)
        return this.convertToCamelCase(response) as T;
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  protected post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body).pipe(
      map(response => {
        console.log('API Response:', response); // Debug log
        if (!response || !response.status) {
          const errorMessage = response?.message || 'خطا در ارسال داده';
          console.error('API Error:', errorMessage, response); // Debug log
          throw { error: { message: errorMessage }, status: response?.statusCode || 400 };
        }
        // برای void responses (مثل assignClinicToUser) که data ممکن است null باشد
        if (response.data === null || response.data === undefined) {
          // اگر status موفق است و data null است، این برای void responses است
          return undefined as T;
        }
        // تبدیل فیلدها از PascalCase به camelCase
        const convertedData = this.convertToCamelCase(response.data) as T;
        console.log('Converted data:', convertedData); // Debug log
        return convertedData;
      }),
      catchError(error => {
        // اگر error از map آمده باشد
        if (error.error && error.error.message) {
          return throwError(() => error);
        }
        // اگر error از HTTP آمده باشد
        const errorMessage = error.error?.message || error.message || 'خطا در ارسال داده';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  protected put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body).pipe(
      map(response => {
        if (!response || !response.status) {
          const errorMessage = response?.message || 'خطا در به‌روزرسانی داده';
          throw { error: { message: errorMessage }, status: response?.statusCode || 400 };
        }
        if (response.data === null || response.data === undefined) {
          throw { error: { message: response.message || 'داده‌ای برگشت داده نشد' }, status: response.statusCode || 400 };
        }
        // تبدیل فیلدها از PascalCase به camelCase
        return this.convertToCamelCase(response.data) as T;
      }),
      catchError(error => {
        // اگر error از map آمده باشد
        if (error.error && error.error.message) {
          return throwError(() => error);
        }
        // اگر error از HTTP آمده باشد
        const errorMessage = error.error?.message || error.message || 'خطا در به‌روزرسانی داده';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  protected deleteRequest<T>(endpoint: string): Observable<T> {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`).pipe(
      map(response => {
        if (!response.status) {
          throw new Error(response.message || 'خطا در حذف داده');
        }
        // برای delete ممکن است data وجود نداشته باشد
        return (response.data ?? undefined) as T;
      }),
      catchError(error => {
        return throwError(() => error);
      })
    );
  }

  protected postWithFile<T>(endpoint: string, formData: FormData): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, formData).pipe(
      map(response => {
        if (!response || !response.status) {
          const errorMessage = response?.message || 'خطا در ارسال داده';
          throw { error: { message: errorMessage }, status: response?.statusCode || 400 };
        }
        if (response.data === null || response.data === undefined) {
          return undefined as T;
        }
        return this.convertToCamelCase(response.data) as T;
      }),
      catchError(error => {
        if (error.error && error.error.message) {
          return throwError(() => error);
        }
        const errorMessage = error.error?.message || error.message || 'خطا در ارسال داده';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  protected putWithFile<T>(endpoint: string, formData: FormData): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, formData).pipe(
      map(response => {
        if (!response || !response.status) {
          const errorMessage = response?.message || 'خطا در به‌روزرسانی داده';
          throw { error: { message: errorMessage }, status: response?.statusCode || 400 };
        }
        if (response.data === null || response.data === undefined) {
          throw { error: { message: response.message || 'داده‌ای برگشت داده نشد' }, status: response.statusCode || 400 };
        }
        return this.convertToCamelCase(response.data) as T;
      }),
      catchError(error => {
        if (error.error && error.error.message) {
          return throwError(() => error);
        }
        const errorMessage = error.error?.message || error.message || 'خطا در به‌روزرسانی داده';
        return throwError(() => ({ error: { message: errorMessage }, status: error.status || 500 }));
      })
    );
  }

  private isPagedResult(data: any): boolean {
    if (!data || typeof data !== 'object') {
      return false;
    }
    // بررسی هم برای PascalCase (Items) و هم برای camelCase (items)
    return (('Items' in data && Array.isArray(data.Items)) ||
      ('items' in data && Array.isArray(data.items)));
  }

  private mapPagedResult(data: any): PagedResult<any> {
    // پشتیبانی از هر دو حالت PascalCase و camelCase
    const itemsArray = data.Items || data.items || [];
    const items = itemsArray.map((item: any) => this.convertToCamelCase(item));

    return {
      items: items,
      totalCount: data.TotalCount || data.totalCount || 0,
      page: data.Page || data.page || 1,
      pageSize: data.PageSize || data.pageSize || 10,
      totalPages: data.TotalPages || data.totalPages || 0
    };
  }

  /**
   * تبدیل فیلدهای یک شیء از PascalCase به camelCase
   */
  private convertToCamelCase(obj: any): any {
    if (obj === null || obj === undefined) {
      return obj;
    }

    if (Array.isArray(obj)) {
      return obj.map(item => this.convertToCamelCase(item));
    }

    if (typeof obj !== 'object') {
      return obj;
    }

    const converted: any = {};
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        const camelKey = this.toCamelCase(key);
        const value = obj[key];

        if (value !== null && typeof value === 'object' && !Array.isArray(value)) {
          converted[camelKey] = this.convertToCamelCase(value);
        } else if (Array.isArray(value)) {
          converted[camelKey] = value.map(item => this.convertToCamelCase(item));
        } else {
          converted[camelKey] = value;
        }
      }
    }
    return converted;
  }

  /**
   * تبدیل یک رشته از PascalCase به camelCase
   */
  private toCamelCase(str: string): string {
    if (!str) return str;
    if (str.length === 0) return str;
    if (str.length === 1) return str.toLowerCase();
    return str.charAt(0).toLowerCase() + str.slice(1);
  }
}
