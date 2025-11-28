import { Component, OnInit } from '@angular/core';
import { DialogService } from '../../services/dialog.service';
import { SnackbarService } from '../../services/snackbar.service';
import { RoleService } from '../../services/role.service';
import { UserRoleDialogComponent } from './user-role-dialog.component';
import { Role } from '../../models/role.model';

@Component({
  selector: 'app-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss']
})
export class UserRolesComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'description', 'actions'];
  roles: Role[] = [];
  filteredRoles: Role[] = [];
  filterValue: string = '';
  isLoading = false;

  // Pagination properties
  currentPage: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  constructor(
    private dialogService: DialogService,
    private snackbarService: SnackbarService,
    private roleService: RoleService
  ) {}

  ngOnInit() {
    this.loadRoles();
  }

  loadRoles() {
    this.isLoading = true;
    this.roleService.getAll({ page: this.currentPage, pageSize: this.pageSize }).subscribe({
      next: (result) => {
        this.roles = result.items;
        this.filteredRoles = [...this.roles];
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.currentPage = result.page;
        this.isLoading = false;
      },
      error: (error) => {
        const errorMessage = error?.message || error?.error?.message || 'خطا در بارگذاری نقش‌ها';
        this.snackbarService.error(errorMessage, 'بستن', 5000);
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadRoles();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.currentPage = 1;
    this.loadRoles();
  }

  openAddDialog() {
    this.dialogService.open(UserRoleDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: null
    }).subscribe(result => {
      if (result && typeof result === 'object' && result.name) {
        this.saveRole(result);
      }
    });
  }

  editRole(role: Role) {
    this.dialogService.open(UserRoleDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: role
    }).subscribe(result => {
      if (result) {
        this.saveRole(result, role.id);
      }
    });
  }

  deleteRole(role: Role) {
    if (role.id) {
      this.dialogService.confirm({
        title: 'حذف نقش',
        message: `آیا از حذف نقش "${role.name}" اطمینان دارید؟`,
        confirmText: 'حذف',
        cancelText: 'انصراف',
        type: 'danger'
      }).subscribe(result => {
        if (result) {
          this.roleService.delete(role.id!).subscribe({
            next: () => {
              this.roles = this.roles.filter(r => r.id !== role.id);
              this.filteredRoles = [...this.roles];
              this.applyFilter();
              this.snackbarService.success('نقش با موفقیت حذف شد', 'بستن', 3000);
            },
            error: (error) => {
              const errorMessage = error?.message || error?.error?.message || 'خطا در حذف نقش';
              this.snackbarService.error(errorMessage, 'بستن', 5000);
            }
          });
        }
      });
    }
  }

  saveRole(roleData: any, id?: number) {
    if (!roleData || !roleData.name) {
      this.snackbarService.error('لطفا نام نقش را وارد نمایید', 'بستن', 3000);
      return;
    }

    if (id) {
      const updateData = { ...roleData, id };
      this.roleService.update(updateData).subscribe({
        next: (updatedRole) => {
          const index = this.roles.findIndex(r => r.id === id);
          if (index !== -1) {
            this.roles[index] = updatedRole;
            this.filteredRoles = [...this.roles];
            this.applyFilter();
          } else {
            this.loadRoles();
          }
          this.snackbarService.success('نقش با موفقیت ویرایش شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در ویرایش نقش';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    } else {
      this.roleService.create(roleData).subscribe({
        next: (newRole) => {
          this.roles = [...this.roles, newRole];
          this.filteredRoles = [...this.roles];
          this.applyFilter();
          this.snackbarService.success('نقش با موفقیت اضافه شد', 'بستن', 3000);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'خطا در افزودن نقش';
          this.snackbarService.error(errorMessage, 'بستن', 5000);
        }
      });
    }
  }

  applyFilter() {
    // Reset to first page when filtering
    this.currentPage = 1;
    this.loadRoles();
  }
}
