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
        });

        // Faculty indexes and constraints
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasIndex(f => f.Code).IsUnique();
            entity.HasMany(f => f.Departments)
                  .WithOne(d => d.Faculty)
                  .HasForeignKey(d => d.FacultyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Department indexes and constraints
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(d => d.Code).IsUnique();
        });

        // AcademicSession
        modelBuilder.Entity<AcademicSession>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
            entity.HasMany(s => s.Semesters)
                  .WithOne(sem => sem.Session)
                  .HasForeignKey(sem => sem.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.Timestamp);
            entity.HasIndex(a => a.UserId);
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

