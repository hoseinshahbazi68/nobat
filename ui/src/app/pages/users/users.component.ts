import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User, Gender } from '../../models/user.model';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { UserService } from '../../services/user.service';
import { UserActionsMenuComponent } from './user-actions-menu.component';
import { UserClinicsDialogComponent } from './user-clinics-dialog.component';
import { AdminChangeUserPasswordDialogComponent } from './admin-change-user-password-dialog.component';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {
  displayedColumns: string[] = ['id', 'nationalCode', 'email', 'fullName', 'role', 'isActive', 'actions'];
  users: User[] = [];
  filteredUsers: User[] = [];
  filterValue: string = '';
  isLoading = false;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private router: Router,
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private userService: UserService
  ) { }

  ngOnInit() {
    this.loadUsers();
  }


  loadUsers() {
    this.isLoading = true;
    const params: any = { page: this.currentPage, pageSize: this.pageSize };

    // اضافه کردن فیلتر SieveModel اگر filterValue وجود دارد
    if (this.filterValue && this.filterValue.trim()) {
      const searchTerm = this.filterValue.trim();
      // جستجو در nationalCode, email, firstName, lastName
      params.filters = `NationalCode@=*${searchTerm}*|Email@=*${searchTerm}*|FirstName@=*${searchTerm}*|LastName@=*${searchTerm}*`;
    }

    this.userService.getAll(params).subscribe({
      next: (result) => {
        this.users = result.items;
        this.filteredUsers = [...this.users];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'خطا در بارگذاری کاربران';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadUsers();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadUsers();
  }

  openAddDialog() {
    this.router.navigate(['/panel/users/new']);
  }

  editUser(user: User) {
    if (user.id) {
      this.router.navigate(['/panel/users', user.id]);
    }
  }

  deleteUser(user: User) {
    if (user.id) {
      this.dialogService.confirm({
        title: 'حذف کاربر',
        message: `آیا از حذف کاربر "${user.nationalCode}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.userService.delete(user.id!).subscribe({
            next: () => {
              this.users = this.users.filter(u => u.id !== user.id);
              this.filteredUsers = [...this.users];
              this.applyFilter();
              this.snackbarService.success('کاربر با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error.error?.message || 'خطا در حذف کاربر';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }


  applyFilter() {
    // بازنشانی به صفحه اول هنگام جستجو
    this.currentPage = 1;
    this.loadUsers();
  }

  getFullName(user: User): string {
    return `${user.firstName} ${user.lastName}`;
  }

  getRoles(user: User): string {
    return (user.roles && user.roles.length > 0) ? user.roles.join(', ') : 'بدون نقش';
  }

  getGenderText(gender?: Gender): string {
    if (gender === undefined || gender === null) {
      return '-';
    }
    switch (gender) {
      case Gender.Male:
        return 'مرد';
      case Gender.Female:
        return 'زن';
      default:
        return '-';
    }
  }

  openActionsMenu(event: Event, user: User) {
    event.stopPropagation();

    // Get button position
    const button = event.currentTarget as HTMLElement;
    const rect = button.getBoundingClientRect();

    this.dialogService.open(UserActionsMenuComponent, {
      width: 'auto',
      maxWidth: '90vw',
      data: user,
      position: {
        top: `${rect.bottom + 8}px`,
        right: `${window.innerWidth - rect.right}px`
      },
      transparentOverlay: true
    }).subscribe(result => {
      if (result && result.action) {
        this.handleMenuAction(result.action, result.user);
      }
    });
  }

  handleMenuAction(action: string, user: User) {
    if (action === 'edit') {
      this.editUser(user);
    } else if (action === 'delete') {
      this.deleteUser(user);
    } else if (action === 'clinics') {
      this.openUserClinicsDialog(user);
    } else if (action === 'change-password') {
      this.openChangePasswordDialog(user);
    }
  }

  openUserClinicsDialog(user: User) {
    this.dialogService.open(UserClinicsDialogComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: user
    }).subscribe(result => {
      // Dialog closed
    });
  }

  openChangePasswordDialog(user: User) {
    this.dialogService.open(AdminChangeUserPasswordDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: user
    }).subscribe(result => {
      // Dialog closed
    });
  }
}
