export interface Shift {
  id?: number;
  name: string;
  startTime: string;
  endTime: string;
  description?: string;
  createdAt?: string;
}

export interface CreateShiftRequest {
  name: string;
  startTime: string;
  endTime: string;
  description?: string;
}

export interface UpdateShiftRequest {
  id: number;
  name: string;
  startTime: string;
  endTime: string;
  description?: string;
}

