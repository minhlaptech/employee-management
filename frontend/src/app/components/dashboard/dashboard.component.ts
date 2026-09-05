import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeService } from '../../services/employee.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Enterprise Overview & Analytics</h1>
        <p class="sub-text">Real-time workforce metrics and payroll distribution</p>
      </div>

      <!-- KPI Cards -->
      <div class="kpi-grid">
        <div class="kpi-card glass">
          <div class="kpi-icon icon-blue">👥</div>
          <div>
            <span class="kpi-label">Total Headcount</span>
            <h2 class="kpi-value">{{ kpis()?.totalEmployees || 0 }}</h2>
            <span class="kpi-trend trend-up">Active Workforce</span>
          </div>
        </div>

        <div class="kpi-card glass">
          <div class="kpi-icon icon-green">⚡</div>
          <div>
            <span class="kpi-label">Active Employees</span>
            <h2 class="kpi-value">{{ kpis()?.activeEmployees || 0 }}</h2>
            <span class="kpi-trend trend-green">Available Today</span>
          </div>
        </div>

        <div class="kpi-card glass">
          <div class="kpi-icon icon-amber">🏖️</div>
          <div>
            <span class="kpi-label">On Leave</span>
            <h2 class="kpi-value">{{ kpis()?.onLeaveEmployees || 0 }}</h2>
            <span class="kpi-trend">Approved Time-off</span>
          </div>
        </div>

        <div class="kpi-card glass">
          <div class="kpi-icon icon-purple">💰</div>
          <div>
            <span class="kpi-label">Average Salary</span>
            <h2 class="kpi-value">\${{ (kpis()?.averageSalary || 0) | number:'1.0-0' }}</h2>
            <span class="kpi-trend">Annualized Benchmark</span>
          </div>
        </div>
      </div>

      <!-- Department Distribution Panel -->
      <div class="analytics-card glass">
        <h3>Department Breakdown</h3>
        <div class="dept-list">
          <div *ngFor="let item of departmentEntries()" class="dept-row">
            <span class="dept-name">{{ item[0] }}</span>
            <div class="progress-track">
              <div class="progress-fill" [style.width.%]="(item[1] / (kpis()?.totalEmployees || 1)) * 100"></div>
            </div>
            <span class="dept-count">{{ item[1] }} members</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container { padding: 32px; max-width: 1200px; margin: 0 auto; }
    .dashboard-header h1 { font-size: 26px; color: #0f172a; margin-bottom: 4px; }
    .sub-text { color: #64748b; font-size: 14px; margin-bottom: 24px; }
    .kpi-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 20px; margin-bottom: 32px; }
    .kpi-card { background: white; border-radius: 12px; padding: 20px; display: flex; align-items: center; gap: 18px; box-shadow: 0 4px 16px rgba(0,0,0,0.06); }
    .kpi-icon { width: 52px; height: 52px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 24px; }
    .icon-blue { background: #eff6ff; }
    .icon-green { background: #dcfce7; }
    .icon-amber { background: #fef3c7; }
    .icon-purple { background: #f3e8ff; }
    .kpi-label { font-size: 12.5px; color: #64748b; font-weight: 500; }
    .kpi-value { font-size: 28px; font-weight: 700; color: #0f172a; margin: 4px 0; }
    .kpi-trend { font-size: 11.5px; font-weight: 600; color: #3b82f6; }
    .trend-green { color: #16a34a; }
    .analytics-card { background: white; border-radius: 12px; padding: 24px; box-shadow: 0 4px 16px rgba(0,0,0,0.06); }
    .analytics-card h3 { font-size: 16px; margin-bottom: 18px; color: #0f172a; }
    .dept-row { display: grid; grid-template-columns: 140px 1fr 100px; align-items: center; gap: 16px; margin-bottom: 14px; }
    .dept-name { font-size: 13.5px; font-weight: 500; color: #334155; }
    .progress-track { background: #f1f5f9; border-radius: 6px; height: 8px; overflow: hidden; }
    .progress-fill { background: #2563eb; height: 100%; border-radius: 6px; }
    .dept-count { font-size: 12.5px; color: #64748b; text-align: right; }
  `]
})
export class DashboardComponent implements OnInit {
  private readonly employeeService = inject(EmployeeService);
  readonly kpis = this.employeeService.dashboardKpis;

  ngOnInit(): void {
    this.employeeService.getDashboardKpis().subscribe();
  }

  departmentEntries(): [string, number][] {
    const raw = this.kpis()?.departmentBreakdown;
    return raw ? Object.entries(raw) : [];
  }
}
