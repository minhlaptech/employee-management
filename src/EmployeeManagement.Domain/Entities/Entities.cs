using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

/// <summary>
/// Represents a department within the organization.
/// A department contains multiple employees and is led by a manager.
/// </summary>
public class Department : BaseEntity
{
    /// <summary>
    /// Department name (e.g., "Engineering", "Human Resources").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique department code for internal reference (e.g., "ENG", "HR").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the department's responsibilities.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional reference to the department manager.
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Maximum headcount allowed for this department.
    /// </summary>
    public int MaxCapacity { get; set; } = 50;

    // Navigation properties
    public Employee? Manager { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

/// <summary>
/// Core entity representing an employee in the organization.
/// Contains personal information, employment details, and department assignment.
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>
    /// Employee's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Corporate email address (unique constraint).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date of birth for age calculation and HR records.
    /// </summary>
    public DateOnly DateOfBirth { get; set; }

    /// <summary>
    /// Date when the employee joined the organization.
    /// </summary>
    public DateOnly HireDate { get; set; }

    /// <summary>
    /// Job title (e.g., "Senior Software Engineer", "Product Manager").
    /// </summary>
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Monthly base salary in USD.
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Current employment status.
    /// </summary>
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

    /// <summary>
    /// Foreign key to the assigned department.
    /// </summary>
    public Guid DepartmentId { get; set; }

    /// <summary>
    /// URL to the employee's profile photo (stored in blob/file system).
    /// </summary>
    public string? ProfilePhotoUrl { get; set; }

    /// <summary>
    /// Residential address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Remaining annual leave days for the current year.
    /// </summary>
    public int AnnualLeaveDaysRemaining { get; set; } = 12;

    // Computed property
    /// <summary>
    /// Full display name combining first and last name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
}

/// <summary>
/// Represents a leave/time-off request submitted by an employee.
/// Follows a workflow: Pending → Approved/Rejected → (optionally Cancelled).
/// </summary>
public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public LeaveType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ReviewerComment { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Calculates the total number of leave days requested.
    /// </summary>
    public int TotalDays => EndDate.DayNumber - StartDate.DayNumber + 1;

    // Navigation
    public Employee Employee { get; set; } = null!;
}

/// <summary>
/// Tracks daily attendance records for employees.
/// Records check-in/check-out times and calculates working hours.
/// </summary>
public class Attendance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Calculates total working hours based on check-in and check-out times.
    /// Returns null if either time is missing.
    /// </summary>
    public double? WorkingHours =>
        CheckInTime.HasValue && CheckOutTime.HasValue
            ? (CheckOutTime.Value.ToTimeSpan() - CheckInTime.Value.ToTimeSpan()).TotalHours
            : null;

    // Navigation
    public Employee Employee { get; set; } = null!;
}
