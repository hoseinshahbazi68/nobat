import { NgModule, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/auth/login/login.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { LayoutComponent } from './components/layout/layout.component';
import { SidebarComponent } from './components/layout/sidebar/sidebar.component';
import { HeaderComponent } from './components/layout/header/header.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';

// Pages
import { UserRolesComponent } from './pages/user-roles/user-roles.component';
import { UsersComponent } from './pages/users/users.component';
import { HolidaysComponent } from './pages/holidays/holidays.component';
import { HolidayDialogComponent } from './pages/holidays/holiday-dialog.component';
import { ServiceDialogComponent } from './pages/services/service-dialog.component';
import { UserRoleDialogComponent } from './pages/user-roles/user-role-dialog.component';
import { UserDialogComponent } from './pages/users/user-dialog.component';
import { UserClinicsDialogComponent } from './pages/users/user-clinics-dialog.component';
import { ShiftsComponent } from './pages/shifts/shifts.component';
import { ShiftDialogComponent } from './pages/shifts/shift-dialog.component';
import { ServicesComponent } from './pages/services/services.component';
import { DoctorsComponent } from './pages/doctors/doctors.component';
import { DoctorFormComponent } from './pages/doctors/doctor-form.component';
import { InsurancesComponent } from './pages/insurances/insurances.component';
import { InsuranceDialogComponent } from './pages/insurances/insurance-dialog.component';
import { ServiceTariffsComponent } from './pages/service-tariffs/service-tariffs.component';
import { TariffDialogComponent } from './pages/service-tariffs/tariff-dialog.component';
import { DoctorSchedulesComponent } from './pages/doctor-schedules/doctor-schedules.component';
import { DoctorScheduleDialogComponent } from './pages/doctor-schedules/doctor-schedule-dialog.component';
import { WeeklyScheduleViewComponent } from './pages/doctor-schedules/weekly-schedule-view.component';
import { WeeklyDayScheduleDialogComponent } from './pages/doctor-schedules/weekly-day-schedule-dialog.component';
import { CreateAppointmentDialogComponent } from './pages/doctor-schedules/create-appointment-dialog.component';
import { GenerateScheduleComponent } from './pages/generate-schedule/generate-schedule.component';
import { SpecialtiesComponent } from './pages/specialties/specialties.component';
import { SpecialtyDialogComponent } from './pages/specialties/specialty-dialog.component';
import { SpecialtyMedicalConditionsDialogComponent } from './pages/specialties/specialty-medical-conditions-dialog.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { UserActivityLogsComponent } from './pages/user-activity-logs/user-activity-logs.component';
import { DatabaseChangeLogsComponent } from './pages/database-change-logs/database-change-logs.component';
import { QueryLogsComponent } from './pages/query-logs/query-logs.component';
import { QueryLogDetailDialogComponent } from './pages/query-logs/query-log-detail-dialog.component';
import { PersianDatepickerComponent } from './components/persian-datepicker/persian-datepicker.component';
import { PersianCalendarComponent } from './components/persian-calendar/persian-calendar.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ChangePasswordDialogComponent } from './components/change-password-dialog/change-password-dialog.component';
import { PaginationComponent } from './components/pagination/pagination.component';
import { LoadingComponent } from './components/loading/loading.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { ChatModalComponent } from './components/chat-modal/chat-modal.component';
import { HomeComponent } from './pages/home/home.component';
import { ChatSupportComponent } from './pages/chat-support/chat-support.component';
import { ClinicsComponent } from './pages/clinics/clinics.component';
import { ClinicDialogComponent } from './pages/clinics/clinic-dialog.component';
import { UserActionsMenuComponent } from './pages/users/user-actions-menu.component';
import { DoctorActionsMenuComponent } from './pages/doctors/doctor-actions-menu.component';
import { DoctorTariffsComponent } from './pages/doctor-tariffs/doctor-tariffs.component';
import { MedicalConditionsComponent } from './pages/medical-conditions/medical-conditions.component';
import { MedicalConditionDialogComponent } from './pages/medical-conditions/medical-condition-dialog.component';
import { DoctorVisitInfosComponent } from './pages/doctor-visit-infos/doctor-visit-infos.component';
import { DoctorVisitInfoDialogComponent } from './pages/doctor-visit-infos/doctor-visit-info-dialog.component';
import { ImageCropperComponent } from './components/image-cropper/image-cropper.component';
import { AuthInterceptor } from './interceptors/auth.interceptor';

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
    UserClinicsDialogComponent,
    UserActionsMenuComponent,
    HolidaysComponent,
    HolidayDialogComponent,
    ServicesComponent,
    ServiceDialogComponent,
    ShiftsComponent,
    ShiftDialogComponent,
    DoctorsComponent,
    DoctorFormComponent,
    DoctorActionsMenuComponent,
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
    ClinicsComponent,
    ClinicDialogComponent,
    DoctorTariffsComponent,
    MedicalConditionsComponent,
    MedicalConditionDialogComponent,
    DoctorVisitInfosComponent,
    DoctorVisitInfoDialogComponent,
    ImageCropperComponent
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
