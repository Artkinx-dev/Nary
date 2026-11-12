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
public class FacultiesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FacultiesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [HasPermission(Permissions.Faculties.Create)]
    [ProducesResponseType(typeof(FacultyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest request)
    {
        var codeExists = await _context.Faculties
            .AnyAsync(f => f.Code.ToLower() == request.Code.ToLower());

        if (codeExists)
        {
            return BadRequest(new { message = $"Faculty with code '{request.Code}' already exists." });
        }

        var faculty = new Faculty
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper(),
            Email = request.Email?.Trim(),
            OfficeLocation = request.OfficeLocation?.Trim(),
            EstablishedYear = request.EstablishedYear ?? 1998,
            DeanUserId = request.DeanUserId,
            DeanName = request.DeanName?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Faculties.Add(faculty);
        await _context.SaveChangesAsync();

        var response = new FacultyResponse(
            Id: faculty.Id,
            Name: faculty.Name,
            Code: faculty.Code,
            Email: faculty.Email,
            OfficeLocation: faculty.OfficeLocation,
            EstablishedYear: faculty.EstablishedYear,
            DeanName: faculty.DeanName,
            IsActive: faculty.IsActive,
            TotalDepartmentsCount: 0,
            TotalStudentsCount: 0,
            TotalLecturersCount: 0,
            CreatedAt: faculty.CreatedAt,
            Departments: new List<DepartmentResponse>()
        );

        return CreatedAtAction(nameof(GetFacultyById), new { id = faculty.Id }, response);
    }

    [HttpGet]
    [HasPermission(Permissions.Faculties.View)]
    [ProducesResponseType(typeof(List<FacultyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFaculties()
    {
        var faculties = await _context.Faculties
            .Include(f => f.Departments)
                .ThenInclude(d => d.Courses)
            .Include(f => f.Departments)
                .ThenInclude(d => d.Users)
            .OrderBy(f => f.Name)
            .ToListAsync();

        var response = faculties.Select(f =>
        {
            var deptResponses = f.Departments.Select(d => new DepartmentResponse(
                d.Id,
                d.Name,
                d.Code,
                d.FacultyId,
                f.Name,
                d.Email,
                d.OfficeLocation,
                d.HodName,
                d.Courses.Count,
                d.Users.Count(u => u.Role == UserRole.Student),
                d.IsActive,
                d.CreatedAt
            )).ToList();

            var totalStudents = f.Departments.SelectMany(d => d.Users).Count(u => u.Role == UserRole.Student);
            var totalLecturers = f.Departments.SelectMany(d => d.Users).Count(u => u.Role == UserRole.Lecturer);

            return new FacultyResponse(
                f.Id,
                f.Name,
                f.Code,
                f.Email,
                f.OfficeLocation,
                f.EstablishedYear,
                f.DeanName,
                f.IsActive,
                f.Departments.Count,
                totalStudents,
                totalLecturers,
                f.CreatedAt,
                deptResponses
            );
        }).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Faculties.View)]
    [ProducesResponseType(typeof(FacultyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacultyById(Guid id)
    {
        var f = await _context.Faculties
            .Include(f => f.Departments)
                .ThenInclude(d => d.Courses)
            .Include(f => f.Departments)
                .ThenInclude(d => d.Users)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (f == null)
        {
            return NotFound(new { message = "Faculty not found." });
        }

        var deptResponses = f.Departments.Select(d => new DepartmentResponse(
            d.Id,
            d.Name,
            d.Code,
            d.FacultyId,
            f.Name,
            d.Email,
            d.OfficeLocation,
            d.HodName,
            d.Courses.Count,
            d.Users.Count(u => u.Role == UserRole.Student),
            d.IsActive,
            d.CreatedAt
        )).ToList();

        var totalStudents = f.Departments.SelectMany(d => d.Users).Count(u => u.Role == UserRole.Student);
        var totalLecturers = f.Departments.SelectMany(d => d.Users).Count(u => u.Role == UserRole.Lecturer);

        var response = new FacultyResponse(
            f.Id,
            f.Name,
            f.Code,
            f.Email,
            f.OfficeLocation,
            f.EstablishedYear,
            f.DeanName,
            f.IsActive,
            f.Departments.Count,
            totalStudents,
            totalLecturers,
            f.CreatedAt,
            deptResponses
        );

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Faculties.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFaculty(Guid id)
    {
        var faculty = await _context.Faculties.FindAsync(id);
        if (faculty == null)
        {
            return NotFound(new { message = "Faculty not found." });
        }

        _context.Faculties.Remove(faculty);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
