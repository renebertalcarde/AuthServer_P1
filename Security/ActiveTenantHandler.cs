using System.Security.Claims;
using AuthServer.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Security;

/// <summary>
/// Confirms the authenticated principal's selected tenant is still an active
/// membership. Apply the ActiveTenant policy to tenant-protected endpoints.
/// </summary>
public sealed class ActiveTenantHandler
    : AuthorizationHandler<ActiveTenantRequirement>
{
    private readonly AuthDbContext _dbContext;

    public ActiveTenantHandler(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveTenantRequirement requirement)
    {
        var userId =
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            context.User.FindFirst("sub")?.Value;

        var tenantIdValue = context.User.FindFirst("tenant_id")?.Value;

        if (string.IsNullOrWhiteSpace(userId) ||
            !int.TryParse(tenantIdValue, out var tenantId))
        {
            return;
        }

        var hasActiveMembership = await _dbContext.UserCompanies
            .AnyAsync(membership =>
                membership.UserId == userId &&
                membership.CompanyId == tenantId &&
                membership.IsActive &&
                membership.Company.IsActive);

        if (hasActiveMembership)
        {
            context.Succeed(requirement);
        }
    }
}
