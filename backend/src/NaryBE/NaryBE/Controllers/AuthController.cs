using Core.DTOs;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NaryBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthController(AppDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Account is inactive. Contact the administrator." });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse(
            Token: token,
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role
        ));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Advisor)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        var coursesCount = await _context.CourseAllocations.CountAsync(ca => ca.LecturerId == userId);

        return Ok(new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: user.FullName,
            Title: user.Title,
            PhoneNumber: user.PhoneNumber,
            Role: user.Role,
            DepartmentId: user.DepartmentId,
            DepartmentName: user.Department?.Name,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            MatricNumber: user.MatricNumber,
            AcademicLevel: user.AcademicLevel,
            CurrentCgpa: user.CurrentCgpa,
            CreditsEarned: user.CreditsEarned,
            AdvisorName: user.Advisor?.FullName,
            AdmissionDate: user.AdmissionDate,
            StaffId: user.StaffId,
            EmploymentType: user.EmploymentType,
            OfficeLocation: user.OfficeLocation,
            OfficeHours: user.OfficeHours,
            CoursesCount: user.Role == Core.Enums.UserRole.Lecturer ? coursesCount : null,
            StudentsMentoredCount: user.StudentsMentoredCount,
            PublicationsCount: user.PublicationsCount
        ));
    }
}
