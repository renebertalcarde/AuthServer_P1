namespace AuthServer.Models;

/// <summary>
/// Represents a company (tenant) that can use the AuthServer-backed applications.
/// </summary>
public sealed class Company
{
    public int Id { get; set; }

    /// <summary>A human-readable company name.</summary>
    public required string Name { get; set; }

    /// <summary>A stable, unique code suitable for lookup and display.</summary>
    public required string Code { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}
