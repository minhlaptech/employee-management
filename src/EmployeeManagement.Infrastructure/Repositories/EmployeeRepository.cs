using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Persistence;

namespace EmployeeManagement.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Include(e => e.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                (e.JobTitle != null && e.JobTitle.ToLower().Contains(term)));
        }

        if (departmentId.HasValue && departmentId.Value != Guid.Empty)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
