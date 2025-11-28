import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AppointmentService } from '../../services/appointment.service';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  stats = [
    { title: 'تعداد پزشکان', value: '0', icon: 'local_hospital', color: '#667eea', route: '/panel/doctors' },
    { title: 'تعداد خدمات', value: '0', icon: 'medical_services', color: '#764ba2', route: '/panel/services' },
    { title: 'نوبت‌های امروز', value: '0', icon: 'event', color: '#f093fb', route: '/panel/doctor-schedules' },
    { title: 'بیمه‌ها', value: '0', icon: 'health_and_safety', color: '#4facfe', route: '/panel/insurances' },
    { title: 'لاگ فعالیت‌ها', value: '📋', icon: 'history', color: '#10b981', route: '/panel/user-activity-logs' }
  ];

  isGenerating = false;

  constructor(
    private router: Router,
    private appointmentService: AppointmentService,
    private snackbarService: SnackbarService
  ) {}

  getGradient(color: string): string {
    const gradients: { [key: string]: string } = {
      '#667eea': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      '#764ba2': 'linear-gradient(135deg, #764ba2 0%, #f093fb 100%)',
      '#f093fb': 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      '#4facfe': 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      '#10b981': 'linear-gradient(135deg, #10b981 0%, #059669 100%)'
    };
    return gradients[color] || `linear-gradient(135deg, ${color} 0%, ${color}dd 100%)`;
  }

  getGradientColor(color: string): string {
    const colors: { [key: string]: string } = {
      '#667eea': '#764ba2',
      '#764ba2': '#f093fb',
      '#f093fb': '#f5576c',
      '#4facfe': '#00f2fe',
      '#10b981': '#059669'
    };
    return colors[color] || color;
  }

  getIcon(iconName: string): string {
    const iconMap: { [key: string]: string } = {
      'local_hospital': '🏥',
      'medical_services': '⚕️',
      'event': '📅',
      'health_and_safety': '🛡️',
      'history': '📋'
    };
    return iconMap[iconName] || '📊';
  }

  navigateTo(route: string) {
    if (route) {
      this.router.navigate([route]);
    }
  }

  generateAppointments() {
    if (this.isGenerating) {
      return;
    }

    this.isGenerating = true;
    this.snackbarService.info('در حال تولید نوبت‌ها...', 'بستن', 2000);

    // تولید نوبت‌ها برای 30 روز آینده
    const today = new Date();
    const endDate = new Date();
    endDate.setDate(today.getDate() + 30);

    const startDateStr = this.formatDate(today);
    const endDateStr = this.formatDate(endDate);

    this.appointmentService.generateAppointments(startDateStr, endDateStr).subscribe({
      next: (response) => {
        this.isGenerating = false;
        if (response && response.status) {
          const count = response.data || 0;
          this.snackbarService.success(
            `تعداد ${count} نوبت با موفقیت تولید شد`,
            'بستن',
            5000
          );
        } else {
          this.snackbarService.error(
            (response as any)?.message || 'خطا در تولید نوبت‌ها',
            'بستن',
            5000
          );
        }
      },
      error: (error) => {
        this.isGenerating = false;
        const errorMessage = error.error?.message || error.message || 'خطا در تولید نوبت‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
      }
    });
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
