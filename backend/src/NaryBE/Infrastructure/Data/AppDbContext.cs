using System.Security.Claims;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AcademicSession> AcademicSessions => Set<AcademicSession>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseAllocation> CourseAllocations => Set<CourseAllocation>();
    public DbSet<AcademicMilestone> AcademicMilestones => Set<AcademicMilestone>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User indexes and constraints
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>();
            entity.HasOne(u => u.Department)
                  .WithMany(d => d.Users)
                  .HasForeignKey(u => u.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Advisor)
                  .WithMany()
                  .HasForeignKey(u => u.AdvisorId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(u => u.MatricNumber)
                  .IsUnique()
                  .HasFilter("\"MatricNumber\" IS NOT NULL");

            entity.HasIndex(u => u.StaffId)
                  .IsUnique()
                  .HasFilter("\"StaffId\" IS NOT NULL");
        });

        // Faculty indexes and constraints
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasIndex(f => f.Code).IsUnique();
            entity.HasMany(f => f.Departments)
                  .WithOne(d => d.Faculty)
                  .HasForeignKey(d => d.FacultyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Dean)
                  .WithMany()
                  .HasForeignKey(f => f.DeanUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Department indexes and constraints
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(d => d.Code).IsUnique();

            entity.HasOne(d => d.HeadOfDepartment)
                  .WithMany()
                  .HasForeignKey(d => d.HodUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(d => d.Courses)
                  .WithOne(c => c.Department)
                  .HasForeignKey(c => c.DepartmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Course indexes and constraints
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(c => c.Code).IsUnique();
        });

        // CourseAllocation
        modelBuilder.Entity<CourseAllocation>(entity =>
        {
            entity.HasOne(ca => ca.Course)
                  .WithMany(c => c.Allocations)
                  .HasForeignKey(ca => ca.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ca => ca.Lecturer)
                  .WithMany()
                  .HasForeignKey(ca => ca.LecturerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ca => ca.Session)
                  .WithMany(s => s.Allocations)
                  .HasForeignKey(ca => ca.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AcademicSession
        modelBuilder.Entity<AcademicSession>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
            entity.HasMany(s => s.Semesters)
                  .WithOne(sem => sem.Session)
                  .HasForeignKey(sem => sem.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Milestones)
                  .WithOne(m => m.Session)
                  .HasForeignKey(m => m.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.Timestamp);
            entity.HasIndex(a => a.UserId);
            entity.HasIndex(a => a.EntityName);
            entity.HasIndex(a => a.EntityId);
        });

        // Dynamic RBAC - Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
        });

        // Dynamic RBAC - RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasIndex(rp => new { rp.Role, rp.PermissionId }).IsUnique();
            entity.Property(rp => rp.Role).HasConversion<string>();
            entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(rp => rp.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        var pendingAudits = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .Select(e => new
            {
                Entry = e,
                Action = e.State switch
                {
                    EntityState.Added => $"Created {e.Entity.GetType().Name}",
                    EntityState.Modified => $"Updated {e.Entity.GetType().Name}",
                    EntityState.Deleted => $"Deleted {e.Entity.GetType().Name}",
                    _ => "Unknown"
                },
                EntityName = e.Entity.GetType().Name
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (pendingAudits.Count > 0)
        {
            var auditEntries = new List<AuditLog>();
            foreach (var item in pendingAudits)
            {
                var pkValue = GetPrimaryKeyValue(item.Entry);
                auditEntries.Add(new AuditLog
                {
                    UserId = currentUserId,
                    Action = item.Action,
                    EntityName = item.EntityName,
                    EntityId = pkValue ?? "N/A",
                    Timestamp = DateTime.UtcNow
                });
            }

            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var parsed) ? parsed : null;
    }

    private static string? GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return keyProp?.CurrentValue?.ToString();
    }
}
