using Core.Entities;
using Core.Enums;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        // 1. Seed Faculties & Departments
        if (!await context.Faculties.AnyAsync())
        {
            logger.LogInformation("Seeding faculties and departments...");

            var fcas = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Computing & Applied Sciences",
                Code = "FCAS",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var feng = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Engineering",
                Code = "FENG",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var fmss = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Management & Social Sciences",
                Code = "FMSS",
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            };

            var fes = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Environmental Sciences",
                Code = "FES",
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };

            context.Faculties.AddRange(fcas, feng, fmss, fes);
            await context.SaveChangesAsync();

            // Departments
            var departments = new List<Department>
            {
                // Computing
                new() { Id = Guid.NewGuid(), Name = "Department of Computer Science", Code = "CSC", FacultyId = fcas.Id, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Software Engineering", Code = "SEN", FacultyId = fcas.Id, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Cybersecurity", Code = "CYB", FacultyId = fcas.Id, CreatedAt = DateTime.UtcNow.AddMonths(-5) },

                // Engineering
                new() { Id = Guid.NewGuid(), Name = "Department of Electrical & Electronics Engineering", Code = "EEE", FacultyId = feng.Id, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Mechanical Engineering", Code = "MEE", FacultyId = feng.Id, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Civil Engineering", Code = "CVE", FacultyId = feng.Id, CreatedAt = DateTime.UtcNow.AddMonths(-5) },

                // Management & Social Sciences
                new() { Id = Guid.NewGuid(), Name = "Department of Business Administration", Code = "BUS", FacultyId = fmss.Id, CreatedAt = DateTime.UtcNow.AddMonths(-5) },
                new() { Id = Guid.NewGuid(), Name = "Department of Accounting", Code = "ACC", FacultyId = fmss.Id, CreatedAt = DateTime.UtcNow.AddMonths(-5) },
                new() { Id = Guid.NewGuid(), Name = "Department of Economics", Code = "ECO", FacultyId = fmss.Id, CreatedAt = DateTime.UtcNow.AddMonths(-4) },

                // Environmental Sciences
                new() { Id = Guid.NewGuid(), Name = "Department of Architecture", Code = "ARC", FacultyId = fes.Id, CreatedAt = DateTime.UtcNow.AddMonths(-4) }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
            logger.LogInformation("Faculties and Departments seeded successfully.");
        }

        // 2. Seed Academic Sessions & Semesters
        if (!await context.AcademicSessions.AnyAsync())
        {
            logger.LogInformation("Seeding academic sessions and semesters...");

            var pastSession = new AcademicSession
            {
                Id = Guid.NewGuid(),
                Name = "2025/2026",
                StartDate = DateTime.SpecifyKind(new DateTime(2025, 9, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(2026, 7, 31), DateTimeKind.Utc),
                IsActive = false
            };

            var currentSession = new AcademicSession
            {
                Id = Guid.NewGuid(),
                Name = "2026/2027",
                StartDate = DateTime.SpecifyKind(new DateTime(2026, 9, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(2027, 7, 31), DateTimeKind.Utc),
                IsActive = true
            };

            context.AcademicSessions.AddRange(pastSession, currentSession);
            await context.SaveChangesAsync();

            var semesters = new List<Semester>
            {
                new() { Id = Guid.NewGuid(), Name = "First Semester", SessionId = pastSession.Id, IsActive = false },
                new() { Id = Guid.NewGuid(), Name = "Second Semester", SessionId = pastSession.Id, IsActive = false },
                new() { Id = Guid.NewGuid(), Name = "First Semester", SessionId = currentSession.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Name = "Second Semester", SessionId = currentSession.Id, IsActive = false }
            };

            context.Semesters.AddRange(semesters);
            await context.SaveChangesAsync();
            logger.LogInformation("Academic Sessions and Semesters seeded successfully.");
        }

        // 3. Seed Users across all UserRoles
        var cscDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "CSC");
        var senDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "SEN");
        var cybDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "CYB");
        var eeeDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "EEE");
        var meeDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "MEE");
        var busDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "BUS");
        var accDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "ACC");
        var ecoDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "ECO");

        // Helper to seed a user if missing
        async Task SeedUserIfNotExists(string email, string password, UserRole role, Guid? departmentId, bool isActive = true)
        {
            var exists = await context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
            if (!exists)
            {
                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = email.ToLower(),
                    PasswordHash = passwordHasher.HashPassword(password),
                    Role = role,
                    DepartmentId = departmentId,
                    IsActive = isActive,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
                });
            }
        }

        logger.LogInformation("Seeding users for all UserRole types...");

        // SuperAdmin
        await SeedUserIfNotExists("superadmin@nary.edu", "SuperAdmin123!", UserRole.SuperAdmin, null);
        await SeedUserIfNotExists("assistant.superadmin@nary.edu", "SuperAdmin123!", UserRole.SuperAdmin, null);

        // DeptAdmin
        if (cscDept != null) await SeedUserIfNotExists("cs.admin@nary.edu", "Admin123!", UserRole.DeptAdmin, cscDept.Id);
        if (eeeDept != null) await SeedUserIfNotExists("eng.admin@nary.edu", "Admin123!", UserRole.DeptAdmin, eeeDept.Id);
        if (busDept != null) await SeedUserIfNotExists("bus.admin@nary.edu", "Admin123!", UserRole.DeptAdmin, busDept.Id);
        // Unassigned admin to test "Assign Department" feature in UI
        await SeedUserIfNotExists("unassigned.admin@nary.edu", "Admin123!", UserRole.DeptAdmin, null);

        // Lecturer
        if (cscDept != null) await SeedUserIfNotExists("prof.turing@nary.edu", "Lecturer123!", UserRole.Lecturer, cscDept.Id);
        if (senDept != null) await SeedUserIfNotExists("dr.hopper@nary.edu", "Lecturer123!", UserRole.Lecturer, senDept.Id);
        if (eeeDept != null) await SeedUserIfNotExists("dr.tesla@nary.edu", "Lecturer123!", UserRole.Lecturer, eeeDept.Id);
        if (ecoDept != null) await SeedUserIfNotExists("dr.smith@nary.edu", "Lecturer123!", UserRole.Lecturer, ecoDept.Id);
        // Inactive lecturer to test account reactivation in UI
        if (cybDept != null) await SeedUserIfNotExists("prof.curie@nary.edu", "Lecturer123!", UserRole.Lecturer, cybDept.Id, isActive: false);

        // Student
        if (cscDept != null) await SeedUserIfNotExists("alice.johnson@nary.edu", "Student123!", UserRole.Student, cscDept.Id);
        if (senDept != null) await SeedUserIfNotExists("bob.smith@nary.edu", "Student123!", UserRole.Student, senDept.Id);
        if (cybDept != null) await SeedUserIfNotExists("charlie.brown@nary.edu", "Student123!", UserRole.Student, cybDept.Id);
        if (eeeDept != null) await SeedUserIfNotExists("diana.prince@nary.edu", "Student123!", UserRole.Student, eeeDept.Id);
        if (meeDept != null) await SeedUserIfNotExists("edward.elric@nary.edu", "Student123!", UserRole.Student, meeDept.Id);
        if (busDept != null) await SeedUserIfNotExists("fiona.gallagher@nary.edu", "Student123!", UserRole.Student, busDept.Id);
        // Inactive student to test account suspension toggle in UI
        if (accDept != null) await SeedUserIfNotExists("george.orwell@nary.edu", "Student123!", UserRole.Student, accDept.Id, isActive: false);
        // Student with no assigned department yet
        await SeedUserIfNotExists("freshman.student@nary.edu", "Student123!", UserRole.Student, null);

        await context.SaveChangesAsync();
        logger.LogInformation("All users seeded successfully.");

        // 4. Seed Dynamic RBAC Permissions & Role-Permission Matrix
        if (!await context.Permissions.AnyAsync())
        {
            logger.LogInformation("Seeding dynamic RBAC permissions...");

            var permEntities = Core.Constants.Permissions.All.Select(p => new Permission
            {
                Id = Guid.NewGuid(),
                Code = p.Code,
                Name = p.Name,
                Module = p.Module,
                Description = p.Description
            }).ToList();

            context.Permissions.AddRange(permEntities);
            await context.SaveChangesAsync();

            logger.LogInformation("Permissions seeded ({Count}). Seeding default role assignments...", permEntities.Count);

            var permMap = permEntities.ToDictionary(p => p.Code, p => p.Id);

            // SuperAdmin gets all permissions
            var rolePerms = new List<RolePermission>();
            foreach (var perm in permEntities)
            {
                rolePerms.Add(new RolePermission { Role = UserRole.SuperAdmin, PermissionId = perm.Id });
            }

            // DeptAdmin permissions
            var deptAdminCodes = new[]
            {
                Core.Constants.Permissions.Faculties.View,
                Core.Constants.Permissions.Departments.View,
                Core.Constants.Permissions.AcademicSessions.View,
                Core.Constants.Permissions.Users.View,
                Core.Constants.Permissions.Users.Create,
                Core.Constants.Permissions.Users.ManageStatus,
                Core.Constants.Permissions.Users.AssignDepartment
            };
            foreach (var code in deptAdminCodes)
            {
                if (permMap.TryGetValue(code, out var id))
                {
                    rolePerms.Add(new RolePermission { Role = UserRole.DeptAdmin, PermissionId = id });
                }
            }

            // Lecturer permissions
            var lecturerCodes = new[]
            {
                Core.Constants.Permissions.Faculties.View,
                Core.Constants.Permissions.Departments.View,
                Core.Constants.Permissions.AcademicSessions.View,
                Core.Constants.Permissions.Users.View
            };
            foreach (var code in lecturerCodes)
            {
                if (permMap.TryGetValue(code, out var id))
                {
                    rolePerms.Add(new RolePermission { Role = UserRole.Lecturer, PermissionId = id });
                }
            }

            // Student permissions
            var studentCodes = new[]
            {
                Core.Constants.Permissions.Faculties.View,
                Core.Constants.Permissions.Departments.View,
                Core.Constants.Permissions.AcademicSessions.View
            };
            foreach (var code in studentCodes)
            {
                if (permMap.TryGetValue(code, out var id))
                {
                    rolePerms.Add(new RolePermission { Role = UserRole.Student, PermissionId = id });
                }
            }

            context.RolePermissions.AddRange(rolePerms);
            await context.SaveChangesAsync();
            logger.LogInformation("Dynamic RBAC role-permission matrix seeded successfully.");
        }
    }
}
