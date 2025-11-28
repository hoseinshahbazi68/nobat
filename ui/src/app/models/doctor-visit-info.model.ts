export interface DoctorVisitInfo {
  id: number;
  doctorId: number;
  doctorName?: string;
  medicalCode?: string;
  about?: string;
  clinicAddress?: string;
  clinicPhone?: string;
  officeHours?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDoctorVisitInfoRequest {
  doctorId: number;
  about?: string;
  clinicAddress?: string;
  clinicPhone?: string;
  officeHours?: string;
}

export interface UpdateDoctorVisitInfoRequest {
  id: number;
  about?: string;
  clinicAddress?: string;
  clinicPhone?: string;
  officeHours?: string;
}
