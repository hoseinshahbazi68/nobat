import { Clinic } from './clinic.model';
import { DoctorMedicalCondition } from './medical-condition.model';
import { Specialty } from './specialty.model';

export enum DoctorPrefix {
  None = 0,
  Doctor = 1,
  Bachelor = 2,
  Master = 3
}

export enum ScientificDegree {
  None = 0,
  Fellowship = 1,
  Subspecialty = 2,
  ProfessionalDoctorate = 3,
  Specialist = 4,
  Doctorate = 5,
  Master = 6,
  Bachelor = 7
}

export interface Doctor {
  id?: number;
  firstName?: string;
  lastName?: string;
  phone?: string;
  email?: string;
  specialties?: DoctorSpecialty[]; // تخصص‌های جدید
  medicalCode: string;
  prefix?: DoctorPrefix;
  scientificDegree?: ScientificDegree;
  nationalCode: string;
  userId?: number; // شناسه کاربر (اختیاری)
  user?: any; // اطلاعات کاربر (شامل firstName, lastName, phone, email)
  clinics?: Clinic[]; // لیست کلینیک‌های پزشک
  medicalConditions?: DoctorMedicalCondition[]; // لیست علائم پزشکی مرتبط با پزشک
  profilePicture?: string; // مسیر عکس پروفایل
  cityId?: number; // شناسه شهر
}

export interface DoctorSpecialty {
  id?: number;
  doctorId: number;
  specialtyId: number;
  specialty?: Specialty;
  sortOrder: number;
}

import { Gender } from './user.model';

export interface CreateDoctorRequest {
  specialtyIds?: number[]; // لیست شناسه تخصص‌ها
  medicalConditionIds?: number[]; // لیست شناسه علائم پزشکی
  medicalCode: string;
  prefix?: DoctorPrefix;
  scientificDegree?: ScientificDegree;
  userId?: number; // شناسه کاربر (اختیاری)
  nationalCode?: string; // کد ملی کاربر (برای جستجو و ایجاد کاربر در صورت عدم وجود)
  clinicId?: number; // شناسه کلینیک (اختیاری)
  firstName?: string; // نام (برای ایجاد کاربر جدید)
  lastName?: string; // نام خانوادگی (برای ایجاد کاربر جدید)
  phone?: string; // شماره تلفن (برای ایجاد کاربر جدید)
  email?: string; // ایمیل (برای ایجاد کاربر جدید)
  cityId?: number; // شناسه شهر (برای ایجاد کاربر جدید)
  gender?: Gender; // جنسیت (برای ایجاد کاربر جدید)
  birthDate?: string; // تاریخ تولد (برای ایجاد کاربر جدید)
}

export interface UpdateDoctorRequest {
  id: number;
  specialtyIds?: number[]; // لیست شناسه تخصص‌ها
  medicalConditionIds?: number[]; // لیست شناسه علائم پزشکی
  medicalCode: string;
  prefix?: DoctorPrefix;
  scientificDegree?: ScientificDegree;
  userId?: number; // شناسه کاربر (اختیاری)
  clinicId?: number; // شناسه کلینیک (اختیاری)
  cityId?: number; // شناسه شهر (برای به‌روزرسانی کاربر)
  gender?: Gender; // جنسیت (برای به‌روزرسانی کاربر)
  birthDate?: string; // تاریخ تولد (برای به‌روزرسانی کاربر)
}

export interface DoctorListDto {
  id: number;
  firstName: string;
  lastName: string;
  phone?: string;
  medicalCode: string;
  prefix?: DoctorPrefix;
  scientificDegree?: ScientificDegree;
  nationalCode: string;
  createdAt: string;
  userId?: number;
}

// Helper functions for displaying enum values
export function getDoctorPrefixLabel(prefix: DoctorPrefix | undefined): string {
  if (prefix === undefined) return 'نامشخص';
  switch (prefix) {
    case DoctorPrefix.Doctor:
      return 'دکتر';
    case DoctorPrefix.Bachelor:
      return 'کارشناس';
    case DoctorPrefix.Master:
      return 'کارشناس ارشد';
    default:
      return 'نامشخص';
  }
}

export function getScientificDegreeLabel(degree: ScientificDegree | undefined): string {
  if (degree === undefined) return 'نامشخص';
  switch (degree) {
    case ScientificDegree.Fellowship:
      return 'فلوشیپ';
    case ScientificDegree.Subspecialty:
      return 'فوق تخصص';
    case ScientificDegree.ProfessionalDoctorate:
      return 'دکترای تخصصی';
    case ScientificDegree.Specialist:
      return 'متخصص';
    case ScientificDegree.Doctorate:
      return 'دکتری';
    case ScientificDegree.Master:
      return 'کارشناس ارشد';
    case ScientificDegree.Bachelor:
      return 'کارشناس';
    default:
      return 'نامشخص';
  }
}
