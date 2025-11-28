import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AppSettings {
  fontFamily: string;
  primaryColor: string;
  secondaryColor: string;
  fontSize: string;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly STORAGE_KEY = 'app_settings';
  private defaultSettings: AppSettings = {
    fontFamily: 'Tahoma',
    primaryColor: '#06b6d4',
    secondaryColor: '#10b981',
    fontSize: 'small'
  };

  private settingsSubject = new BehaviorSubject<AppSettings>(this.loadSettings());
  public settings$: Observable<AppSettings> = this.settingsSubject.asObservable();

  constructor() {
    this.applySettings(this.settingsSubject.value);
  }

  private loadSettings(): AppSettings {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        return { ...this.defaultSettings, ...JSON.parse(stored) };
      }
    } catch (error) {
      console.error('Error loading settings:', error);
    }
    return { ...this.defaultSettings };
  }

  getSettings(): AppSettings {
    return this.settingsSubject.value;
  }

  updateSettings(settings: Partial<AppSettings>): void {
    const newSettings = { ...this.settingsSubject.value, ...settings };
    this.settingsSubject.next(newSettings);
    this.saveSettings(newSettings);
    this.applySettings(newSettings);
  }

  private saveSettings(settings: AppSettings): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(settings));
    } catch (error) {
      console.error('Error saving settings:', error);
    }
  }

  private applySettings(settings: AppSettings): void {
    const root = document.documentElement;
    
    // Apply font family using CSS variable
    root.style.setProperty('--app-font-family', settings.fontFamily);
    document.body.style.fontFamily = `var(--app-font-family, ${settings.fontFamily})`;
    
    // Apply font to sidebar and header specifically
    const sidebar = document.querySelector('.sidebar');
    const header = document.querySelector('.header-toolbar');
    const sidebarNav = document.querySelector('.sidebar-nav');
    
    if (sidebar) {
      (sidebar as HTMLElement).style.fontFamily = `var(--app-font-family, ${settings.fontFamily})`;
    }
    if (header) {
      (header as HTMLElement).style.fontFamily = `var(--app-font-family, ${settings.fontFamily})`;
    }
    if (sidebarNav) {
      (sidebarNav as HTMLElement).style.fontFamily = `var(--app-font-family, ${settings.fontFamily})`;
    }
    
    // Apply colors
    root.style.setProperty('--primary', settings.primaryColor);
    root.style.setProperty('--primary-dark', this.darkenColor(settings.primaryColor, 10));
    root.style.setProperty('--primary-light', this.lightenColor(settings.primaryColor, 10));
    root.style.setProperty('--secondary', settings.secondaryColor);
    
    // Apply font size
    const fontSizeMap: { [key: string]: string } = {
      small: '14px',
      medium: '16px',
      large: '18px'
    };
    const fontSize = fontSizeMap[settings.fontSize] || fontSizeMap['medium'];
    root.style.setProperty('--app-font-size', fontSize);
    document.body.style.fontSize = fontSize;
    
    // Calculate scaled font sizes based on base font size
    const baseSize = parseInt(fontSize);
    root.style.setProperty('--app-font-size-xs', `${baseSize * 0.75}px`); // 0.75x
    root.style.setProperty('--app-font-size-sm', `${baseSize * 0.875}px`); // 0.875x
    root.style.setProperty('--app-font-size-base', fontSize); // 1x
    root.style.setProperty('--app-font-size-lg', `${baseSize * 1.125}px`); // 1.125x
    root.style.setProperty('--app-font-size-xl', `${baseSize * 1.25}px`); // 1.25x
    root.style.setProperty('--app-font-size-2xl', `${baseSize * 1.5}px`); // 1.5x
    root.style.setProperty('--app-font-size-3xl', `${baseSize * 1.875}px`); // 1.875x
    root.style.setProperty('--app-font-size-4xl', `${baseSize * 2.25}px`); // 2.25x
    root.style.setProperty('--app-font-size-5xl', `${baseSize * 3}px`); // 3x
  }

  resetSettings(): void {
    this.updateSettings(this.defaultSettings);
  }

  private darkenColor(color: string, percent: number): string {
    const num = parseInt(color.replace('#', ''), 16);
    const amt = Math.round(2.55 * percent);
    const R = Math.max(0, Math.min(255, (num >> 16) + -amt));
    const G = Math.max(0, Math.min(255, ((num >> 8) & 0x00FF) + -amt));
    const B = Math.max(0, Math.min(255, (num & 0x0000FF) + -amt));
    return '#' + (0x1000000 + R * 0x10000 + G * 0x100 + B).toString(16).slice(1);
  }

  private lightenColor(color: string, percent: number): string {
    const num = parseInt(color.replace('#', ''), 16);
    const amt = Math.round(2.55 * percent);
    const R = Math.max(0, Math.min(255, (num >> 16) + amt));
    const G = Math.max(0, Math.min(255, ((num >> 8) & 0x00FF) + amt));
    const B = Math.max(0, Math.min(255, (num & 0x0000FF) + amt));
    return '#' + (0x1000000 + R * 0x10000 + G * 0x100 + B).toString(16).slice(1);
  }

  getAvailableFonts(): string[] {
    return [
      'Vazirmatn',
      'Tahoma',
      'Arial',
      'Segoe UI',
      'Roboto',
      'Iranian Sans',
      'Samim',
      'Shabnam'
    ];
  }

  getAvailableColors(): { name: string; value: string }[] {
    return [
      { name: 'آبی فیروزه‌ای', value: '#06b6d4' },
      { name: 'سبز', value: '#10b981' },
      { name: 'آبی', value: '#3b82f6' },
      { name: 'بنفش', value: '#8b5cf6' },
      { name: 'صورتی', value: '#ec4899' },
      { name: 'قرمز', value: '#ef4444' },
      { name: 'نارنجی', value: '#f59e0b' },
      { name: 'زرد', value: '#eab308' }
    ];
  }
}
