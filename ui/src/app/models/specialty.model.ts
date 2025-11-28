export interface Specialty {
  id?: number;
  name: string;
  description?: string;
}

export interface CreateSpecialtyRequest {
  name: string;
  description?: string;
}

export interface UpdateSpecialtyRequest {
  id: number;
  name: string;
  description?: string;
}

export interface SpecialtyMedicalCondition {
  id?: number;
  specialtyId: number;
  medicalConditionId: number;
  medicalCondition?: {
    id?: number;
    name: string;
    description?: string;
  };
}
