import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { BaseApiService } from './base-api.service';
import { LoginRequest, RegisterRequest, AuthResponse, ApiResponse, User } from '../models/auth.model';
import { UpdateProfileRequest, ChangePasswordRequest } from '../models/user.model';
import { Clinic } from '../models/clinic.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService extends BaseApiService {
  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.post<AuthResponse>('auth/login', credentials).pipe(
      map(response => {
        if (response && response.token && response.user) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
          return response;
        } else {
          throw new Error('خطا در ورود');
        }
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در ورود. لطفا دوباره تلاش کنید.';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.post<AuthResponse>('auth/register', data).pipe(
      map(response => {
        if (response && response.token && response.user) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
          return response;
        } else {
          throw new Error('خطا در ثبت نام');
        }
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در ثبت نام. لطفا دوباره تلاش کنید.';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getCurrentUserFromApi(): Observable<User> {
    return this.get<User>('auth/me').pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          return user;
        } else {
          throw new Error('خطا در دریافت اطلاعات کاربر');
        }
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در دریافت اطلاعات کاربر';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }

  updateProfile(updateData: UpdateProfileRequest): Observable<User> {
    return this.put<User>('auth/me', updateData).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          return user;
        } else {
          throw new Error('خطا در به‌روزرسانی پروفایل');
        }
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در به‌روزرسانی پروفایل';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }

  changePassword(changePasswordData: ChangePasswordRequest): Observable<any> {
    // API فقط currentPassword و newPassword را می‌خواهد
    const apiData = {
      currentPassword: changePasswordData.currentPassword,
      newPassword: changePasswordData.newPassword
    };
    return this.post<any>('auth/change-password', apiData).pipe(
      map(response => {
        return response;
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در تغییر رمز عبور';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }

  getCurrentUserClinics(): Observable<Clinic[]> {
    return this.get<Clinic[]>('auth/me/clinics').pipe(
      map(response => {
        return response || [];
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در دریافت لیست کلینیک‌ها';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }

  uploadProfilePicture(file: File): Observable<User> {
    const formData = new FormData();
    formData.append('file', file);
    return this.postWithFile<User>('auth/me/profile-picture', formData).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          return user;
        } else {
          throw new Error('خطا در آپلود عکس پروفایل');
        }
      }),
      catchError(error => {
        const errorMessage = error.error?.message || error.message || 'خطا در آپلود عکس پروفایل';
        return throwError(() => ({ error: { message: errorMessage } }));
      })
    );
  }
}
