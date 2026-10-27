using Microsoft.AspNetCore.Identity;

namespace AuthServer.Models;

/// <summary>
/// Links an existing Identity user to a company. A user may belong to multiple companies.
/// </summary>
public sealed class UserCompany
{
    public required string UserId { get; set; }

    public int CompanyId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset JoinedUtc { get; set; } = DateTimeOffset.UtcNow;

    public IdentityUser User { get; set; } = null!;

    public Company Company { get; set; } = null!;
}
