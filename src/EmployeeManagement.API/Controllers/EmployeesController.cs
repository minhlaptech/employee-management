using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

/// <summary>
/// RESTful API controller for employee management operations.
/// Supports CRUD, search, pagination, and dashboard analytics.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(EmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of employees.
    /// Supports search by name/email and filtering by department.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="search">Optional search term for name or email</param>
    /// <param name="departmentId">Optional department filter</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.GetEmployeesAsync(
            pageNumber, pageSize, search, departmentId, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single employee by their unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
            return Ok(employee);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new employee record.
    /// Requires Admin or Manager role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = User.Identity?.Name ?? "system";
            var employee = await _employeeService.CreateEmployeeAsync(request, createdBy, cancellationToken);

            _logger.LogInformation("Employee created: {EmployeeId} by {CreatedBy}", employee.Id, createdBy);

            return CreatedAtAction(
                nameof(GetEmployee),
                new { id = employee.Id },
                employee);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing employee. Supports partial updates.
    /// Requires Admin or Manager role.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmployee(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedBy = User.Identity?.Name ?? "system";
            var employee = await _employeeService.UpdateEmployeeAsync(id, request, updatedBy, cancellationToken);

            _logger.LogInformation("Employee updated: {EmployeeId} by {UpdatedBy}", id, updatedBy);

            return Ok(employee);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft-deletes an employee (preserves audit trail).
    /// Requires Admin role only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _employeeService.DeleteEmployeeAsync(id, cancellationToken);

            _logger.LogInformation("Employee soft-deleted: {EmployeeId}", id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves dashboard KPI data for the management overview page.
    /// Includes employee counts, salary averages, and department breakdown.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardKpiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var kpis = await _employeeService.GetDashboardKpisAsync(cancellationToken);
        return Ok(kpis);
    }
}
