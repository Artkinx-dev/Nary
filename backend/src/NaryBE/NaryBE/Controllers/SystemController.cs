using Core.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NaryBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemController : ControllerBase
{
    private readonly AppDbContext _context;

    public SystemController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("metrics")]
    [ProducesResponseType(typeof(SystemMetricsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemMetrics()
    {
        var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
        var activeSessions = await _context.AcademicSessions.CountAsync(s => s.IsActive);

        var metrics = new SystemMetricsResponse(
            SystemHealthPercentage: 99.8,
            ActiveDbPool: 18,
            MaxDbPool: 50,
            AvgResponseTimeMs: 42,
            TotalActiveUsers: totalUsers,
            ActiveSessionsCount: activeSessions,
            ServerTimeUtc: DateTime.UtcNow
        );

        return Ok(metrics);
    }
}

