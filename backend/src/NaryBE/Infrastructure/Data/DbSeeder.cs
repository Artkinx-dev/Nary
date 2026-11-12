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
                Email = "fcas@nary.edu",
                OfficeLocation = "Science Complex, Block A",
                EstablishedYear = 1998,
                DeanName = "Prof. Helen Brooks",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var feng = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Engineering",
                Code = "FENG",
                Email = "faculty.of.engineering@nary.edu",
                OfficeLocation = "Central Academic Complex",
                EstablishedYear = 2001,
                DeanName = "Dr. Robert Cole",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var fmss = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Management & Social Sciences",
                Code = "FMSS",
                Email = "fmss@nary.edu",
                OfficeLocation = "Management Tower, Level 3",
                EstablishedYear = 2004,
                DeanName = "Prof. Marcus Vance",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            };

            var fes = new Faculty
            {
                Id = Guid.NewGuid(),
                Name = "Faculty of Environmental Sciences",
                Code = "FES",
                Email = "fes@nary.edu",
                OfficeLocation = "Design & Environmental Center",
                EstablishedYear = 2010,
                DeanName = "Dr. Amina Yusuf",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };

            context.Faculties.AddRange(fcas, feng, fmss, fes);
            await context.SaveChangesAsync();

            // Departments
            var departments = new List<Department>
            {
                // Computing
                new() { Id = Guid.NewGuid(), Name = "Department of Computer Science", Code = "CSC", FacultyId = fcas.Id, Email = "cs@nary.edu", OfficeLocation = "Engineering Block B, Room 204", HodName = "Dr. Amelie Hart", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Software Engineering", Code = "SEN", FacultyId = fcas.Id, Email = "se@nary.edu", OfficeLocation = "Engineering Block B, Room 210", HodName = "Prof. David Chen", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Cybersecurity", Code = "CYB", FacultyId = fcas.Id, Email = "cyber@nary.edu", OfficeLocation = "Science Complex, Room 105", HodName = "Dr. Sarah Morgan", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-5) },

                // Engineering
                new() { Id = Guid.NewGuid(), Name = "Department of Electrical & Electronics Engineering", Code = "EEE", FacultyId = feng.Id, Email = "eee@nary.edu", OfficeLocation = "Engineering Lab 1", HodName = "Dr. Nikola Tesla", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Mechanical Engineering", Code = "MEE", FacultyId = feng.Id, Email = "mech@nary.edu", OfficeLocation = "Workshop Complex", HodName = "Dr. Arthur Pendelton", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-6) },
                new() { Id = Guid.NewGuid(), Name = "Department of Civil Engineering", Code = "CVE", FacultyId = feng.Id, Email = "civil@nary.edu", OfficeLocation = "Civil Block, Room 12", HodName = "Prof. Tariq Al-Mansoor", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-5) },

                // Management & Social Sciences
                new() { Id = Guid.NewGuid(), Name = "Department of Business Administration", Code = "BUS", FacultyId = fmss.Id, Email = "bus@nary.edu", OfficeLocation = "Management Wing B", HodName = "Dr. Elena Rossi", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-5) },
                new() { Id = Guid.NewGuid(), Name = "Department of Accounting", Code = "ACC", FacultyId = fmss.Id, Email = "acc@nary.edu", OfficeLocation = "Accounting Floor 2", HodName = "James Okafor", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-5) },
                new() { Id = Guid.NewGuid(), Name = "Department of Economics", Code = "ECO", FacultyId = fmss.Id, Email = "econ@nary.edu", OfficeLocation = "Social Sciences Suite", HodName = "Dr. Adam Smith", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-4) },

                // Environmental Sciences
                new() { Id = Guid.NewGuid(), Name = "Department of Architecture", Code = "ARC", FacultyId = fes.Id, Email = "arch@nary.edu", OfficeLocation = "Studio A-101", HodName = "Prof. Kenzo Tange", IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-4) }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
            logger.LogInformation("Faculties and Departments seeded successfully.");
        }

        // 2. Seed Academic Sessions & Semesters
        if (!await context.AcademicSessions.AnyAsync())
        {
            logger.LogInformation("Seeding academic sessions, semesters, and milestones...");

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
                new() {
                    Id = Guid.NewGuid(),
                    Name = "First Semester",
                    SessionId = pastSession.Id,
                    IsActive = false,
                    StartDate = DateTime.SpecifyKind(new DateTime(2025, 9, 1), DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(new DateTime(2026, 1, 31), DateTimeKind.Utc)
                },
                new() {
                    Id = Guid.NewGuid(),
                    Name = "Second Semester",
                    SessionId = pastSession.Id,
                    IsActive = false,
                    StartDate = DateTime.SpecifyKind(new DateTime(2026, 2, 10), DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(new DateTime(2026, 6, 27), DateTimeKind.Utc)
                },
                new() {
                    Id = Guid.NewGuid(),
                    Name = "First Semester",
                    SessionId = currentSession.Id,
                    IsActive = true,
                    StartDate = DateTime.SpecifyKind(new DateTime(2026, 9, 1), DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(new DateTime(2027, 1, 31), DateTimeKind.Utc),
                    RegistrationStartDate = DateTime.SpecifyKind(new DateTime(2026, 9, 1), DateTimeKind.Utc),
                    RegistrationEndDate = DateTime.SpecifyKind(new DateTime(2026, 10, 15), DateTimeKind.Utc),
                    ExamStartDate = DateTime.SpecifyKind(new DateTime(2027, 1, 10), DateTimeKind.Utc),
                    ExamEndDate = DateTime.SpecifyKind(new DateTime(2027, 1, 28), DateTimeKind.Utc)
                },
                new() {
                    Id = Guid.NewGuid(),
                    Name = "Second Semester",
                    SessionId = currentSession.Id,
                    IsActive = false,
                    StartDate = DateTime.SpecifyKind(new DateTime(2027, 2, 15), DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(new DateTime(2027, 7, 15), DateTimeKind.Utc)
                }
            };

            context.Semesters.AddRange(semesters);

            // Milestones for current session
            var milestones = new List<AcademicMilestone>
            {
                new() { Id = Guid.NewGuid(), SessionId = currentSession.Id, Title = "Course registration closes", Subtitle = "2026/2027 · First semester", TargetDate = DateTime.SpecifyKind(new DateTime(2026, 10, 15), DateTimeKind.Utc), Category = "Registration" },
                new() { Id = Guid.NewGuid(), SessionId = currentSession.Id, Title = "Semester examinations commence", Subtitle = "All faculties", TargetDate = DateTime.SpecifyKind(new DateTime(2027, 1, 10), DateTimeKind.Utc), Category = "Exams" },
                new() { Id = Guid.NewGuid(), SessionId = currentSession.Id, Title = "Session concludes", Subtitle = "Official recess begins", TargetDate = DateTime.SpecifyKind(new DateTime(2027, 7, 15), DateTimeKind.Utc), Category = "General" },
                new() { Id = Guid.NewGuid(), SessionId = currentSession.Id, Title = "New session orientation", Subtitle = "Freshmen welcoming week", TargetDate = DateTime.SpecifyKind(new DateTime(2027, 9, 1), DateTimeKind.Utc), Category = "General" }
            };

            context.AcademicMilestones.AddRange(milestones);
            await context.SaveChangesAsync();
            logger.LogInformation("Academic Sessions, Semesters, and Milestones seeded successfully.");
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

        async Task<User> SeedUserIfNotExists(string email, string password, UserRole role, Guid? departmentId,
            string firstName = "", string lastName = "", string? title = null, string? staffId = null, string? matricNumber = null,
            int? level = null, decimal? cgpa = null, int credits = 0, string? office = null, string? employmentType = "Full-time", bool isActive = true)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email.ToLower(),
                    PasswordHash = passwordHasher.HashPassword(password),
                    Role = role,
                    DepartmentId = departmentId,
                    FirstName = firstName,
                    LastName = lastName,
                    Title = title,
                    StaffId = staffId,
                    MatricNumber = matricNumber,
                    AcademicLevel = level,
                    CurrentCgpa = cgpa,
                    CreditsEarned = credits,
                    OfficeLocation = office,
                    OfficeHours = role == UserRole.Lecturer ? "Mon & Wed: 10:00 AM - 1:00 PM" : null,
                    EmploymentType = employmentType,
                    StudentsMentoredCount = role == UserRole.Lecturer ? 42 : 0,
                    PublicationsCount = role == UserRole.Lecturer ? 18 : 0,
                    IsActive = isActive,
                    AdmissionDate = role == UserRole.Student ? DateTime.UtcNow.AddYears(-2) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
                };
                context.Users.Add(user);
            }
            return user;
        }

        logger.LogInformation("Seeding users matching frontend specifications...");

        // SuperAdmin
        await SeedUserIfNotExists("superadmin@nary.edu", "SuperAdmin123!", UserRole.SuperAdmin, null, firstName: "System", lastName: "Administrator", title: "Dr.");
        await SeedUserIfNotExists("assistant.superadmin@nary.edu", "SuperAdmin123!", UserRole.SuperAdmin, null, firstName: "Assistant", lastName: "Admin", title: "Mr.");

        // DeptAdmin
        if (cscDept != null) await SeedUserIfNotExists("cs.admin@nary.edu", "SuperAdmin123!", UserRole.DeptAdmin, cscDept.Id, firstName: "Amelie", lastName: "Hart", title: "Dr.", office: "Block B, Room 204");
        if (eeeDept != null) await SeedUserIfNotExists("eng.admin@nary.edu", "SuperAdmin123!", UserRole.DeptAdmin, eeeDept.Id, firstName: "Nikola", lastName: "Tesla", title: "Dr.", office: "Lab 1");
        if (busDept != null) await SeedUserIfNotExists("bus.admin@nary.edu", "SuperAdmin123!", UserRole.DeptAdmin, busDept.Id, firstName: "Elena", lastName: "Rossi", title: "Dr.", office: "Wing B");
        await SeedUserIfNotExists("unassigned.admin@nary.edu", "SuperAdmin123!", UserRole.DeptAdmin, null, firstName: "Unassigned", lastName: "Admin", title: "Mr.");

        // Lecturers matching frontend gaps
        var drMarcus = await SeedUserIfNotExists("marcus.smith@nary.edu", "SuperAdmin123!", UserRole.Lecturer, cscDept?.Id, firstName: "Marcus", lastName: "Smith", title: "Dr.", staffId: "STF-0142", office: "Block B, Room 304", employmentType: "Full-time");
        var profChen = await SeedUserIfNotExists("david.chen@nary.edu", "SuperAdmin123!", UserRole.Lecturer, senDept?.Id, firstName: "David", lastName: "Chen", title: "Prof.", staffId: "STF-0087", office: "Block B, Room 210", employmentType: "Full-time");
        await SeedUserIfNotExists("sarah.morgan@nary.edu", "SuperAdmin123!", UserRole.Lecturer, cybDept?.Id, firstName: "Sarah", lastName: "Morgan", title: "Dr.", staffId: "STF-0235", office: "Science Complex, Room 105", employmentType: "Full-time");
        await SeedUserIfNotExists("elena.rossi@nary.edu", "SuperAdmin123!", UserRole.Lecturer, busDept?.Id, firstName: "Elena", lastName: "Rossi", title: "Dr.", staffId: "STF-0418", office: "Management Suite", employmentType: "Visiting");
        await SeedUserIfNotExists("james.okafor@nary.edu", "SuperAdmin123!", UserRole.Lecturer, accDept?.Id, firstName: "James", lastName: "Okafor", title: "Mr.", staffId: "STF-0195", office: "Accounting Floor 2", employmentType: "Part-time");
        if (cybDept != null) await SeedUserIfNotExists("prof.curie@nary.edu", "SuperAdmin123!", UserRole.Lecturer, cybDept.Id, firstName: "Marie", lastName: "Curie", title: "Prof.", staffId: "STF-0099", office: "Cyber Lab", isActive: false);

        // Students matching frontend gaps
        await SeedUserIfNotExists("alice.johnson@nary.edu", "SuperAdmin123!", UserRole.Student, cscDept?.Id, firstName: "Alice", lastName: "Johnson", matricNumber: "NAR/24/1001", level: 300, cgpa: 4.21m, credits: 86);
        await SeedUserIfNotExists("bob.smith@nary.edu", "SuperAdmin123!", UserRole.Student, senDept?.Id, firstName: "Bob", lastName: "Smith", matricNumber: "NAR/24/1002", level: 200, cgpa: 3.85m, credits: 52);
        await SeedUserIfNotExists("charlie.brown@nary.edu", "SuperAdmin123!", UserRole.Student, cybDept?.Id, firstName: "Charlie", lastName: "Brown", matricNumber: "NAR/23/0942", level: 400, cgpa: 4.50m, credits: 114);
        await SeedUserIfNotExists("diana.prince@nary.edu", "SuperAdmin123!", UserRole.Student, eeeDept?.Id, firstName: "Diana", lastName: "Prince", matricNumber: "NAR/24/1004", level: 100, cgpa: 4.10m, credits: 24);
        await SeedUserIfNotExists("edward.elric@nary.edu", "SuperAdmin123!", UserRole.Student, meeDept?.Id, firstName: "Edward", lastName: "Elric", matricNumber: "NAR/24/1005", level: 300, cgpa: 3.92m, credits: 78);
        await SeedUserIfNotExists("fiona.gallagher@nary.edu", "SuperAdmin123!", UserRole.Student, busDept?.Id, firstName: "Fiona", lastName: "Gallagher", matricNumber: "NAR/24/1006", level: 200, cgpa: 3.65m, credits: 48);
        await SeedUserIfNotExists("george.orwell@nary.edu", "SuperAdmin123!", UserRole.Student, accDept?.Id, firstName: "George", lastName: "Orwell", matricNumber: "NAR/22/0512", level: 400, cgpa: 2.80m, credits: 96, isActive: false);
        await SeedUserIfNotExists("freshman.student@nary.edu", "SuperAdmin123!", UserRole.Student, null, firstName: "Freshman", lastName: "Student", matricNumber: "NAR/26/2001", level: 100, cgpa: 0.00m, credits: 0);

        await context.SaveChangesAsync();

        // 4. Seed Courses & Course Allocations
        if (!await context.Courses.AnyAsync() && cscDept != null && senDept != null)
        {
            logger.LogInformation("Seeding course catalog...");

            var courses = new List<Course>
            {
                new() { Id = Guid.NewGuid(), Code = "CSC 312", Title = "Advanced Data Structures & Algorithms", Credits = 3, Level = 300, DepartmentId = cscDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "CSC 305", Title = "Database Systems & Design", Credits = 3, Level = 300, DepartmentId = cscDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "CSC 412", Title = "Advanced Software Architectures", Credits = 4, Level = 400, DepartmentId = cscDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "CSC 310", Title = "Operating Systems Principles", Credits = 3, Level = 300, DepartmentId = cscDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "SEN 201", Title = "Software Engineering Principles", Credits = 3, Level = 200, DepartmentId = senDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "CYB 301", Title = "Network Security & Cryptography", Credits = 3, Level = 300, DepartmentId = cybDept?.Id ?? cscDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "EEE 301", Title = "Linear Circuit Theory", Credits = 3, Level = 300, DepartmentId = eeeDept?.Id ?? cscDept.Id, IsActive = true },
                new() { Id = Guid.NewGuid(), Code = "BUS 101", Title = "Introduction to Business Management", Credits = 2, Level = 100, DepartmentId = busDept?.Id ?? cscDept.Id, IsActive = true }
            };

            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();

            var currentActiveSession = await context.AcademicSessions.FirstOrDefaultAsync(s => s.IsActive);
            if (currentActiveSession != null)
            {
                var csc312 = courses.First(c => c.Code == "CSC 312");
                var csc305 = courses.First(c => c.Code == "CSC 305");
                var csc412 = courses.First(c => c.Code == "CSC 412");

                var allocations = new List<CourseAllocation>
                {
                    new() { Id = Guid.NewGuid(), CourseId = csc312.Id, LecturerId = drMarcus.Id, SessionId = currentActiveSession.Id, StudentsEnrolledCount = 68 },
                    new() { Id = Guid.NewGuid(), CourseId = csc305.Id, LecturerId = drMarcus.Id, SessionId = currentActiveSession.Id, StudentsEnrolledCount = 92 },
                    new() { Id = Guid.NewGuid(), CourseId = csc412.Id, LecturerId = profChen.Id, SessionId = currentActiveSession.Id, StudentsEnrolledCount = 45 }
                };

                context.CourseAllocations.AddRange(allocations);
                await context.SaveChangesAsync();
                logger.LogInformation("Courses and Course Allocations seeded successfully.");
            }
        }

        // 5. Seed Dynamic RBAC Permissions & Role-Permission Matrix
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

            var permMap = permEntities.ToDictionary(p => p.Code, p => p.Id);
            var rolePerms = new List<RolePermission>();

            foreach (var perm in permEntities)
            {
                rolePerms.Add(new RolePermission { Role = UserRole.SuperAdmin, PermissionId = perm.Id });
            }

            var deptAdminCodes = new[]
            {
                Core.Constants.Permissions.Faculties.View,
                Core.Constants.Permissions.Departments.View,
                Core.Constants.Permissions.AcademicSessions.View,
                Core.Constants.Permissions.Users.View,
                Core.Constants.Permissions.Users.Create,
                Core.Constants.Permissions.Users.ManageStatus,
                Core.Constants.Permissions.Users.AssignDepartment,
                Core.Constants.Permissions.Courses.View,
                Core.Constants.Permissions.Courses.Create
            };
            foreach (var code in deptAdminCodes)
            {
                if (permMap.TryGetValue(code, out var id))
                {
                    rolePerms.Add(new RolePermission { Role = UserRole.DeptAdmin, PermissionId = id });
                }
            }

            var lecturerCodes = new[]
            {
                Core.Constants.Permissions.Faculties.View,
                Core.Constants.Permissions.Departments.View,
                Core.Constants.Permissions.AcademicSessions.View,
                Core.Constants.Permissions.Users.View,
                Core.Constants.Permissions.Courses.View
            };
            foreach (var code in lecturerCodes)
            {
                if (permMap.TryGetValue(code, out var id))
                {
                    rolePerms.Add(new RolePermission { Role = UserRole.Lecturer, PermissionId = id });
                }
            }

            var studentCodes = new[]
            {
                Core.Constants.Permissions.Faculties.View,
                Core.Constants.Permissions.Departments.View,
                Core.Constants.Permissions.AcademicSessions.View,
                Core.Constants.Permissions.Courses.View
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
