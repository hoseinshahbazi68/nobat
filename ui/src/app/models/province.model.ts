export interface Province {
  id?: number;
  name: string;
  code?: string;
  createdAt?: string;
}

export interface CreateProvinceRequest {
  name: string;
  code?: string;
}

export interface UpdateProvinceRequest {
  id: number;
  name: string;
  code?: string;
}
