using Core.Constants;
using Core.DTOs;
using Infrastructure.Data;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NaryBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HasPermission(Permissions.AuditLogs.View)]
    [ProducesResponseType(typeof(List<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);

        var logs = await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new AuditLogResponse(
                a.Id,
                a.UserId,
                a.Action,
                a.EntityName,
                a.EntityId,
                a.Details,
                a.Timestamp
            ))
            .ToListAsync();

        return Ok(logs);
    }
}
