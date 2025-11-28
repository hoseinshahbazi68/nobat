export interface QueryLog {
  id: number;
  userId?: number;
  queryText: string;
  parameters?: string;
  executionTimeMs: number;
  executionTime: string;
  commandType?: string;
  tablesUsed?: string;
  ipAddress?: string;
  controllerAction?: string;
  isHeavy: boolean;
  errorMessage?: string;
  createdAt: string;
}

export interface QueryLogStatistics {
  totalHeavyQueries: number;
  averageExecutionTimeMs: number;
  maxExecutionTimeMs: number;
  minExecutionTimeMs: number;
  errorCount: number;
  commandTypeCounts: { [key: string]: number };
  tableUsageCounts: { [key: string]: number };
}

export interface QueryLogQueryParams {
  page?: number;
  pageSize?: number;
  filters?: string;
  sorts?: string;
}
