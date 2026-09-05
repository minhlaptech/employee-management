using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Services;

public interface IEmployeeService
{
    Task<PagedResponse<EmployeeResponse>> GetEmployeesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<EmployeeResponse> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeResponse> CreateEmployeeAsync(
        CreateEmployeeRequest request, 
        string? createdBy = null, 
        CancellationToken cancellationToken = default);

    Task<EmployeeResponse> UpdateEmployeeAsync(
        Guid id, 
        UpdateEmployeeRequest request, 
        string? updatedBy = null, 
        CancellationToken cancellationToken = default);

    Task DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DashboardKpiResponse> GetDashboardKpisAsync(CancellationToken cancellationToken = default);
}
