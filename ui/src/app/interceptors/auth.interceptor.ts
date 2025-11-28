import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');

    let authReq = req;
    if (token) {
      authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // برای endpoint‌های عمومی که AllowAnonymous هستند، نباید به login هدایت شود
          const isPublicEndpoint = authReq.url.includes('/public') ||
                                   authReq.url.includes('/chat/') ||
                                   authReq.url.includes('/auth/register') ||
                                   authReq.url.includes('/auth/login');

          if (!isPublicEndpoint) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            this.router.navigate(['/login']);
          }
        }
        return throwError(() => error);
      })
    );
  }
}
