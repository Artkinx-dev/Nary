using Core.Constants;
using Core.DTOs;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NaryBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [HasPermission(Permissions.Departments.Create)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        var faculty = await _context.Faculties.FindAsync(request.FacultyId);
        if (faculty == null)
        {
            return BadRequest(new { message = "Associated faculty not found." });
        }

        var codeExists = await _context.Departments
            .AnyAsync(d => d.Code.ToLower() == request.Code.ToLower());

        if (codeExists)
        {
            return BadRequest(new { message = $"Department with code '{request.Code}' already exists." });
        }

        var department = new Department
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            FacultyId = request.FacultyId,
            Email = request.Email?.Trim(),
            OfficeLocation = request.OfficeLocation?.Trim(),
            HodUserId = request.HodUserId,
            HodName = request.HodName?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        var response = new DepartmentResponse(
            department.Id,
            department.Name,
            department.Code,
            department.FacultyId,
            faculty.Name,
            department.Email,
            department.OfficeLocation,
            department.HodName,
            0,
            0,
            department.IsActive,
            department.CreatedAt
        );

        return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, response);
    }

    [HttpGet]
    [HasPermission(Permissions.Departments.View)]
    [ProducesResponseType(typeof(List<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments([FromQuery] Guid? facultyId = null)
    {
        var query = _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Courses)
            .Include(d => d.Users)
            .AsQueryable();

        if (facultyId.HasValue)
        {
            query = query.Where(d => d.FacultyId == facultyId.Value);
        }

        var departments = await query
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentResponse(
                d.Id,
                d.Name,
                d.Code,
                d.FacultyId,
                d.Faculty != null ? d.Faculty.Name : null,
                d.Email,
                d.OfficeLocation,
                d.HodName,
                d.Courses.Count,
                d.Users.Count(u => u.Role == UserRole.Student),
                d.IsActive,
                d.CreatedAt
            ))
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Departments.View)]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        var d = await _context.Departments
            .Include(d => d.Faculty)
            .Include(d => d.Courses)
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (d == null)
        {
            return NotFound(new { message = "Department not found." });
        }

        var response = new DepartmentResponse(
            d.Id,
            d.Name,
            d.Code,
            d.FacultyId,
            d.Faculty?.Name,
            d.Email,
            d.OfficeLocation,
            d.HodName,
            d.Courses.Count,
            d.Users.Count(u => u.Role == UserRole.Student),
            d.IsActive,
            d.CreatedAt
        );

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Departments.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound(new { message = "Department not found." });
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
