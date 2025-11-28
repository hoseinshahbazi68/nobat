import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PersianDateService {

  constructor() {}

  /**
   * تبدیل تاریخ میلادی به شمسی
   * این تابع تاریخ را به درستی تبدیل می‌کند و مشکلات timezone را حل می‌کند
   * برای تاریخ‌هایی که از سرور می‌آیند (معمولاً UTC)، از UTC استفاده می‌کند
   */
  toPersian(date: Date | string | null): string {
    if (!date) return '';

    let d: Date;
    let useUTC = false;

    if (typeof date === 'string') {
      const dateStr = date.trim();

      // اگر تاریخ به صورت YYYY-MM-DD است (بدون زمان)
      // این فرمت معمولاً از سرور می‌آید و باید به صورت UTC تفسیر شود
      if (/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
        // تاریخ را به صورت UTC تفسیر می‌کنیم
        d = new Date(dateStr + 'T00:00:00Z');
        useUTC = true;
      }
      // اگر تاریخ به صورت ISO string است اما Z ندارد
      else if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{3})?$/.test(dateStr)) {
        // تاریخ را به صورت UTC تفسیر می‌کنیم
        d = new Date(dateStr + 'Z');
        useUTC = true;
      }
      // اگر تاریخ شامل Z است (UTC)
      else if (dateStr.includes('Z') || dateStr.includes('+') || dateStr.match(/-\d{2}:\d{2}$/)) {
        d = new Date(dateStr);
        useUTC = true;
      }
      else {
        d = new Date(dateStr);
        useUTC = false;
      }
    } else {
      // برای Date objects، از تاریخ محلی استفاده می‌کنیم
      d = new Date(date);
      useUTC = false;
    }

    if (isNaN(d.getTime())) return '';

    // استفاده از UTC یا محلی بسته به نوع تاریخ
    const gy = useUTC ? d.getUTCFullYear() : d.getFullYear();
    const gm = useUTC ? d.getUTCMonth() + 1 : d.getMonth() + 1;
    const gd = useUTC ? d.getUTCDate() : d.getDate();

    const jy = this.gregorianToJalali(gy, gm, gd);
    return `${jy[0]}/${String(jy[1]).padStart(2, '0')}/${String(jy[2]).padStart(2, '0')}`;
  }

  /**
   * تبدیل تاریخ شمسی به میلادی
   */
  toGregorian(persianDate: string): Date;
  toGregorian(year: number, month: number, day: number): Date;
  toGregorian(persianDateOrYear: string | number, month?: number, day?: number): Date {
    if (typeof persianDateOrYear === 'string') {
      if (!persianDateOrYear) return new Date();
      const parts = persianDateOrYear.split('/').map(p => parseInt(p.trim()));
    if (parts.length !== 3 || parts.some(p => isNaN(p))) {
      return new Date();
    }
    const [jy, jm, jd] = parts;
    const g = this.jalaliToGregorian(jy, jm, jd);
    return new Date(g[0], g[1] - 1, g[2]);
    } else {
      const g = this.jalaliToGregorian(persianDateOrYear, month!, day!);
      return new Date(g[0], g[1] - 1, g[2]);
    }
  }

  /**
   * تبدیل Date به object شمسی
   */
  toPersianDate(date: Date): { year: number; month: number; day: number } {
    const persian = this.toPersian(date);
    const parts = persian.split('/').map(p => parseInt(p));
    return {
      year: parts[0],
      month: parts[1],
      day: parts[2]
    };
  }

  /**
   * دریافت سال جاری شمسی
   */
  getCurrentPersianYear(): number {
    return this.toPersianDate(new Date()).year;
  }

  /**
   * تبدیل تاریخ میلادی به شمسی با فرمت کامل
   */
  toPersianFull(date: Date | string | null): string {
    if (!date) return '';
    const persian = this.toPersian(date);
    const d = new Date(date);
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${persian} - ${hours}:${minutes}`;
  }

  /**
   * دریافت تاریخ امروز به شمسی
   */
  getTodayPersian(): string {
    return this.toPersian(new Date());
  }

  /**
   * تبدیل رشته شمسی به Date object
   */
  parsePersianDate(persianDate: string): Date {
    if (!persianDate) return new Date();
    return this.toGregorian(persianDate);
  }

  /**
   * تبدیل تاریخ میلادی به شمسی با فرمت تاریخ و زمان کامل
   */
  formatDateTime(dateString: string | null): string {
    if (!dateString) return '';
    return this.toPersianFull(dateString);
  }

  /**
   * تبدیل میلادی به شمسی
   */
  private gregorianToJalali(gy: number, gm: number, gd: number): number[] {
    const g_d_m = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
    let jy = (gy <= 1600) ? 0 : 979;
    let gy2 = (gy <= 1600) ? gy + 1600 : gy;
    let days = (365 * gy2) + (Math.floor((gy2 + 3) / 4)) - (Math.floor((gy2 + 99) / 100)) +
               (Math.floor((gy2 + 399) / 400)) - 80 + gd + g_d_m[gm - 1];
    jy += 33 * Math.floor(days / 12053);
    days %= 12053;
    jy += 4 * Math.floor(days / 1461);
    days %= 1461;
    if (days > 365) {
      jy += Math.floor((days - 1) / 365);
      days = (days - 1) % 365;
    }
    let jm = (days < 186) ? 1 + Math.floor(days / 31) : 7 + Math.floor((days - 186) / 30);
    let jd = 1 + ((days < 186) ? (days % 31) : ((days - 186) % 30));
    return [jy, jm, jd];
  }

  /**
   * تبدیل شمسی به میلادی
   */
  private jalaliToGregorian(jy: number, jm: number, jd: number): number[] {
    jy += 1595;
    let days = -355668 + (365 * jy) + (Math.floor(jy / 33) * 8) +
               Math.floor(((jy % 33) + 3) / 4) + jd +
               ((jm < 7) ? (jm - 1) * 31 : ((jm - 7) * 30) + 186);
    let gy = 400 * Math.floor(days / 146097);
    days %= 146097;
    if (days > 36524) {
      gy += 100 * Math.floor(--days / 36524);
      days %= 36524;
      if (days >= 365) days++;
    }
    gy += 4 * Math.floor(days / 1461);
    days %= 1461;
    if (days > 365) {
      gy += Math.floor((days - 1) / 365);
      days = (days - 1) % 365;
    }
    let gd = days + 1;
    const sal_a = [0, 31, ((gy % 4 === 0 && gy % 100 !== 0) || (gy % 400 === 0)) ? 29 : 28,
                   31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    let gm = 0;
    while (gm < 13 && gd > sal_a[gm]) {
      gd -= sal_a[gm++];
    }
    return [gy, gm, gd];
  }
}
