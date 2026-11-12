using Core.Constants;
using Core.DTOs;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NaryBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var emailLower = request.Email.Trim().ToLower();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower);
        if (exists)
        {
            return BadRequest(new { message = $"User with email '{request.Email}' already exists." });
        }

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId.Value);
            if (!deptExists)
            {
                return BadRequest(new { message = "Selected department does not exist." });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.MatricNumber))
        {
            var matricExists = await _context.Users.AnyAsync(u => u.MatricNumber == request.MatricNumber.Trim());
            if (matricExists)
            {
                return BadRequest(new { message = $"Student with Matriculation Number '{request.MatricNumber}' already exists." });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.StaffId))
        {
            var staffExists = await _context.Users.AnyAsync(u => u.StaffId == request.StaffId.Trim());
            if (staffExists)
            {
                return BadRequest(new { message = $"Lecturer with Staff ID '{request.StaffId}' already exists." });
            }
        }

        var user = new User
        {
            Email = emailLower,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            FirstName = request.FirstName?.Trim() ?? string.Empty,
            LastName = request.LastName?.Trim() ?? string.Empty,
            Title = request.Title?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            MatricNumber = request.MatricNumber?.Trim(),
            AcademicLevel = request.AcademicLevel,
            CurrentCgpa = request.CurrentCgpa,
            AdvisorId = request.AdvisorId,
            AdmissionDate = request.Role == UserRole.Student ? DateTime.UtcNow : null,
            StaffId = request.StaffId?.Trim(),
            EmploymentType = request.EmploymentType?.Trim() ?? "Full-time",
            OfficeLocation = request.OfficeLocation?.Trim(),
            OfficeHours = request.OfficeHours?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, MapToResponse(user, null, 0));
    }

    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserRole? role = null)
    {
        var query = _context.Users
            .Include(u => u.Department)
            .Include(u => u.Advisor)
            .AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var lecturerIds = users.Where(u => u.Role == UserRole.Lecturer).Select(u => u.Id).ToList();
        var allocationCounts = await _context.CourseAllocations
            .Where(ca => lecturerIds.Contains(ca.LecturerId))
            .GroupBy(ca => ca.LecturerId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var response = users.Select(u =>
        {
            var coursesCount = allocationCounts.TryGetValue(u.Id, out var count) ? count : 0;
            return MapToResponse(u, u.Advisor?.FullName, coursesCount);
        }).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Advisor)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var coursesCount = await _context.CourseAllocations.CountAsync(ca => ca.LecturerId == id);

        return Ok(MapToResponse(user, user.Advisor?.FullName, coursesCount));
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(Permissions.Users.ManageStatus)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Advisor)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        var coursesCount = await _context.CourseAllocations.CountAsync(ca => ca.LecturerId == id);
        return Ok(MapToResponse(user, user.Advisor?.FullName, coursesCount));
    }

    [HttpPut("{id:guid}/assign-department")]
    [HasPermission(Permissions.Users.AssignDepartment)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDepartment(Guid id, [FromBody] AssignDepartmentRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Advisor)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (request.DepartmentId.HasValue)
        {
            var dept = await _context.Departments.FindAsync(request.DepartmentId.Value);
            if (dept == null)
            {
                return BadRequest(new { message = "Selected department does not exist." });
            }
            user.Department = dept;
        }
        else
        {
            user.Department = null;
        }

        user.DepartmentId = request.DepartmentId;
        await _context.SaveChangesAsync();

        var coursesCount = await _context.CourseAllocations.CountAsync(ca => ca.LecturerId == id);
        return Ok(MapToResponse(user, user.Advisor?.FullName, coursesCount));
    }

    private static UserResponse MapToResponse(User u, string? advisorName, int coursesCount)
    {
        return new UserResponse(
            Id: u.Id,
            Email: u.Email,
            FirstName: u.FirstName,
            LastName: u.LastName,
            FullName: u.FullName,
            Title: u.Title,
            PhoneNumber: u.PhoneNumber,
            Role: u.Role,
            DepartmentId: u.DepartmentId,
            DepartmentName: u.Department?.Name,
            IsActive: u.IsActive,
            CreatedAt: u.CreatedAt,
            // Student specifics
            MatricNumber: u.MatricNumber,
            AcademicLevel: u.AcademicLevel,
            CurrentCgpa: u.CurrentCgpa,
            CreditsEarned: u.CreditsEarned,
            AdvisorName: advisorName,
            AdmissionDate: u.AdmissionDate,
            // Lecturer specifics
            StaffId: u.StaffId,
            EmploymentType: u.EmploymentType,
            OfficeLocation: u.OfficeLocation,
            OfficeHours: u.OfficeHours,
            CoursesCount: u.Role == UserRole.Lecturer ? coursesCount : null,
            StudentsMentoredCount: u.StudentsMentoredCount,
            PublicationsCount: u.PublicationsCount
        );
    }
}
