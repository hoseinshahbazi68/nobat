export interface Holiday {
  id?: number;
  date: string;
  name: string;
  description?: string;
}

export interface CreateHolidayRequest {
  date: string;
  name: string;
  description?: string;
}

export interface UpdateHolidayRequest {
  id: number;
  date: string;
  name: string;
  description?: string;
}

