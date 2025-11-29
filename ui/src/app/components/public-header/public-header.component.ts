import { Component, OnInit, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { DialogService } from '../../services/dialog.service';
import { ChangePasswordDialogComponent } from '../change-password-dialog/change-password-dialog.component';

@Component({
  selector: 'app-public-header',
  templateUrl: './public-header.component.html',
  styleUrls: ['./public-header.component.scss']
})
export class PublicHeaderComponent implements OnInit {
  currentUser: User | null = null;
  isAuthenticated = false;
  userPanelOpen = false;
  treatmentCentersDropdownOpen = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private dialogService: DialogService
  ) { }

  ngOnInit() {
    this.checkAuthentication();
  }

  checkAuthentication() {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.currentUser = this.authService.getCurrentUser();
    }
  }

  getUserRoleDisplay(): string {
    if (!this.currentUser || !this.currentUser.roles || this.currentUser.roles.length === 0) {
      return 'کاربر';
    }
    const roleMap: { [key: string]: string } = {
      'Admin': 'مدیر سیستم',
      'User': 'کاربر',
      'Doctor': 'پزشک',
      'Receptionist': 'منشی'
    };
    return this.currentUser.roles.map(role => roleMap[role] || role).join('، ');
  }

  getUserFullName(): string {
    if (!this.currentUser) {
      return 'مدیر سیستم';
    }
    const firstName = this.currentUser.firstName || '';
    const lastName = this.currentUser.lastName || '';
    const fullName = `${firstName} ${lastName}`.trim();
    return fullName || this.currentUser.nationalCode || 'مدیر سیستم';
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-panel-container')) {
      this.userPanelOpen = false;
    }
    if (!target.closest('.dropdown')) {
      this.treatmentCentersDropdownOpen = false;
    }
  }

  toggleTreatmentCentersDropdown() {
    this.treatmentCentersDropdownOpen = !this.treatmentCentersDropdownOpen;
  }

  toggleUserPanel() {
    this.userPanelOpen = !this.userPanelOpen;
  }

  closeUserPanel() {
    this.userPanelOpen = false;
  }

  openChangePasswordDialog() {
    this.closeUserPanel();
    this.dialogService.open(ChangePasswordDialogComponent, {
      width: '500px',
      maxWidth: '90vw'
    }).subscribe();
  }

  goToProfile() {
    this.closeUserPanel();
    this.router.navigate(['/panel/profile']);
  }

  logout() {
    this.closeUserPanel();
    this.authService.logout();
    this.isAuthenticated = false;
    this.currentUser = null;
    this.router.navigate(['/home']);
  }

  goToDoctorList() {
    this.router.navigate(['/doctor-list']);
  }

  goToHome() {
    this.router.navigate(['/home']);
  }
}

