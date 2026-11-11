using System.Security.Claims;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        // SuperAdmin has full unrestricted access to all permissions
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value
            ?? context.User.FindFirst("role")?.Value;

        if (string.Equals(roleClaim, nameof(UserRole.SuperAdmin), StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        if (string.IsNullOrWhiteSpace(roleClaim) || !Enum.TryParse<UserRole>(roleClaim, true, out var userRole))
        {
            return;
        }

        // Check database for role permissions
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hasPermission = await db.RolePermissions
            .AnyAsync(rp => rp.Role == userRole && rp.Permission != null && rp.Permission.Code == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

