using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Security;

public sealed class ActiveTenantRequirement : IAuthorizationRequirement
{
}
