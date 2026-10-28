using System.Security.Claims;
using AuthServer.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

namespace AuthServer.Controllers;

[ApiController]
[Route("api/tenant")]
[Authorize(
    AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Policy = "ActiveTenant")]
public sealed class TenantController : ControllerBase
{
    private readonly AuthDbContext _dbContext;

    public TenantController(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the current tenant from the authenticated principal after
    /// the ActiveTenant policy has verified the user's membership.
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentTenant()
    {
        var tenantIdValue = User.FindFirst("tenant_id")?.Value;
        if (!int.TryParse(tenantIdValue, out var tenantId))
        {
            return Forbid();
        }

        var company = await _dbContext.Companies
            .Where(company => company.Id == tenantId && company.IsActive)
            .Select(company => new
            {
                company.Id,
                company.Name,
                company.Code
            })
            .SingleOrDefaultAsync();

        if (company is null)
        {
            return Forbid();
        }

        return Ok(company);
    }
}
