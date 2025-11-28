export interface MedicalCondition {
  id?: number;
  name: string;
  description?: string;
  createdAt?: string;
}

export interface CreateMedicalConditionRequest {
  name: string;
  description?: string;
}

export interface UpdateMedicalConditionRequest {
  id: number;
  name: string;
  description?: string;
}

export interface DoctorMedicalCondition {
  id?: number;
  doctorId: number;
  medicalConditionId: number;
  medicalCondition?: MedicalCondition;
}
