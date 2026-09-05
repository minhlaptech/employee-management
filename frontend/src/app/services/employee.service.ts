import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Employee, CreateEmployeeRequest, UpdateEmployeeRequest, PagedResponse, DashboardKpis } from '../models/employee.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/employees';

  // Angular 17 Signals for reactive state management
  readonly employees = signal<Employee[]>([]);
  readonly currentEmployee = signal<Employee | null>(null);
  readonly dashboardKpis = signal<DashboardKpis | null>(null);
  readonly isLoading = signal<boolean>(false);
  readonly totalCount = signal<number>(0);

  getEmployees(
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    departmentId?: string
  ): Observable<PagedResponse<Employee>> {
    this.isLoading.set(true);

    let params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('searchTerm', search);
    if (departmentId) params = params.set('departmentId', departmentId);

    return this.http.get<PagedResponse<Employee>>(this.apiUrl, { params }).pipe(
      tap({
        next: (res) => {
          this.employees.set(res.items);
          this.totalCount.set(res.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      })
    );
  }

  getEmployeeById(id: string): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`).pipe(
      tap((emp) => this.currentEmployee.set(emp))
    );
  }

  createEmployee(payload: CreateEmployeeRequest): Observable<Employee> {
    return this.http.post<Employee>(this.apiUrl, payload).pipe(
      tap((created) => {
        this.employees.update((list) => [created, ...list]);
        this.totalCount.update((c) => c + 1);
      })
    );
  }

  updateEmployee(id: string, payload: UpdateEmployeeRequest): Observable<Employee> {
    return this.http.put<Employee>(`${this.apiUrl}/${id}`, payload).pipe(
      tap((updated) => {
        this.employees.update((list) =>
          list.map((emp) => (emp.id === id ? updated : emp))
        );
      })
    );
  }

  deleteEmployee(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        this.employees.update((list) => list.filter((emp) => emp.id !== id));
        this.totalCount.update((c) => Math.max(0, c - 1));
      })
    );
  }

  getDashboardKpis(): Observable<DashboardKpis> {
    return this.http.get<DashboardKpis>(`${this.apiUrl}/dashboard/kpis`).pipe(
      tap((kpis) => this.dashboardKpis.set(kpis))
    );
  }
}
