import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  show(message: string, action: string = 'بستن', duration: number = 3000, type: 'success' | 'error' | 'info' = 'info') {
    const snackbar = document.createElement('div');
    snackbar.className = 'snackbar';

    // تعیین رنگ پس‌زمینه بر اساس نوع
    let backgroundColor = '#0f172a'; // پیش‌فرض
    if (type === 'error') {
      backgroundColor = '#dc2626'; // قرمز
    } else if (type === 'success') {
      backgroundColor = '#16a34a'; // سبز
    }

    snackbar.style.cssText = `
      position: fixed;
      bottom: 20px;
      left: 50%;
      transform: translateX(-50%) translateY(100px);
      background: ${backgroundColor};
      color: white;
      padding: 16px 24px;
      border-radius: 12px;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
      z-index: 99999;
      display: flex;
      align-items: center;
      gap: 16px;
      min-width: 300px;
      max-width: 90vw;
      animation: slideUpSnackbar 0.3s cubic-bezier(0.4, 0, 0.2, 1) forwards;
      font-family: 'Vazirmatn', sans-serif;
    `;

    const messageSpan = document.createElement('span');
    messageSpan.textContent = message;
    messageSpan.style.flex = '1';
    snackbar.appendChild(messageSpan);

    if (action) {
      const actionButton = document.createElement('button');
      actionButton.textContent = action;
      actionButton.style.cssText = `
        background: rgba(255, 255, 255, 0.1);
        border: none;
        color: white;
        padding: 8px 16px;
        border-radius: 8px;
        cursor: pointer;
        font-weight: 600;
        transition: all 0.2s;
        font-family: 'Vazirmatn', sans-serif;
      `;
      actionButton.addEventListener('mouseenter', () => {
        actionButton.style.background = 'rgba(255, 255, 255, 0.2)';
      });
      actionButton.addEventListener('mouseleave', () => {
        actionButton.style.background = 'rgba(255, 255, 255, 0.1)';
      });
      actionButton.addEventListener('click', () => {
        this.removeSnackbar(snackbar);
      });
      snackbar.appendChild(actionButton);
    }

    document.body.appendChild(snackbar);

    // Add animation
    const style = document.createElement('style');
    if (!document.getElementById('snackbar-animations')) {
      style.id = 'snackbar-animations';
      style.textContent = `
        @keyframes slideUpSnackbar {
          to {
            transform: translateX(-50%) translateY(0);
          }
        }
        @keyframes slideDownSnackbar {
          from {
            transform: translateX(-50%) translateY(0);
          }
          to {
            transform: translateX(-50%) translateY(100px);
            opacity: 0;
          }
        }
      `;
      document.head.appendChild(style);
    }

    setTimeout(() => {
      this.removeSnackbar(snackbar);
    }, duration);
  }

  private removeSnackbar(snackbar: HTMLElement) {
    snackbar.style.animation = 'slideDownSnackbar 0.3s cubic-bezier(0.4, 0, 0.2, 1) forwards';
    setTimeout(() => {
      snackbar.remove();
    }, 300);
  }

  success(message: string, action: string = 'بستن', duration: number = 3000) {
    this.show(message, action, duration, 'success');
  }

  error(message: string, action: string = 'بستن', duration: number = 3000) {
    this.show(message, action, duration, 'error');
  }

  info(message: string, action: string = 'بستن', duration: number = 3000) {
    this.show(message, action, duration, 'info');
  }
}
