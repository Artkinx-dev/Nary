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
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SessionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [HasPermission(Permissions.AcademicSessions.Create)]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        if (request.EndDate <= request.StartDate)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        var sessionExists = await _context.AcademicSessions
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower());

        if (sessionExists)
        {
            return BadRequest(new { message = $"Academic session '{request.Name}' already exists." });
        }

        var session = new AcademicSession
        {
            Name = request.Name.Trim(),
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
            IsActive = false
        };

        _context.AcademicSessions.Add(session);
        await _context.SaveChangesAsync();

        var response = new SessionResponse(
            Id: session.Id,
            Name: session.Name,
            StartDate: session.StartDate,
            EndDate: session.EndDate,
            IsActive: session.IsActive,
            Semesters: new List<SemesterResponse>()
        );

        return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, response);
    }

    [HttpGet]
    [HasPermission(Permissions.AcademicSessions.View)]
    [ProducesResponseType(typeof(List<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _context.AcademicSessions
            .Include(s => s.Semesters)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SessionResponse(
                s.Id,
                s.Name,
                s.StartDate,
                s.EndDate,
                s.IsActive,
                s.Semesters.Select(sem => new SemesterResponse(
                    sem.Id,
                    sem.Name,
                    sem.SessionId,
                    sem.IsActive
                )).ToList()
            ))
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.AcademicSessions.View)]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionById(Guid id)
    {
        var session = await _context.AcademicSessions
            .Include(s => s.Semesters)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            return NotFound(new { message = "Academic session not found." });
        }

        var response = new SessionResponse(
            Id: session.Id,
            Name: session.Name,
            StartDate: session.StartDate,
            EndDate: session.EndDate,
            IsActive: session.IsActive,
            Semesters: session.Semesters.Select(sem => new SemesterResponse(
                sem.Id,
                sem.Name,
                sem.SessionId,
                sem.IsActive
            )).ToList()
        );

        return Ok(response);
    }

    [HttpPost("{id:guid}/semesters")]
    [HasPermission(Permissions.AcademicSessions.Create)]
    [ProducesResponseType(typeof(SemesterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSemester(Guid id, [FromBody] CreateSemesterRequest request)
    {
        var session = await _context.AcademicSessions.FindAsync(id);
        if (session == null)
        {
            return NotFound(new { message = "Academic session not found." });
        }

        var semesterExists = await _context.Semesters
            .AnyAsync(s => s.SessionId == id && s.Name.ToLower() == request.Name.ToLower());

        if (semesterExists)
        {
            return BadRequest(new { message = $"Semester '{request.Name}' already exists in this session." });
        }

        var semester = new Semester
        {
            Name = request.Name.Trim(),
            SessionId = id,
            IsActive = false
        };

        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();

        var response = new SemesterResponse(
            Id: semester.Id,
            Name: semester.Name,
            SessionId: semester.SessionId,
            IsActive: semester.IsActive
        );

        return Ok(response);
    }

    [HttpPut("{id:guid}/activate")]
    [HasPermission(Permissions.AcademicSessions.Activate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSession(Guid id)
    {
        var targetSession = await _context.AcademicSessions.FindAsync(id);
        if (targetSession == null)
        {
            return NotFound(new { message = "Academic session not found." });
        }

        var allSessions = await _context.AcademicSessions.ToListAsync();
        foreach (var session in allSessions)
        {
            session.IsActive = (session.Id == id);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Academic session '{targetSession.Name}' is now active." });
    }
}
