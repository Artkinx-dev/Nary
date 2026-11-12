using Core.Constants;
using Core.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NaryBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HasPermission(Permissions.Courses.View)]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] Guid? departmentId = null,
        [FromQuery] int? level = null)
    {
        var query = _context.Courses
            .Include(c => c.Department)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(c => c.DepartmentId == departmentId.Value);
        }

        if (level.HasValue)
        {
            query = query.Where(c => c.Level == level.Value);
        }

        var courses = await query
            .OrderBy(c => c.Code)
            .Select(c => new CourseResponse(
                c.Id,
                c.Code,
                c.Title,
                c.Credits,
                c.Level,
                c.DepartmentId,
                c.Department != null ? c.Department.Name : null,
                c.IsActive,
                c.CreatedAt
            ))
            .ToListAsync();

        return Ok(courses);
    }

    [HttpPost]
    [HasPermission(Permissions.Courses.Create)]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var codeExists = await _context.Courses.AnyAsync(c => c.Code.ToLower() == request.Code.Trim().ToLower());
        if (codeExists)
        {
            return BadRequest(new { message = $"Course with code '{request.Code}' already exists." });
        }

        var department = await _context.Departments.FindAsync(request.DepartmentId);
        if (department == null)
        {
            return BadRequest(new { message = "Selected department does not exist." });
        }

        var course = new Course
        {
            Code = request.Code.Trim().ToUpper(),
            Title = request.Title.Trim(),
            Credits = request.Credits,
            Level = request.Level,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        var response = new CourseResponse(
            course.Id,
            course.Code,
            course.Title,
            course.Credits,
            course.Level,
            course.DepartmentId,
            department.Name,
            course.IsActive,
            course.CreatedAt
        );

        return CreatedAtAction(nameof(GetCourses), new { departmentId = course.DepartmentId }, response);
    }

    [HttpGet("lecturers/{lecturerId:guid}")]
    [HasPermission(Permissions.Courses.View)]
    [ProducesResponseType(typeof(List<CourseAllocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLecturerCourses(Guid lecturerId)
    {
        var allocations = await _context.CourseAllocations
            .Where(ca => ca.LecturerId == lecturerId)
            .Include(ca => ca.Course)
            .Include(ca => ca.Lecturer)
            .OrderBy(ca => ca.Course!.Code)
            .Select(ca => new CourseAllocationResponse(
                ca.Id,
                ca.CourseId,
                ca.Course != null ? ca.Course.Code : "N/A",
                ca.Course != null ? ca.Course.Title : "N/A",
                ca.LecturerId,
                ca.Lecturer != null ? ca.Lecturer.FullName : "N/A",
                ca.SessionId,
                ca.StudentsEnrolledCount
            ))
            .ToListAsync();

        return Ok(allocations);
    }

    [HttpGet("students/{studentId:guid}")]
    [HasPermission(Permissions.Courses.View)]
    [ProducesResponseType(typeof(List<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentCourses(Guid studentId)
    {
        var student = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == studentId);

        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        // Return courses corresponding to student's department and level
        var query = _context.Courses
            .Include(c => c.Department)
            .Where(c => c.DepartmentId == student.DepartmentId);

        if (student.AcademicLevel.HasValue)
        {
            query = query.Where(c => c.Level == student.AcademicLevel.Value);
        }

        var courses = await query
            .OrderBy(c => c.Code)
            .Select(c => new CourseResponse(
                c.Id,
                c.Code,
                c.Title,
                c.Credits,
                c.Level,
                c.DepartmentId,
                c.Department != null ? c.Department.Name : null,
                c.IsActive,
                c.CreatedAt
            ))
            .ToListAsync();

        return Ok(courses);
    }
}

