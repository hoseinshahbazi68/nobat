export interface DatabaseChangeLog {
  id: number;
  userId?: number;
  tableName: string;
  recordId: string;
  changeType: string;
  changedColumns?: string;
  oldValues?: string;
  newValues?: string;
  changeTime: string;
  ipAddress?: string;
  additionalData?: string;
  createdAt: string;
}

export interface DatabaseChangeLogQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}
