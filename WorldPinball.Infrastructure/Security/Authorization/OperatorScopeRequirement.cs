using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class OperatorScopeRequirement(string requiredScope) : IAuthorizationRequirement
{
    public string RequiredScope { get; } = requiredScope;
}