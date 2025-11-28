export interface Appointment {
  id?: number;
  doctorId: number;
  doctor?: any;
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
  doctorId: number;
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
