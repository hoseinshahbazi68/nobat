export interface Clinic {
  id?: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  cityId?: number;
  cityName?: string;
  isActive: boolean;
  appointmentGenerationDays?: number;
  createdAt?: Date;
}

export interface CreateClinicRequest {
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  cityId?: number;
  isActive?: boolean;
  appointmentGenerationDays?: number;
}

export interface UpdateClinicRequest {
  id: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  cityId?: number;
  isActive: boolean;
  appointmentGenerationDays?: number;
}

export interface ClinicSimple {
  id: number;
  name: string;
}
