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

        var user = new User
        {
            Email = emailLower,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        string? deptName = null;
        if (user.DepartmentId.HasValue)
        {
            deptName = await _context.Departments
                .Where(d => d.Id == user.DepartmentId.Value)
                .Select(d => d.Name)
                .FirstOrDefaultAsync();
        }

        var response = new UserResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role,
            DepartmentId: user.DepartmentId,
            DepartmentName: deptName,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt
        );

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
    }

    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserRole? role = null)
    {
        var query = _context.Users
            .Include(u => u.Department)
            .AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserResponse(
                u.Id,
                u.Email,
                u.Role,
                u.DepartmentId,
                u.Department != null ? u.Department.Name : null,
                u.IsActive,
                u.CreatedAt
            ))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Users.View)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var response = new UserResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role,
            DepartmentId: user.DepartmentId,
            DepartmentName: user.Department?.Name,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt
        );

        return Ok(response);
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(Permissions.Users.ManageStatus)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new UserResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role,
            DepartmentId: user.DepartmentId,
            DepartmentName: user.Department?.Name,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt
        ));
    }

    [HttpPut("{id:guid}/assign-department")]
    [HasPermission(Permissions.Users.AssignDepartment)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDepartment(Guid id, [FromBody] AssignDepartmentRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        string? deptName = null;
        if (request.DepartmentId.HasValue)
        {
            var dept = await _context.Departments.FindAsync(request.DepartmentId.Value);
            if (dept == null)
            {
                return BadRequest(new { message = "Selected department does not exist." });
            }
            deptName = dept.Name;
        }

        user.DepartmentId = request.DepartmentId;
        await _context.SaveChangesAsync();

        return Ok(new UserResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role,
            DepartmentId: user.DepartmentId,
            DepartmentName: deptName,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt
        ));
    }
}
