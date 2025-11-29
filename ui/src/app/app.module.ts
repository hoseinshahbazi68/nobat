import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { CUSTOM_ELEMENTS_SCHEMA, NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/auth/login/login.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { HeaderComponent } from './components/layout/header/header.component';
import { LayoutComponent } from './components/layout/layout.component';
import { SidebarComponent } from './components/layout/sidebar/sidebar.component';

// Pages
import { ChangePasswordDialogComponent } from './components/change-password-dialog/change-password-dialog.component';
import { ChatModalComponent } from './components/chat-modal/chat-modal.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { ImageCropperComponent } from './components/image-cropper/image-cropper.component';
import { LoadingComponent } from './components/loading/loading.component';
import { PaginationComponent } from './components/pagination/pagination.component';
import { PersianCalendarComponent } from './components/persian-calendar/persian-calendar.component';
import { PersianDatepickerComponent } from './components/persian-datepicker/persian-datepicker.component';
import { RichTextEditorComponent } from './components/rich-text-editor/rich-text-editor.component';
import { SettingsComponent } from './components/settings/settings.component';
import { TimeMaskDirective } from './directives/time-mask.directive';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { PublicHeaderComponent } from './components/public-header/public-header.component';
import { ChatSupportComponent } from './pages/chat-support/chat-support.component';
import { ClinicDialogComponent } from './pages/clinics/clinic-dialog.component';
import { ClinicFormComponent } from './pages/clinics/clinic-form.component';
import { ClinicUsersDialogComponent } from './pages/clinics/clinic-users-dialog.component';
import { ClinicsComponent } from './pages/clinics/clinics.component';
import { DatabaseChangeLogsComponent } from './pages/database-change-logs/database-change-logs.component';
import { CreateAppointmentDialogComponent } from './pages/doctor-schedules/create-appointment-dialog.component';
import { DoctorScheduleDialogComponent } from './pages/doctor-schedules/doctor-schedule-dialog.component';
import { DoctorSchedulesComponent } from './pages/doctor-schedules/doctor-schedules.component';
import { WeeklyDayScheduleDialogComponent } from './pages/doctor-schedules/weekly-day-schedule-dialog.component';
import { WeeklyScheduleViewComponent } from './pages/doctor-schedules/weekly-schedule-view.component';
import { DoctorTariffsComponent } from './pages/doctor-tariffs/doctor-tariffs.component';
import { DoctorVisitInfoDialogComponent } from './pages/doctor-visit-infos/doctor-visit-info-dialog.component';
import { DoctorVisitInfosComponent } from './pages/doctor-visit-infos/doctor-visit-infos.component';
import { AdminChangeDoctorPasswordDialogComponent } from './pages/doctors/admin-change-doctor-password-dialog.component';
import { DoctorActionsMenuComponent } from './pages/doctors/doctor-actions-menu.component';
import { DoctorFormComponent } from './pages/doctors/doctor-form.component';
import { DoctorsComponent } from './pages/doctors/doctors.component';
import { GenerateScheduleComponent } from './pages/generate-schedule/generate-schedule.component';
import { HolidayDialogComponent } from './pages/holidays/holiday-dialog.component';
import { HolidaysComponent } from './pages/holidays/holidays.component';
import { HomeComponent } from './pages/home/home.component';
import { DoctorListComponent } from './pages/doctor-list/doctor-list.component';
import { InsuranceDialogComponent } from './pages/insurances/insurance-dialog.component';
import { InsurancesComponent } from './pages/insurances/insurances.component';
import { MedicalConditionDialogComponent } from './pages/medical-conditions/medical-condition-dialog.component';
import { MedicalConditionsComponent } from './pages/medical-conditions/medical-conditions.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { QueryLogDetailDialogComponent } from './pages/query-logs/query-log-detail-dialog.component';
import { QueryLogsComponent } from './pages/query-logs/query-logs.component';
import { ServiceTariffsComponent } from './pages/service-tariffs/service-tariffs.component';
import { TariffDialogComponent } from './pages/service-tariffs/tariff-dialog.component';
import { ServiceDialogComponent } from './pages/services/service-dialog.component';
import { ServicesComponent } from './pages/services/services.component';
import { ShiftDialogComponent } from './pages/shifts/shift-dialog.component';
import { ShiftsComponent } from './pages/shifts/shifts.component';
import { SpecialtiesComponent } from './pages/specialties/specialties.component';
import { SpecialtyDialogComponent } from './pages/specialties/specialty-dialog.component';
import { SpecialtyMedicalConditionsDialogComponent } from './pages/specialties/specialty-medical-conditions-dialog.component';
import { UserActivityLogsComponent } from './pages/user-activity-logs/user-activity-logs.component';
import { UserRoleDialogComponent } from './pages/user-roles/user-role-dialog.component';
import { UserRolesComponent } from './pages/user-roles/user-roles.component';
import { AdminChangeUserPasswordDialogComponent } from './pages/users/admin-change-user-password-dialog.component';
import { UserActionsMenuComponent } from './pages/users/user-actions-menu.component';
import { UserClinicsDialogComponent } from './pages/users/user-clinics-dialog.component';
import { UserDialogComponent } from './pages/users/user-dialog.component';
import { UserFormComponent } from './pages/users/user-form.component';
import { UsersComponent } from './pages/users/users.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    LayoutComponent,
    SidebarComponent,
    HeaderComponent,
    DashboardComponent,
    UserRolesComponent,
    UserRoleDialogComponent,
    UsersComponent,
    UserDialogComponent,
    UserFormComponent,
    UserClinicsDialogComponent,
    UserActionsMenuComponent,
    AdminChangeUserPasswordDialogComponent,
    HolidaysComponent,
    HolidayDialogComponent,
    ServicesComponent,
    ServiceDialogComponent,
    ShiftsComponent,
    ShiftDialogComponent,
    DoctorsComponent,
    DoctorFormComponent,
    DoctorActionsMenuComponent,
    AdminChangeDoctorPasswordDialogComponent,
    InsurancesComponent,
    InsuranceDialogComponent,
    ServiceTariffsComponent,
    TariffDialogComponent,
    DoctorSchedulesComponent,
    DoctorScheduleDialogComponent,
    WeeklyScheduleViewComponent,
    WeeklyDayScheduleDialogComponent,
    CreateAppointmentDialogComponent,
    GenerateScheduleComponent,
    SpecialtiesComponent,
    SpecialtyDialogComponent,
    SpecialtyMedicalConditionsDialogComponent,
    ProfileComponent,
    UserActivityLogsComponent,
    DatabaseChangeLogsComponent,
    QueryLogsComponent,
    QueryLogDetailDialogComponent,
    PersianDatepickerComponent,
    PersianCalendarComponent,
    SettingsComponent,
    ChangePasswordDialogComponent,
    PaginationComponent,
    LoadingComponent,
    ConfirmDialogComponent,
    ChatModalComponent,
    HomeComponent,
    ChatSupportComponent,
    DoctorListComponent,
    PublicHeaderComponent,
    ClinicsComponent,
    ClinicDialogComponent,
    ClinicFormComponent,
    ClinicUsersDialogComponent,
    DoctorTariffsComponent,
    MedicalConditionsComponent,
    MedicalConditionDialogComponent,
    DoctorVisitInfosComponent,
    DoctorVisitInfoDialogComponent,
    ImageCropperComponent,
    RichTextEditorComponent,
    TimeMaskDirective
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA]
})
export class AppModule { }
