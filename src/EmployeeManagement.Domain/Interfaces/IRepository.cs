using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Domain.Interfaces;

/// <summary>
/// Generic repository interface following the Repository pattern.
/// Provides a consistent abstraction layer over data access operations.
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Extended repository interface for Employee-specific queries.
/// Provides optimized methods for common employee data access patterns.
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work pattern interface for managing transactional boundaries.
/// Ensures all repository operations within a business transaction
/// are committed or rolled back atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IRepository<Department> Departments { get; }
    IRepository<LeaveRequest> LeaveRequests { get; }
    IRepository<Attendance> Attendances { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
