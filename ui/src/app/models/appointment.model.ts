export interface Appointment {
  id?: number;
  doctorScheduleId: number;
  doctorSchedule?: any;
  userId?: number;
  user?: any;
  patientName: string;
  patientPhone: string;
  patientEmail?: string;
  appointmentDateTime: string;
  status: AppointmentStatus;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export enum AppointmentStatus {
  Booked = 1,
  Cancelled = 2,
  Completed = 3
}

export interface CreateAppointmentRequest {
  doctorScheduleId: number;
  appointmentDateTime: string;
  expireDateTime: string;
  startTime: string;
  endTime: string;
}

export interface UpdateAppointmentRequest {
  id: number;
  status?: AppointmentStatus;
  notes?: string;
}
