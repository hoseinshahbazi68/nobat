export interface Service {
  id?: number;
  name: string;
  description?: string;
}

export interface CreateServiceRequest {
  name: string;
  description?: string;
}

export interface UpdateServiceRequest {
  id: number;
  name: string;
  description?: string;
}
