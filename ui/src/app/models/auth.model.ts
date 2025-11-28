import { User } from './user.model';

export interface LoginRequest {
  nationalCode: string;
  password: string;
}

import { Gender } from './user.model';

export interface RegisterRequest {
  nationalCode: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  cityId?: number;
  gender: Gender; // الزامی
  birthDate?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}

export interface ApiResponse<T> {
  status: boolean;
  statusCode: number;
  message: string;
  data?: T;
  description?: string;
  errors?: string[];
}

export { User };
