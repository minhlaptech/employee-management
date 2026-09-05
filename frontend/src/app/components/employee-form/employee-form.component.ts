import { Component, EventEmitter, Input, Output, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService } from '../../services/employee.service';
import { Employee, CreateEmployeeRequest } from '../../models/employee.model';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="form-card">
      <div class="form-header">
        <h3>{{ isEditMode ? 'Edit Employee' : 'New Team Member' }}</h3>
        <button class="close-btn" (click)="cancel.emit()">&times;</button>
      </div>

      <form [formGroup]="employeeForm" (ngSubmit)="onSubmit()">
        <div class="form-row">
          <div class="form-group">
            <label>First Name *</label>
            <input type="text" formControlName="firstName" class="form-control" />
            <span *ngIf="isInvalid('firstName')" class="err-msg">First name is required</span>
          </div>

          <div class="form-group">
            <label>Last Name *</label>
            <input type="text" formControlName="lastName" class="form-control" />
            <span *ngIf="isInvalid('lastName')" class="err-msg">Last name is required</span>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Work Email *</label>
            <input type="email" formControlName="email" class="form-control" />
            <span *ngIf="isInvalid('email')" class="err-msg">Valid work email required</span>
          </div>

          <div class="form-group">
            <label>Phone Number</label>
            <input type="text" formControlName="phoneNumber" class="form-control" />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Job Title *</label>
            <input type="text" formControlName="jobTitle" class="form-control" />
          </div>

          <div class="form-group">
            <label>Annual Salary ($) *</label>
            <input type="number" formControlName="salary" class="form-control" />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Hire Date *</label>
            <input type="date" formControlName="hireDate" class="form-control" />
          </div>

          <div class="form-group">
            <label>Department ID *</label>
            <input type="text" formControlName="departmentId" class="form-control" placeholder="GUID" />
          </div>
        </div>

        <div class="form-group">
          <label>Office / Home Address</label>
          <textarea formControlName="address" rows="2" class="form-control"></textarea>
        </div>

        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="cancel.emit()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="employeeForm.invalid || isSubmitting">
            {{ isSubmitting ? 'Saving...' : 'Save Employee' }}
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-card { background: white; border-radius: 12px; padding: 24px; box-shadow: 0 4px 16px rgba(0,0,0,0.08); max-width: 600px; margin: 0 auto; }
    .form-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid #f1f5f9; padding-bottom: 12px; }
    .close-btn { background: none; border: none; font-size: 24px; cursor: pointer; color: #94a3b8; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 14px; }
    .form-group { margin-bottom: 14px; }
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #334155; margin-bottom: 6px; }
    .form-control { width: 100%; padding: 10px 14px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 14px; box-sizing: border-box; }
    .form-control:focus { border-color: #2563eb; outline: none; }
    .err-msg { color: #dc2626; font-size: 12px; margin-top: 4px; display: block; }
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 24px; }
    .btn { padding: 10px 18px; border-radius: 6px; font-size: 13.5px; font-weight: 600; cursor: pointer; border: none; }
    .btn-primary { background: #2563eb; color: white; }
    .btn-secondary { background: #f1f5f9; color: #475569; }
  `]
})
export class EmployeeFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly employeeService = inject(EmployeeService);

  @Input() employeeToEdit: Employee | null = null;
  @Output() saved = new EventEmitter<Employee>();
  @Output() cancel = new EventEmitter<void>();

  employeeForm!: FormGroup;
  isEditMode = false;
  isSubmitting = false;

  ngOnInit(): void {
    this.isEditMode = !!this.employeeToEdit;

    this.employeeForm = this.fb.group({
      firstName: [this.employeeToEdit?.firstName || '', [Validators.required, Validators.maxLength(50)]],
      lastName: [this.employeeToEdit?.lastName || '', [Validators.required, Validators.maxLength(50)]],
      email: [this.employeeToEdit?.email || '', [Validators.required, Validators.email]],
      phoneNumber: [this.employeeToEdit?.phoneNumber || '', [Validators.maxLength(20)]],
      jobTitle: [this.employeeToEdit?.jobTitle || '', [Validators.required, Validators.maxLength(100)]],
      salary: [this.employeeToEdit?.salary || 50000, [Validators.required, Validators.min(0)]],
      hireDate: [this.employeeToEdit?.hireDate?.split('T')[0] || new Date().toISOString().split('T')[0], [Validators.required]],
      departmentId: [this.employeeToEdit?.departmentId || '00000000-0000-0000-0000-000000000001', [Validators.required]],
      address: [this.employeeToEdit?.address || '', [Validators.maxLength(200)]]
    });
  }

  isInvalid(controlName: string): boolean {
    const control = this.employeeForm.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  onSubmit(): void {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.employeeForm.value as CreateEmployeeRequest;

    if (this.isEditMode && this.employeeToEdit) {
      this.employeeService.updateEmployee(this.employeeToEdit.id, formValue).subscribe({
        next: (updated) => {
          this.isSubmitting = false;
          this.saved.emit(updated);
        },
        error: () => (this.isSubmitting = false)
      });
    } else {
      this.employeeService.createEmployee(formValue).subscribe({
        next: (created) => {
          this.isSubmitting = false;
          this.saved.emit(created);
        },
        error: () => (this.isSubmitting = false)
      });
    }
  }
}
