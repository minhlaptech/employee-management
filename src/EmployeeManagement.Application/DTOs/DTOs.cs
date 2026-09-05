using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.DTOs;

// ─────────────────────────────────────────────
// Employee DTOs
// ─────────────────────────────────────────────

/// <summary>
/// Response DTO for employee data. Excludes sensitive fields like salary
/// unless the requester has appropriate permissions.
/// </summary>
public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    DateOnly HireDate,
    string JobTitle,
    EmploymentStatus Status,
    string StatusDisplay,
    Guid DepartmentId,
    string DepartmentName,
    string? ProfilePhotoUrl,
    int AnnualLeaveDaysRemaining,
    DateTime CreatedAt
);

/// <summary>
/// Request DTO for creating a new employee.
/// All required fields are validated at the API layer using FluentValidation.
/// </summary>
public record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    DateOnly HireDate,
    string JobTitle,
    decimal Salary,
    Guid DepartmentId,
    string? Address
);

/// <summary>
/// Request DTO for updating an existing employee.
/// Supports partial updates — only non-null fields are applied.
/// </summary>
public record UpdateEmployeeRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? JobTitle,
    decimal? Salary,
    EmploymentStatus? Status,
    Guid? DepartmentId,
    string? Address
);

// ─────────────────────────────────────────────
// Department DTOs
// ─────────────────────────────────────────────

public record DepartmentResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    int EmployeeCount,
    int MaxCapacity,
    string? ManagerName
);

public record CreateDepartmentRequest(
    string Name,
    string Code,
    string? Description,
    int MaxCapacity
);

// ─────────────────────────────────────────────
// Leave Request DTOs
// ─────────────────────────────────────────────

public record LeaveRequestResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    LeaveType Type,
    string TypeDisplay,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    string? Reason,
    LeaveStatus Status,
    string StatusDisplay,
    string? ReviewerComment,
    DateTime CreatedAt
);

public record CreateLeaveRequest(
    LeaveType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Reason
);

public record ReviewLeaveRequest(
    LeaveStatus Decision,
    string? Comment
);

// ─────────────────────────────────────────────
// Dashboard / Analytics DTOs
// ─────────────────────────────────────────────

public record DashboardKpiResponse(
    int TotalEmployees,
    int ActiveEmployees,
    int OnLeaveToday,
    int PendingLeaveRequests,
    int DepartmentCount,
    decimal AverageSalary,
    IReadOnlyList<DepartmentSummary> DepartmentBreakdown
);

public record DepartmentSummary(
    string DepartmentName,
    int EmployeeCount,
    int MaxCapacity,
    double OccupancyRate
);

// ─────────────────────────────────────────────
// Pagination
// ─────────────────────────────────────────────

/// <summary>
/// Generic paginated response wrapper.
/// Provides metadata for client-side pagination controls.
/// </summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
