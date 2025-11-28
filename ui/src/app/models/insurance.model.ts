export interface Insurance {
  id?: number;
  name: string;
  code: string;
  isActive: boolean;
}

export interface CreateInsuranceRequest {
  name: string;
  code: string;
  isActive: boolean;
}

export interface UpdateInsuranceRequest {
  id: number;
  name: string;
  code: string;
  isActive: boolean;
}
