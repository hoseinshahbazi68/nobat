export interface ServiceTariff {
  id?: number;
  serviceId: number;
  serviceName?: string;
  insuranceId: number;
  insuranceName?: string;
  doctorId?: number;
  doctorName?: string;
  clinicId: number;
  clinicName?: string;
  price: number;
  finalPrice?: number;
  visitDuration?: number;
}

export interface CreateServiceTariffRequest {
  serviceId: number;
  insuranceId: number;
  doctorId?: number;
  clinicId: number;
  price: number;
  visitDuration?: number;
}

export interface UpdateServiceTariffRequest {
  id: number;
  serviceId: number;
  insuranceId: number;
  doctorId?: number;
  clinicId: number;
  price: number;
  visitDuration?: number;
}
