using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// Application service for employee business operations.
/// Orchestrates domain logic, data access, and DTO mapping.
/// Follows the Service Layer pattern — thin controllers delegate here.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Retrieves a paginated list of employees with optional search and department filtering.
    /// Supports server-side pagination for efficient handling of large datasets.
    /// </summary>
    public async Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        // Clamp values to prevent abuse
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (employees, totalCount) = await _unitOfWork.Employees
            .GetPagedAsync(pageNumber, pageSize, searchTerm, departmentId, cancellationToken);

        var items = employees.Select(MapToResponse).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<EmployeeResponse>(items, pageNumber, pageSize, totalCount, totalPages);
    }

    /// <summary>
    /// Retrieves a single employee by ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when no employee matches the given ID.</exception>
    public async Task<EmployeeResponse> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID '{id}' was not found.");

        return MapToResponse(employee);
    }

    /// <summary>
    /// Creates a new employee record after validating business rules.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when email is already in use.</exception>
    public async Task<EmployeeResponse> CreateEmployeeAsync(
        CreateEmployeeRequest request,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        // Business rule: Email must be unique
        var existingEmployee = await _unitOfWork.Employees.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmployee is not null)
        {
            throw new InvalidOperationException($"An employee with email '{request.Email}' already exists.");
        }

        // Business rule: Department must exist
        var departmentExists = await _unitOfWork.Departments.ExistsAsync(request.DepartmentId, cancellationToken);
        if (!departmentExists)
        {
            throw new KeyNotFoundException($"Department with ID '{request.DepartmentId}' was not found.");
        }

        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            DateOfBirth = request.DateOfBirth,
            HireDate = request.HireDate,
            JobTitle = request.JobTitle.Trim(),
            Salary = request.Salary,
            DepartmentId = request.DepartmentId,
            Address = request.Address?.Trim(),
            Status = EmploymentStatus.Active,
            CreatedBy = createdBy
        };

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var created = await _unitOfWork.Employees.GetByIdAsync(employee.Id, cancellationToken);
        return MapToResponse(created!);
    }

    /// <summary>
    /// Updates an existing employee. Supports partial updates —
    /// only fields with non-null values in the request are applied.
    /// </summary>
    public async Task<EmployeeResponse> UpdateEmployeeAsync(
        Guid id,
        UpdateEmployeeRequest request,
        string? updatedBy = null,
        CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee with ID '{id}' was not found.");

        // Apply partial updates (null = no change)
        if (request.FirstName is not null) employee.FirstName = request.FirstName.Trim();
        if (request.LastName is not null) employee.LastName = request.LastName.Trim();
        if (request.PhoneNumber is not null) employee.PhoneNumber = request.PhoneNumber.Trim();
        if (request.JobTitle is not null) employee.JobTitle = request.JobTitle.Trim();
        if (request.Salary.HasValue) employee.Salary = request.Salary.Value;
        if (request.Status.HasValue) employee.Status = request.Status.Value;
        if (request.Address is not null) employee.Address = request.Address.Trim();

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _unitOfWork.Departments.ExistsAsync(request.DepartmentId.Value, cancellationToken);
            if (!deptExists) throw new KeyNotFoundException($"Department '{request.DepartmentId}' not found.");
            employee.DepartmentId = request.DepartmentId.Value;
        }

        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = updatedBy;

        await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
        return MapToResponse(updated!);
    }

    /// <summary>
    /// Soft-deletes an employee by marking IsDeleted = true.
    /// The record is preserved for audit purposes but excluded from active queries.
    /// </summary>
    public async Task DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Employees.ExistsAsync(id, cancellationToken);
        if (!exists) throw new KeyNotFoundException($"Employee with ID '{id}' was not found.");

        await _unitOfWork.Employees.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Generates dashboard KPI data for the management overview.
    /// </summary>
    public async Task<DashboardKpiResponse> GetDashboardKpisAsync(CancellationToken cancellationToken = default)
    {
        var allEmployees = await _unitOfWork.Employees.GetAllAsync(cancellationToken);
        var departments = await _unitOfWork.Departments.GetAllAsync(cancellationToken);
        var leaveRequests = await _unitOfWork.LeaveRequests.GetAllAsync(cancellationToken);

        var activeEmployees = allEmployees.Where(e => e.Status == EmploymentStatus.Active).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var onLeaveToday = leaveRequests.Count(lr =>
            lr.Status == LeaveStatus.Approved &&
            lr.StartDate <= today &&
            lr.EndDate >= today);

        var pendingRequests = leaveRequests.Count(lr => lr.Status == LeaveStatus.Pending);

        var departmentBreakdown = departments.Select(d =>
        {
            var count = allEmployees.Count(e => e.DepartmentId == d.Id);
            return new DepartmentSummary(
                d.Name,
                count,
                d.MaxCapacity,
                d.MaxCapacity > 0 ? Math.Round((double)count / d.MaxCapacity * 100, 1) : 0
            );
        }).ToList();

        return new DashboardKpiResponse(
            TotalEmployees: allEmployees.Count,
            ActiveEmployees: activeEmployees.Count,
            OnLeaveToday: onLeaveToday,
            PendingLeaveRequests: pendingRequests,
            DepartmentCount: departments.Count,
            AverageSalary: activeEmployees.Any() ? Math.Round(activeEmployees.Average(e => e.Salary), 2) : 0,
            DepartmentBreakdown: departmentBreakdown
        );
    }

    // ─────────────────────────────────────────────
    // Private Mapping Methods
    // ─────────────────────────────────────────────

    private static EmployeeResponse MapToResponse(Employee employee) => new(
        Id: employee.Id,
        FirstName: employee.FirstName,
        LastName: employee.LastName,
        FullName: employee.FullName,
        Email: employee.Email,
        PhoneNumber: employee.PhoneNumber,
        DateOfBirth: employee.DateOfBirth,
        HireDate: employee.HireDate,
        JobTitle: employee.JobTitle,
        Status: employee.Status,
        StatusDisplay: employee.Status.ToString(),
        DepartmentId: employee.DepartmentId,
        DepartmentName: employee.Department?.Name ?? "Unassigned",
        ProfilePhotoUrl: employee.ProfilePhotoUrl,
        AnnualLeaveDaysRemaining: employee.AnnualLeaveDaysRemaining,
        CreatedAt: employee.CreatedAt
    );
}
