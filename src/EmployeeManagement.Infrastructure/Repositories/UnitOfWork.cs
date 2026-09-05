using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Persistence;

namespace EmployeeManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IEmployeeRepository? _employees;
    private IRepository<Department>? _departments;
    private IRepository<LeaveRequest>? _leaveRequests;
    private IRepository<Attendance>? _attendances;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IEmployeeRepository Employees =>
        _employees ??= new EmployeeRepository(_context);

    public IRepository<Department> Departments =>
        _departments ??= new GenericRepository<Department>(_context);

    public IRepository<LeaveRequest> LeaveRequests =>
        _leaveRequests ??= new GenericRepository<LeaveRequest>(_context);

    public IRepository<Attendance> Attendances =>
        _attendances ??= new GenericRepository<Attendance>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
