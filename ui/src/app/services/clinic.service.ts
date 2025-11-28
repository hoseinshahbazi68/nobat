import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { BaseApiService } from './base-api.service';
import { Clinic, ClinicSimple, CreateClinicRequest, UpdateClinicRequest } from '../models/clinic.model';
import { PagedResult } from '../models/paged-result.model';

export interface ClinicQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ClinicService extends BaseApiService {
  private selectedClinicSubject = new BehaviorSubject<Clinic | null>(null);
  public selectedClinic$ = this.selectedClinicSubject.asObservable();

  constructor(http: HttpClient) {
    super(http);
    // بارگذاری کلینیک انتخاب شده از کوکی در صورت وجود
    this.loadSelectedClinicFromCookie();
  }

  getAll(params?: ClinicQueryParams): Observable<PagedResult<Clinic>> {
    return this.get<PagedResult<Clinic>>('clinics', params);
  }

  getById(id: number): Observable<Clinic> {
    return this.get<Clinic>(`clinics/${id}`);
  }

  create(clinic: CreateClinicRequest): Observable<Clinic> {
    return this.post<Clinic>('clinics', clinic);
  }

  update(clinic: UpdateClinicRequest): Observable<Clinic> {
    return this.put<Clinic>(`clinics/${clinic.id}`, clinic);
  }

  delete(id: number): Observable<void> {
    return this.deleteRequest<void>(`clinics/${id}`);
  }

  getSimpleList(searchTerm?: string): Observable<ClinicSimple[]> {
    const params = searchTerm ? { searchTerm } : undefined;
    return this.get<ClinicSimple[]>('clinics/simple', params);
  }

  setSelectedClinic(clinic: Clinic | null): void {
    this.selectedClinicSubject.next(clinic);
    if (clinic?.id) {
      this.setCookie('selectedClinicId', clinic.id.toString(), 365);
    } else {
      this.deleteCookie('selectedClinicId');
    }
  }

  getSelectedClinic(): Clinic | null {
    return this.selectedClinicSubject.value;
  }

  private loadSelectedClinicFromCookie(): void {
    const clinicId = this.getCookie('selectedClinicId');
    if (clinicId) {
      // کلینیک از کوکی بارگذاری می‌شود اما باید از API دریافت شود
      // این کار در sidebar component انجام می‌شود
    }
  }

  private getCookie(name: string): string | null {
    const nameEQ = name + '=';
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
      let c = ca[i];
      while (c.charAt(0) === ' ') c = c.substring(1, c.length);
      if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
  }

  private setCookie(name: string, value: string, days: number): void {
    let expires = '';
    if (days) {
      const date = new Date();
      date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
      expires = '; expires=' + date.toUTCString();
    }
    document.cookie = name + '=' + (value || '') + expires + '; path=/';
  }

  private deleteCookie(name: string): void {
    document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  }
}
