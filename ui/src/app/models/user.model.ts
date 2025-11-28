export enum Gender {
  Unknown = 0,
  Male = 1,
  Female = 2
}

export interface User {
  id: number;
  nationalCode: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  gender?: Gender;
  birthDate?: string;
  cityId?: number;
  cityName?: string;
  provinceName?: string;
  isActive: boolean;
  roles: string[];
  profilePicture?: string;
}

export interface CreateUserRequest {
  nationalCode: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  gender?: Gender;
  birthDate?: string;
  cityId?: number;
  roleIds?: number[];
}

export interface UpdateUserRequest {
  id: number;
  nationalCode: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  gender?: Gender;
  birthDate?: string;
  cityId?: number;
  isActive: boolean;
  roleIds?: number[];
}

export interface UpdateProfileRequest {
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  gender: Gender; // الزامی
  birthDate?: string;
  cityId?: number;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
