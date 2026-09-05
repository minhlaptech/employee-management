namespace EmployeeManagement.Domain.Enums;

/// <summary>
/// Represents the employment status of an employee.
/// </summary>
public enum EmploymentStatus
{
    Active = 1,
    OnLeave = 2,
    Suspended = 3,
    Terminated = 4
}

/// <summary>
/// Represents available leave request types.
/// </summary>
public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Personal = 3,
    Maternity = 4,
    Paternity = 5,
    Unpaid = 6
}

/// <summary>
/// Represents the approval status of a leave request.
/// </summary>
public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

/// <summary>
/// User roles for RBAC authorization.
/// </summary>
public enum UserRole
{
    Employee = 1,
    Manager = 2,
    Admin = 3
}
