using Microsoft.AspNetCore.Identity;

namespace LogixSys.AuthServer.Persistence;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var username = configuration["SeedUser:UserName"];
        var email = configuration["SeedUser:Email"];
        var password = configuration["SeedUser:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SeedUser configuration is incomplete.");
        }

        var existingUser =
            await userManager.FindByNameAsync(username);

        if (existingUser is not null)
        {
            return;
        }

        var user = new IdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(
            user,
            password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(
                    error => error.Description));

            throw new InvalidOperationException(
                $"Failed to create user: {errors}");
        }
    }
}