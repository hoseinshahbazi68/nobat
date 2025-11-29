import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { LayoutComponent } from './components/layout/layout.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { UserRolesComponent } from './pages/user-roles/user-roles.component';
import { UsersComponent } from './pages/users/users.component';
import { UserFormComponent } from './pages/users/user-form.component';
import { HolidaysComponent } from './pages/holidays/holidays.component';
import { ShiftsComponent } from './pages/shifts/shifts.component';
import { ServicesComponent } from './pages/services/services.component';
import { DoctorsComponent } from './pages/doctors/doctors.component';
import { DoctorFormComponent } from './pages/doctors/doctor-form.component';
import { InsurancesComponent } from './pages/insurances/insurances.component';
import { ServiceTariffsComponent } from './pages/service-tariffs/service-tariffs.component';
import { DoctorSchedulesComponent } from './pages/doctor-schedules/doctor-schedules.component';
import { WeeklyScheduleViewComponent } from './pages/doctor-schedules/weekly-schedule-view.component';
import { GenerateScheduleComponent } from './pages/generate-schedule/generate-schedule.component';
import { SpecialtiesComponent } from './pages/specialties/specialties.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { UserActivityLogsComponent } from './pages/user-activity-logs/user-activity-logs.component';
import { DatabaseChangeLogsComponent } from './pages/database-change-logs/database-change-logs.component';
import { QueryLogsComponent } from './pages/query-logs/query-logs.component';
import { HomeComponent } from './pages/home/home.component';
import { ChatSupportComponent } from './pages/chat-support/chat-support.component';
import { ClinicsComponent } from './pages/clinics/clinics.component';
import { ClinicFormComponent } from './pages/clinics/clinic-form.component';
import { DoctorTariffsComponent } from './pages/doctor-tariffs/doctor-tariffs.component';
import { MedicalConditionsComponent } from './pages/medical-conditions/medical-conditions.component';
import { DoctorVisitInfosComponent } from './pages/doctor-visit-infos/doctor-visit-infos.component';
import { DoctorListComponent } from './pages/doctor-list/doctor-list.component';

const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },
  { path: 'doctor-list', component: DoctorListComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'panel',
    component: LayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'user-roles', component: UserRolesComponent },
      { path: 'users', component: UsersComponent },
      { path: 'users/new', component: UserFormComponent },
      { path: 'users/:id', component: UserFormComponent },
      { path: 'holidays', component: HolidaysComponent },
      { path: 'shifts', component: ShiftsComponent },
      { path: 'services', component: ServicesComponent },
      { path: 'doctors', component: DoctorsComponent },
      { path: 'doctors/new', component: DoctorFormComponent },
      { path: 'doctors/:id', component: DoctorFormComponent },
      { path: 'insurances', component: InsurancesComponent },
      { path: 'service-tariffs', component: ServiceTariffsComponent },
      { path: 'doctor-schedules', component: DoctorSchedulesComponent },
      { path: 'weekly-schedule', component: WeeklyScheduleViewComponent },
      { path: 'generate-schedule', component: GenerateScheduleComponent },
      { path: 'specialties', component: SpecialtiesComponent },
      { path: 'user-activity-logs', component: UserActivityLogsComponent },
      { path: 'query-logs', component: QueryLogsComponent },
      { path: 'database-change-logs', component: DatabaseChangeLogsComponent },
      { path: 'chat-support', component: ChatSupportComponent },
      { path: 'profile', component: ProfileComponent },
      { path: 'clinics', component: ClinicsComponent },
      { path: 'clinics/new', component: ClinicFormComponent },
      { path: 'clinics/:id', component: ClinicFormComponent },
      { path: 'doctor-tariffs', component: DoctorTariffsComponent },
      { path: 'medical-conditions', component: MedicalConditionsComponent },
      { path: 'doctor-visit-infos', component: DoctorVisitInfosComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
