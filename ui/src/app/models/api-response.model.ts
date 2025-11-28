export interface ApiResponse<T> {
  status: boolean;
  statusCode: number;
  message: string;
  description?: string;
  data?: T;
  errors?: string[];
}
