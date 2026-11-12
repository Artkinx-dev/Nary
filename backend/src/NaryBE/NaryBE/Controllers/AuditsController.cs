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
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName = null,
        [FromQuery] string? entityId = null,
        [FromQuery] int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            var trimmedName = entityName.Trim().ToLower();
            query = query.Where(a => a.EntityName.ToLower() == trimmedName);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            var trimmedId = entityId.Trim().ToLower();
            query = query.Where(a => a.EntityId.ToLower() == trimmedId);
        }

        var logs = await query
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
