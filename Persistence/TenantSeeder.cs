using AuthServer.Models;
using AuthServer.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LogixSys.AuthServer.Persistence;

/// <summary>
/// Seeds the initial development tenant and links an existing Identity user.
/// This seeder never creates users or changes credentials.
/// </summary>
public static class TenantSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        const string username = "gong";
        const string companyName = "Company A";
        const string companyCode = "COMP-A";

        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"Tenant seeding stopped: Identity user '{username}' was not found. " +
                "Create or verify this user in the Development database, then restart the application.");
        }

        // Use the unique company code so startup can safely be repeated.
        var company = await dbContext.Companies
            .SingleOrDefaultAsync(company => company.Code == companyCode);

        if (company is null)
        {
            company = new Company
            {
                Name = companyName,
                Code = companyCode,
                IsActive = true,
                CreatedUtc = DateTimeOffset.UtcNow
            };

            dbContext.Companies.Add(company);
            await dbContext.SaveChangesAsync();
        }

        // The composite primary key makes this membership unique.
        var membershipExists = await dbContext.UserCompanies
            .AnyAsync(membership =>
                membership.UserId == user.Id &&
                membership.CompanyId == company.Id);

        if (!membershipExists)
        {
            dbContext.UserCompanies.Add(new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                IsActive = true,
                JoinedUtc = DateTimeOffset.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
