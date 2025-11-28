import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { interval, Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { User } from '../../../models/user.model';
import { DialogService } from '../../../services/dialog.service';
import { ChangePasswordDialogComponent } from '../../change-password-dialog/change-password-dialog.component';
import { ChatService } from '../../../services/chat.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  @Input() isMobile: boolean = false;
  @Output() toggleSidenav = new EventEmitter<void>();

  username: string = 'کاربر';
  currentUser: User | null = null;
  currentRoute: string = '';
  userMenuOpen: boolean = false;
  unansweredChatCount: number = 0;
  profilePictureUrl: string | null = null;
  private chatCountSubscription?: Subscription;

  private pageTitles: { [key: string]: string } = {
    '/panel/dashboard': 'داشبورد',
    '/panel/user-roles': 'نقش کاربران',
    '/panel/users': 'کاربران',
    '/panel/holidays': 'روزهای تعطیل',
    '/panel/shifts': 'شیفت',
    '/panel/services': 'خدمت',
    '/panel/doctors': 'پزشکان',
    '/panel/specialties': 'تخصص',
    '/panel/weekly-schedule': 'برنامه هفتگی',
    '/panel/insurances': 'بیمه',
    '/panel/service-tariffs': 'تعرفه خدمات',
    '/panel/doctor-schedules': 'برنامه حضور پزشکان',
    '/panel/generate-schedule': 'تولید زمان‌بندی',
    '/panel/medical-conditions': 'علائم پزشکی'
  };

  constructor(
    private router: Router,
    private authService: AuthService,
    private dialogService: DialogService,
    private chatService: ChatService
  ) {
    // Subscribe to route changes
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.currentRoute = event.url;
      });

    // Set initial route
    this.currentRoute = this.router.url;
  }

  ngOnInit() {
    // ابتدا از localStorage استفاده می‌کنیم برای نمایش سریع
    this.currentUser = this.authService.getCurrentUser();
    if (this.currentUser) {
      this.username = this.currentUser.nationalCode || `${this.currentUser.firstName} ${this.currentUser.lastName}`.trim() || 'کاربر';
      this.setProfilePicture(this.currentUser.profilePicture);

      // بررسی نقش کاربر - اگر Admin یا Support بود، تعداد چت‌های پاسخ داده نشده را دریافت می‌کنیم
      const userRoles = this.currentUser.roles || [];
      if (userRoles.includes('Admin') || userRoles.includes('Support')) {
        this.loadUnansweredChatCount();
        this.startChatCountPolling();
      }
    }

    // سپس از API به‌روزرسانی می‌کنیم
    this.authService.getCurrentUserFromApi().subscribe({
      next: (user) => {
        this.currentUser = user;
        this.username = user.nationalCode || `${user.firstName} ${user.lastName}`.trim() || 'کاربر';
        this.setProfilePicture(user.profilePicture);
      },
      error: () => {
        // اگر خطا در دریافت از API بود، از localStorage استفاده می‌کنیم (قبلاً تنظیم شده)
      }
    });
  }

  private setProfilePicture(profilePicture: string | undefined) {
    if (profilePicture) {
      if (profilePicture.startsWith('http://') || profilePicture.startsWith('https://')) {
        this.profilePictureUrl = profilePicture;
      } else {
        const baseUrl = environment.apiUrl.replace('/api/v1', '');
        this.profilePictureUrl = profilePicture.startsWith('/')
          ? `${baseUrl}${profilePicture}`
          : `${baseUrl}/${profilePicture}`;
      }
    } else {
      // استفاده از عکس پیش‌فرض
      this.profilePictureUrl = this.getDefaultProfilePicture();
    }
  }

  getDefaultProfilePicture(): string {
    // استفاده از یک placeholder image یا SVG
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2U1ZTdlYiIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iNDAiIGZpbGw9IiM5Y2EzYWYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj7wn5GkPC90ZXh0Pjwvc3ZnPg==';
  }

  getProfilePictureUrl(): string {
    return this.profilePictureUrl || this.getDefaultProfilePicture();
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = this.getDefaultProfilePicture();
    }
  }

  ngOnDestroy() {
    if (this.chatCountSubscription) {
      this.chatCountSubscription.unsubscribe();
    }
  }

  loadUnansweredChatCount() {
    this.chatService.getUnansweredCount().subscribe({
      next: (count) => {
        this.unansweredChatCount = count;
      },
      error: (error) => {
        console.error('Error loading unanswered chat count', error);
        this.unansweredChatCount = 0;
      }
    });
  }

  startChatCountPolling() {
    // هر 30 ثانیه یکبار تعداد چت‌های پاسخ داده نشده را به‌روزرسانی می‌کنیم
    this.chatCountSubscription = interval(30000)
      .subscribe(() => {
        this.loadUnansweredChatCount();
      });
  }

  getCurrentPageTitle(): string {
    return this.pageTitles[this.currentRoute] || 'پنل مدیریت';
  }

  toggleUserMenu() {
    this.userMenuOpen = !this.userMenuOpen;
  }

  closeUserMenu() {
    this.userMenuOpen = false;
  }

  goToProfile() {
    this.router.navigate(['/panel/profile']);
    this.closeUserMenu();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  openChangePasswordDialog() {
    this.closeUserMenu();
    this.dialogService.open(ChangePasswordDialogComponent, {
      width: '500px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      // Dialog closed
    });
  }

  navigateToChatSupport() {
    this.router.navigate(['/panel/chat-support']);
  }
}
