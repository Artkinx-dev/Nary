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
[Authorize(Policy = "RequireSuperAdmin")]
public class RbacController : ControllerBase
{
    private readonly AppDbContext _context;

    public RbacController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("permissions")]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionResponse(
                p.Id,
                p.Code,
                p.Name,
                p.Module,
                p.Description
            ))
            .ToListAsync();

        return Ok(permissions);
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(List<RolePermissionsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolePermissions()
    {
        var allRoles = Enum.GetValues<UserRole>();
        var rolePermissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .ToListAsync();

        var result = new List<RolePermissionsResponse>();

        foreach (var role in allRoles)
        {
            var perms = rolePermissions
                .Where(rp => rp.Role == role && rp.Permission != null)
                .Select(rp => new PermissionResponse(
                    rp.Permission!.Id,
                    rp.Permission.Code,
                    rp.Permission.Name,
                    rp.Permission.Module,
                    rp.Permission.Description
                ))
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .ToList();

            result.Add(new RolePermissionsResponse(
                Role: role,
                RoleName: role.ToString(),
                Permissions: perms
            ));
        }

        return Ok(result);
    }

    [HttpGet("roles/{role}/permissions")]
    [ProducesResponseType(typeof(RolePermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermissionsForRole(UserRole role)
    {
        var perms = await _context.RolePermissions
            .Where(rp => rp.Role == role && rp.Permission != null)
            .Include(rp => rp.Permission)
            .Select(rp => new PermissionResponse(
                rp.Permission!.Id,
                rp.Permission.Code,
                rp.Permission.Name,
                rp.Permission.Module,
                rp.Permission.Description
            ))
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return Ok(new RolePermissionsResponse(
            Role: role,
            RoleName: role.ToString(),
            Permissions: perms
        ));
    }

    [HttpPut("roles/{role}/permissions")]
    [ProducesResponseType(typeof(RolePermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRolePermissions(UserRole role, [FromBody] UpdateRolePermissionsRequest request)
    {
        if (role == UserRole.SuperAdmin)
        {
            return BadRequest(new { message = "SuperAdmin permissions are immutable to prevent administrative lockout." });
        }

        var normalizedCodes = request.PermissionCodes
            .Select(c => c.Trim().ToLower())
            .Distinct()
            .ToList();

        var validPermissions = await _context.Permissions
            .Where(p => normalizedCodes.Contains(p.Code.ToLower()))
            .ToListAsync();

        // Remove existing permissions for this role
        var existingRolePerms = await _context.RolePermissions
            .Where(rp => rp.Role == role)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingRolePerms);

        // Add newly assigned permissions
        var newRolePerms = validPermissions.Select(p => new RolePermission
        {
            Role = role,
            PermissionId = p.Id,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.RolePermissions.AddRange(newRolePerms);
        await _context.SaveChangesAsync();

        var updatedPerms = validPermissions.Select(p => new PermissionResponse(
            p.Id,
            p.Code,
            p.Name,
            p.Module,
            p.Description
        )).OrderBy(p => p.Module).ThenBy(p => p.Name).ToList();

        return Ok(new RolePermissionsResponse(
            Role: role,
            RoleName: role.ToString(),
            Permissions: updatedPerms
        ));
    }

    [HttpGet("users/{userId:guid}/effective-permissions")]
    [ProducesResponseType(typeof(UserEffectivePermissionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserEffectivePermissions(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        List<string> permissionCodes;

        if (user.Role == UserRole.SuperAdmin)
        {
            // SuperAdmin has all permissions
            permissionCodes = await _context.Permissions.Select(p => p.Code).ToListAsync();
        }
        else
        {
            permissionCodes = await _context.RolePermissions
                .Where(rp => rp.Role == user.Role && rp.Permission != null)
                .Select(rp => rp.Permission!.Code)
                .ToListAsync();
        }

        return Ok(new UserEffectivePermissionsResponse(
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role,
            Permissions: permissionCodes
        ));
    }
}

