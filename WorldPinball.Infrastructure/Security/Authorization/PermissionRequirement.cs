using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;