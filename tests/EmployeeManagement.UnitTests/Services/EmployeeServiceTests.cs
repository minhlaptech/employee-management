using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Domain.Interfaces;
using Moq;

namespace EmployeeManagement.UnitTests.Services;

/// <summary>
/// Unit tests for EmployeeService.
/// Uses Moq for mocking IUnitOfWork and repositories.
/// Follows Arrange-Act-Assert pattern with descriptive test names.
/// </summary>
public class EmployeeServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
    private readonly Mock<IRepository<Department>> _mockDepartmentRepo;
    private readonly EmployeeService _sut; // System Under Test

    public EmployeeServiceTests()
    {
        _mockEmployeeRepo = new Mock<IEmployeeRepository>();
        _mockDepartmentRepo = new Mock<IRepository<Department>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepo.Object);
        _mockUnitOfWork.Setup(u => u.Departments).Returns(_mockDepartmentRepo.Object);

        _sut = new EmployeeService(_mockUnitOfWork.Object);
    }

    // ─────────────────────────────────────────────
    // GetEmployeeByIdAsync Tests
    // ─────────────────────────────────────────────

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenEmployeeExists_ReturnsEmployeeResponse()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var department = new Department { Id = Guid.NewGuid(), Name = "Engineering", Code = "ENG" };
        var employee = CreateTestEmployee(employeeId, department);

        _mockEmployeeRepo
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _sut.GetEmployeeByIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("Engineering", result.DepartmentName);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_WhenEmployeeNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockEmployeeRepo
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetEmployeeByIdAsync(nonExistentId));

        Assert.Contains(nonExistentId.ToString(), exception.Message);
    }

    // ─────────────────────────────────────────────
    // CreateEmployeeAsync Tests
    // ─────────────────────────────────────────────

    [Fact]
    public async Task CreateEmployeeAsync_WithValidData_CreatesAndReturnsEmployee()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var request = new CreateEmployeeRequest(
            FirstName: "Jane",
            LastName: "Smith",
            Email: "jane.smith@company.com",
            PhoneNumber: "+84901234567",
            DateOfBirth: new DateOnly(1995, 3, 15),
            HireDate: new DateOnly(2024, 1, 10),
            JobTitle: "Senior Angular Developer",
            Salary: 3500m,
            DepartmentId: departmentId,
            Address: "Ho Chi Minh City, Vietnam"
        );

        _mockEmployeeRepo
            .Setup(r => r.GetByEmailAsync("jane.smith@company.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null); // No duplicate

        _mockDepartmentRepo
            .Setup(r => r.ExistsAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockEmployeeRepo
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee e, CancellationToken _) => e);

        var department = new Department { Id = departmentId, Name = "Engineering", Code = "ENG" };
        _mockEmployeeRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateTestEmployee(id, department, "Jane", "Smith"));

        // Act
        var result = await _sut.CreateEmployeeAsync(request, "admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateEmployeeRequest(
            "John", "Doe", "existing@company.com", null,
            new DateOnly(1990, 1, 1), new DateOnly(2024, 1, 1),
            "Developer", 3000m, Guid.NewGuid(), null
        );

        _mockEmployeeRepo
            .Setup(r => r.GetByEmailAsync("existing@company.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Email = "existing@company.com" });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateEmployeeAsync(request));
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithNonExistentDepartment_ThrowsKeyNotFoundException()
    {
        // Arrange
        var fakeDeptId = Guid.NewGuid();
        var request = new CreateEmployeeRequest(
            "John", "Doe", "john@company.com", null,
            new DateOnly(1990, 1, 1), new DateOnly(2024, 1, 1),
            "Developer", 3000m, fakeDeptId, null
        );

        _mockEmployeeRepo
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        _mockDepartmentRepo
            .Setup(r => r.ExistsAsync(fakeDeptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateEmployeeAsync(request));
    }

    // ─────────────────────────────────────────────
    // DeleteEmployeeAsync Tests
    // ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteEmployeeAsync_WhenEmployeeExists_CallsDeleteAndSave()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockEmployeeRepo
            .Setup(r => r.ExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.DeleteEmployeeAsync(employeeId);

        // Assert
        _mockEmployeeRepo.Verify(r => r.DeleteAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─────────────────────────────────────────────
    // Helper Methods
    // ─────────────────────────────────────────────

    private static Employee CreateTestEmployee(
        Guid id, Department department,
        string firstName = "John", string lastName = "Doe")
    {
        return new Employee
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@company.com",
            DateOfBirth = new DateOnly(1990, 5, 15),
            HireDate = new DateOnly(2022, 3, 1),
            JobTitle = "Software Engineer",
            Salary = 3000m,
            Status = EmploymentStatus.Active,
            DepartmentId = department.Id,
            Department = department
        };
    }
}
