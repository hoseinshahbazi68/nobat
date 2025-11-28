export interface UserActivityLog {
  id: number;
  userId?: number;
  username?: string;
  userFullName?: string;
  action: string;
  controller?: string;
  httpMethod?: string;
  requestPath?: string;
  ipAddress?: string;
  userAgent?: string;
  additionalData?: string;
  statusCode?: number;
  errorMessage?: string;
  activityTime: string;
  createdAt: string;
}
