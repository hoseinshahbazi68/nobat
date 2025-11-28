import { Component, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { interval, Subscription } from 'rxjs';
import { ChatService } from '../../../services/chat.service';
import { AuthService } from '../../../services/auth.service';
import { ClinicService } from '../../../services/clinic.service';
import { Clinic } from '../../../models/clinic.model';
import { User } from '../../../models/user.model';

interface MenuItem {
  title: string;
  icon: string;
  route: string;
  badgeCount?: number;
}

interface MenuGroup {
  title: string;
  icon: string;
  items: MenuItem[];
  expanded?: boolean;
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Output() closeSidenav = new EventEmitter<void>();

  searchQuery: string = '';
  private chatCountSubscription?: Subscription;

  // Clinic dropdown properties
  clinics: Clinic[] = [];
  selectedClinic: Clinic | null = null;
  clinicDropdownOpen: boolean = false;
  currentUser: User | null = null;

  // Dashboard as standalone menu item
  dashboardItem: MenuItem = {
    title: 'داشبورد',
    icon: 'dashboard',
    route: '/panel/dashboard'
  };

  menuGroups: MenuGroup[] = [
    {
      title: 'مدیریت کاربران',
      icon: 'people',
      items: [
        { title: 'نقش کاربران', icon: 'admin_panel_settings', route: '/panel/user-roles' },
        { title: 'کاربران', icon: 'people', route: '/panel/users' }
      ],
      expanded: false
    },
    {
      title: 'مدیریت زمان‌بندی',
      icon: 'schedule',
      items: [
        { title: 'تولید زمان‌بندی', icon: 'auto_awesome', route: '/panel/generate-schedule' }
      ],
      expanded: false
    },
    {
      title: 'مدیریت خدمات',
      icon: 'medical_services',
      items: [
        { title: 'پزشکان', icon: 'local_hospital', route: '/panel/doctors' }
      ],
      expanded: false
    },
    {
      title: 'اطلاعات پایه',
      icon: 'info',
      items: [
        { title: 'بیمه', icon: 'health_and_safety', route: '/panel/insurances' },
        { title: 'خدمت', icon: 'medical_services', route: '/panel/services' },
        { title: 'تخصص', icon: 'badge', route: '/panel/specialties' },
        { title: 'کلینیک‌ها', icon: 'local_hospital', route: '/panel/clinics' },
        { title: 'شیفت', icon: 'schedule', route: '/panel/shifts' },
        { title: 'روزهای تعطیل', icon: 'event_busy', route: '/panel/holidays' },
        { title: 'علائم پزشکی', icon: 'medical_information', route: '/panel/medical-conditions' }
      ],
      expanded: false
    },
    {
      title: 'پشتیبانی',
      icon: 'support_agent',
      items: [
        { title: 'پشتیبانی چت', icon: 'chat', route: '/panel/chat-support', badgeCount: 0 }
      ],
      expanded: false
    },
    {
      title: 'گزارش‌ها',
      icon: 'query_stats',
      items: [
        { title: 'گزارش لاگ کوئری‌ها', icon: 'query_stats', route: '/panel/query-logs' },
        { title: 'لاگ فعالیت‌ها', icon: 'history', route: '/panel/user-activity-logs' }
      ],
      expanded: false
    }
  ];

  constructor(
    private router: Router,
    private chatService: ChatService,
    private authService: AuthService,
    private clinicService: ClinicService,
    private cdr: ChangeDetectorRef
  ) {
    // Close sidenav on route change (for mobile) and expand relevant group
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.closeSidenav.emit();
        this.expandActiveGroup();
      });

    // Close dropdown when clicking outside
    document.addEventListener('click', this.handleDocumentClick);
  }

  ngOnInit() {
    // Expand group for current route on init
    this.expandActiveGroup();

    // دریافت اطلاعات کاربر فعلی
    this.currentUser = this.authService.getCurrentUser();
    if (this.currentUser) {
      const userRoles = this.currentUser.roles || [];

      // اگر کاربر Admin یا Support است، تعداد چت‌های پاسخ داده نشده را دریافت می‌کنیم
      if (userRoles.includes('Admin') || userRoles.includes('Support')) {
        this.loadUnansweredChatCount();
        this.startChatCountPolling();
      }

      // بارگذاری کلینیک‌های کاربر برای همه کاربران
      this.loadUserClinics();
    }
  }

  ngOnDestroy() {
    if (this.chatCountSubscription) {
      this.chatCountSubscription.unsubscribe();
    }
    // Remove event listener
    document.removeEventListener('click', this.handleDocumentClick);
  }

  private handleDocumentClick = (event: MouseEvent) => {
    if (this.clinicDropdownOpen) {
      const target = event.target as HTMLElement;
      const dropdownContainer = document.querySelector('.clinic-dropdown-container');
      if (dropdownContainer && !dropdownContainer.contains(target)) {
        this.closeClinicDropdown();
      }
    }
  }

  loadUnansweredChatCount() {
    this.chatService.getUnansweredCount().subscribe({
      next: (count) => {
        const supportGroup = this.menuGroups.find(g => g.title === 'پشتیبانی');
        if (supportGroup) {
          const chatItem = supportGroup.items.find(i => i.route === '/panel/chat-support');
          if (chatItem) {
            chatItem.badgeCount = count;
          }
        }
      },
      error: (error) => {
        console.error('Error loading unanswered chat count', error);
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

  expandActiveGroup() {
    const currentUrl = this.router.url;

    // Close all groups first
    this.menuGroups.forEach(group => {
      group.expanded = false;
    });

    // Don't expand any group if dashboard is active
    if (currentUrl === this.dashboardItem.route || currentUrl.startsWith(this.dashboardItem.route + '/')) {
      return;
    }

    // Find and expand the group containing the active route
    for (const group of this.menuGroups) {
      const hasActiveItem = group.items.some(item => {
        // Check exact match or if current URL starts with the route
        return currentUrl === item.route || currentUrl.startsWith(item.route + '/');
      });

      if (hasActiveItem) {
        group.expanded = true;
        break;
      }
    }
  }

  get filteredMenuGroups(): MenuGroup[] {
    if (!this.searchQuery.trim()) {
      return this.menuGroups;
    }

    const query = this.searchQuery.toLowerCase().trim();
    return this.menuGroups
      .map(group => ({
        ...group,
        items: group.items.filter(item =>
          item.title.toLowerCase().includes(query) ||
          group.title.toLowerCase().includes(query)
        )
      }))
      .filter(group => group.items.length > 0);
  }

  get shouldShowDashboard(): boolean {
    if (!this.searchQuery.trim()) {
      return true;
    }
    const query = this.searchQuery.toLowerCase().trim();
    return this.dashboardItem.title.toLowerCase().includes(query);
  }

  toggleGroup(group: MenuGroup) {
    group.expanded = !group.expanded;
  }

  navigate(route: string) {
    this.router.navigate([route]);
  }

  isActive(route: string): boolean {
    return this.router.url === route;
  }

  getIcon(iconName: string): string {
    const iconMap: { [key: string]: string } = {
      'dashboard': '📊',
      'admin_panel_settings': '👥',
      'people': '👤',
      'event_busy': '📅',
      'schedule': '⏰',
      'medical_services': '🏥',
      'local_hospital': '👨‍⚕️',
      'badge': '🎓',
      'health_and_safety': '🛡️',
      'receipt_long': '💰',
      'calendar_today': '📆',
      'auto_awesome': '✨',
      'query_stats': '📊',
      'history': '📋',
      'support_agent': '💬',
      'chat': '💬',
      'business': '🏢',
      'info': 'ℹ️',
      'medical_information': '🩺'
    };
    return iconMap[iconName] || '📄';
  }

  clearSearch() {
    this.searchQuery = '';
  }

  loadUserClinics() {
    this.authService.getCurrentUserClinics().subscribe({
      next: (clinics) => {
        this.clinics = clinics || [];
        if (this.clinics.length > 0) {
          // اگر کلینیکی در کوکی ذخیره شده، آن را انتخاب کن
          const savedClinicId = this.getCookie('selectedClinicId');
          if (savedClinicId) {
            const savedClinic = this.clinics.find(c => c.id?.toString() === savedClinicId);
            if (savedClinic) {
              this.selectedClinic = savedClinic;
              this.clinicService.setSelectedClinic(savedClinic);
            } else {
              // اگر کلینیک ذخیره شده در لیست نیست، اولین کلینیک را انتخاب کن
              this.selectedClinic = this.clinics[0];
              this.clinicService.setSelectedClinic(this.selectedClinic);
            }
          } else {
            // اگر هیچ کلینیکی در کوکی نیست، اولین کلینیک را انتخاب کن
            this.selectedClinic = this.clinics[0];
            this.clinicService.setSelectedClinic(this.selectedClinic);
          }
        }
      },
      error: (error) => {
        console.error('Error loading user clinics', error);
      }
    });
  }

  toggleClinicDropdown() {
    this.clinicDropdownOpen = !this.clinicDropdownOpen;
  }

  closeClinicDropdown() {
    this.clinicDropdownOpen = false;
  }

  selectClinic(clinic: Clinic) {
    console.log('Selecting clinic:', clinic);
    console.log('Current selected clinic:', this.selectedClinic);

    // بررسی اینکه آیا کلینیک واقعاً تغییر کرده است
    const isSameClinic = this.selectedClinic?.id === clinic.id;

    // همیشه کلینیک را به‌روزرسانی کن (حتی اگر همان کلینیک باشد)
    this.selectedClinic = clinic;
    this.clinicService.setSelectedClinic(clinic);
    this.closeClinicDropdown();

    // Force change detection
    this.cdr.detectChanges();

    console.log('Selected clinic updated to:', this.selectedClinic);

    // فقط اگر کلینیک تغییر کرده باشد، صفحه را رفرش کن
    if (!isSameClinic) {
      // رفرش صفحه برای به‌روزرسانی داده‌ها با استفاده از Router
      const currentUrl = this.router.url;
      this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
        this.router.navigate([currentUrl]);
      });
    }
  }

  getCookie(name: string): string | null {
    const nameEQ = name + '=';
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
      let c = ca[i];
      while (c.charAt(0) === ' ') c = c.substring(1, c.length);
      if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
  }

  setCookie(name: string, value: string, days: number) {
    let expires = '';
    if (days) {
      const date = new Date();
      date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
      expires = '; expires=' + date.toUTCString();
    }
    document.cookie = name + '=' + (value || '') + expires + '; path=/';
  }
}
