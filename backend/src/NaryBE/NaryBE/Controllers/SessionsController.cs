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
                    sem.IsActive,
                    sem.StartDate,
                    sem.EndDate,
                    sem.RegistrationStartDate,
                    sem.RegistrationEndDate,
                    sem.ExamStartDate,
                    sem.ExamEndDate
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
                sem.IsActive,
                sem.StartDate,
                sem.EndDate,
                sem.RegistrationStartDate,
                sem.RegistrationEndDate,
                sem.ExamStartDate,
                sem.ExamEndDate
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

        var startDate = request.StartDate.HasValue
            ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc)
            : session.StartDate;

        var endDate = request.EndDate.HasValue
            ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc)
            : session.EndDate;

        var semester = new Semester
        {
            Name = request.Name.Trim(),
            SessionId = id,
            IsActive = false,
            StartDate = startDate,
            EndDate = endDate,
            RegistrationStartDate = request.RegistrationStartDate.HasValue ? DateTime.SpecifyKind(request.RegistrationStartDate.Value, DateTimeKind.Utc) : null,
            RegistrationEndDate = request.RegistrationEndDate.HasValue ? DateTime.SpecifyKind(request.RegistrationEndDate.Value, DateTimeKind.Utc) : null,
            ExamStartDate = request.ExamStartDate.HasValue ? DateTime.SpecifyKind(request.ExamStartDate.Value, DateTimeKind.Utc) : null,
            ExamEndDate = request.ExamEndDate.HasValue ? DateTime.SpecifyKind(request.ExamEndDate.Value, DateTimeKind.Utc) : null
        };

        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();

        var response = new SemesterResponse(
            Id: semester.Id,
            Name: semester.Name,
            SessionId: semester.SessionId,
            IsActive: semester.IsActive,
            StartDate: semester.StartDate,
            EndDate: semester.EndDate,
            RegistrationStartDate: semester.RegistrationStartDate,
            RegistrationEndDate: semester.RegistrationEndDate,
            ExamStartDate: semester.ExamStartDate,
            ExamEndDate: semester.ExamEndDate
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

    // --- Academic Milestones ---
    [HttpGet("{id:guid}/milestones")]
    [HasPermission(Permissions.AcademicSessions.View)]
    [ProducesResponseType(typeof(List<AcademicMilestoneResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionMilestones(Guid id)
    {
        var sessionExists = await _context.AcademicSessions.AnyAsync(s => s.Id == id);
        if (!sessionExists)
        {
            return NotFound(new { message = "Academic session not found." });
        }

        var milestones = await _context.AcademicMilestones
            .Where(m => m.SessionId == id)
            .OrderBy(m => m.TargetDate)
            .Select(m => new AcademicMilestoneResponse(
                m.Id,
                m.SessionId,
                m.Title,
                m.Subtitle,
                m.TargetDate,
                m.Category
            ))
            .ToListAsync();

        return Ok(milestones);
    }

    [HttpPost("{id:guid}/milestones")]
    [HasPermission(Permissions.AcademicSessions.Create)]
    [ProducesResponseType(typeof(AcademicMilestoneResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMilestone(Guid id, [FromBody] CreateAcademicMilestoneRequest request)
    {
        var session = await _context.AcademicSessions.FindAsync(id);
        if (session == null)
        {
            return NotFound(new { message = "Academic session not found." });
        }

        var milestone = new AcademicMilestone
        {
            SessionId = id,
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle?.Trim(),
            TargetDate = DateTime.SpecifyKind(request.TargetDate, DateTimeKind.Utc),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim()
        };

        _context.AcademicMilestones.Add(milestone);
        await _context.SaveChangesAsync();

        var response = new AcademicMilestoneResponse(
            milestone.Id,
            milestone.SessionId,
            milestone.Title,
            milestone.Subtitle,
            milestone.TargetDate,
            milestone.Category
        );

        return CreatedAtAction(nameof(GetSessionMilestones), new { id = milestone.SessionId }, response);
    }
}
