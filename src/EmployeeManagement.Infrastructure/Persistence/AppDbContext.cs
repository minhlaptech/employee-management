using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Employee Management System.
/// Applies global query filters for soft-delete and configures entity relationships.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Attendance> Attendances => Set<Attendance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter: exclude soft-deleted records from all queries
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(lr => !lr.IsDeleted);
        modelBuilder.Entity<Attendance>().HasQueryFilter(a => !a.IsDeleted);
    }

    /// <summary>
    /// Automatically updates audit timestamps on SaveChanges.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

// ─────────────────────────────────────────────
// Entity Configurations (Fluent API)
// ─────────────────────────────────────────────

/// <summary>
/// EF Core configuration for Employee entity.
/// Defines column types, constraints, indexes, and relationships.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(e => e.JobTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Salary)
            .HasPrecision(18, 2);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.ProfilePhotoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Indexes for common query patterns
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.LastName, e.FirstName }); // Name search optimization

        // Computed column (not mapped to DB)
        builder.Ignore(e => e.FullName);

        // Relationships
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.LeaveRequests)
            .WithOne(lr => lr.Employee)
            .HasForeignKey(lr => lr.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.AttendanceRecords)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for Department entity.
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.HasIndex(d => d.Code).IsUnique();

        // Self-referencing: department manager is an employee
        builder.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed initial departments
        builder.HasData(
            new Department { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), Name = "Engineering", Code = "ENG", Description = "Software development and technical operations", MaxCapacity = 50 },
            new Department { Id = Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"), Name = "Human Resources", Code = "HR", Description = "People operations and talent management", MaxCapacity = 15 },
            new Department { Id = Guid.Parse("a1b2c3d4-0003-0000-0000-000000000003"), Name = "Finance", Code = "FIN", Description = "Financial planning and accounting", MaxCapacity = 20 },
            new Department { Id = Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"), Name = "Marketing", Code = "MKT", Description = "Brand strategy and growth marketing", MaxCapacity = 25 }
        );
    }
}

/// <summary>
/// EF Core configuration for LeaveRequest entity.
/// </summary>
public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Reason).HasMaxLength(1000);
        builder.Property(lr => lr.ReviewerComment).HasMaxLength(500);

        builder.Property(lr => lr.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(lr => lr.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(lr => lr.TotalDays);

        builder.HasIndex(lr => lr.EmployeeId);
        builder.HasIndex(lr => lr.Status);
        builder.HasIndex(lr => new { lr.StartDate, lr.EndDate });
    }
}
