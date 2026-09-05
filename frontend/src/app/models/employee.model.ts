export enum EmploymentStatus {
  Active = 0,
  OnLeave = 1,
  Terminated = 2,
  Probation = 3
}

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  hireDate: string;
  jobTitle: string;
  salary: number;
  status: EmploymentStatus;
  departmentId: string;
  departmentName?: string;
  address?: string;
  createdAt: string;
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  hireDate: string;
  jobTitle: string;
  salary: number;
  departmentId: string;
  address?: string;
}

export interface UpdateEmployeeRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  jobTitle?: string;
  salary?: number;
  status?: EmploymentStatus;
  departmentId?: string;
  address?: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface DashboardKpis {
  totalEmployees: number;
  activeEmployees: number;
  onLeaveEmployees: number;
  averageSalary: number;
  departmentBreakdown: Record<string, number>;
}
