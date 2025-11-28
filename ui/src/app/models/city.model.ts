export interface City {
  id?: number;
  name: string;
  provinceId: number;
  provinceName?: string;
  code?: string;
  createdAt?: string;
}

export interface CreateCityRequest {
  name: string;
  provinceId: number;
  code?: string;
}

export interface UpdateCityRequest {
  id: number;
  name: string;
  provinceId: number;
  code?: string;
}
