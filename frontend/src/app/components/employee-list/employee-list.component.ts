import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../../services/employee.service';
import { Employee, EmploymentStatus } from '../../models/employee.model';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="employee-list-container">
      <div class="header-actions">
        <h2>Team Members</h2>
        <div class="search-bar">
          <input
            type="text"
            [ngModel]="searchTerm()"
            (ngModelChange)="onSearchChange($event)"
            placeholder="Search by name, email, role..."
            class="input-search"
          />
        </div>
      </div>

      <div *ngIf="employeeService.isLoading()" class="loading-spinner">
        Loading employees...
      </div>

      <div class="table-card" *ngIf="!employeeService.isLoading()">
        <table class="data-table">
          <thead>
            <tr>
              <th>Employee</th>
              <th>Job Title</th>
              <th>Department</th>
              <th>Status</th>
              <th>Hire Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let emp of employeeService.employees()">
              <td class="user-cell">
                <div class="avatar-sm">{{ emp.firstName[0] }}{{ emp.lastName[0] }}</div>
                <div>
                  <strong>{{ emp.fullName }}</strong>
                  <span class="email-sub">{{ emp.email }}</span>
                </div>
              </td>
              <td>{{ emp.jobTitle }}</td>
              <td><span class="dept-badge">{{ emp.departmentName || 'General' }}</span></td>
              <td>
                <span class="status-pill" [ngClass]="getStatusClass(emp.status)">
                  {{ getStatusLabel(emp.status) }}
                </span>
              </td>
              <td>{{ emp.hireDate | date:'mediumDate' }}</td>
              <td class="action-buttons">
                <button class="btn-sm btn-edit" (click)="onEdit(emp)">Edit</button>
                <button class="btn-sm btn-delete" (click)="onDelete(emp.id)">Delete</button>
              </td>
            </tr>
            <tr *ngIf="employeeService.employees().length === 0">
              <td colspan="6" class="empty-state">No employees found.</td>
            </tr>
          </tbody>
        </table>

        <div class="pagination-bar">
          <span>Showing {{ employeeService.employees().length }} of {{ employeeService.totalCount() }}</span>
          <div class="page-controls">
            <button [disabled]="currentPage() <= 1" (click)="changePage(currentPage() - 1)">Previous</button>
            <span>Page {{ currentPage() }}</span>
            <button [disabled]="!hasNextPage()" (click)="changePage(currentPage() + 1)">Next</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .employee-list-container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .header-actions { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .input-search { padding: 10px 16px; border: 1px solid #cbd5e1; border-radius: 8px; width: 320px; font-size: 14px; }
    .table-card { background: white; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; }
    .data-table th, .data-table td { padding: 14px 18px; text-align: left; border-bottom: 1px solid #f1f5f9; font-size: 13.5px; }
    .data-table th { background: #f8fafc; font-weight: 600; color: #475569; }
    .user-cell { display: flex; align-items: center; gap: 12px; }
    .avatar-sm { width: 36px; height: 36px; border-radius: 50%; background: #2563eb; color: white; display: flex; align-items: center; justify-content: center; font-weight: 600; }
    .email-sub { display: block; font-size: 12px; color: #64748b; }
    .dept-badge { background: #eff6ff; color: #1d4ed8; padding: 3px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .status-pill { padding: 3px 10px; border-radius: 12px; font-size: 12px; font-weight: 600; }
    .status-active { background: #dcfce7; color: #15803d; }
    .status-leave { background: #fef9c3; color: #a16207; }
    .status-term { background: #fee2e2; color: #b91c1c; }
    .btn-sm { padding: 5px 10px; border-radius: 4px; border: none; cursor: pointer; font-size: 12px; margin-right: 6px; }
    .btn-edit { background: #e0e7ff; color: #4338ca; }
    .btn-delete { background: #fee2e2; color: #dc2626; }
    .pagination-bar { display: flex; justify-content: space-between; align-items: center; padding: 14px 20px; color: #64748b; font-size: 13px; }
    .page-controls button { padding: 6px 14px; border: 1px solid #cbd5e1; background: white; border-radius: 6px; cursor: pointer; margin: 0 6px; }
  `]
})
export class EmployeeListComponent implements OnInit {
  readonly employeeService = inject(EmployeeService);

  readonly searchTerm = signal<string>('');
  readonly currentPage = signal<number>(1);
  readonly pageSize = signal<number>(10);

  readonly hasNextPage = computed(() => {
    return this.currentPage() * this.pageSize() < this.employeeService.totalCount();
  });

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.employeeService.getEmployees(
      this.currentPage(),
      this.pageSize(),
      this.searchTerm()
    ).subscribe();
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1);
    this.loadEmployees();
  }

  changePage(page: number): void {
    this.currentPage.set(page);
    this.loadEmployees();
  }

  onEdit(emp: Employee): void {
    console.log('Editing employee:', emp);
  }

  onDelete(id: string): void {
    if (confirm('Are you sure you want to delete this employee?')) {
      this.employeeService.deleteEmployee(id).subscribe();
    }
  }

  getStatusLabel(status: EmploymentStatus): string {
    switch (status) {
      case EmploymentStatus.Active: return 'Active';
      case EmploymentStatus.OnLeave: return 'On Leave';
      case EmploymentStatus.Terminated: return 'Terminated';
      default: return 'Probation';
    }
  }

  getStatusClass(status: EmploymentStatus): string {
    switch (status) {
      case EmploymentStatus.Active: return 'status-active';
      case EmploymentStatus.OnLeave: return 'status-leave';
      case EmploymentStatus.Terminated: return 'status-term';
      default: return 'status-active';
    }
  }
}
