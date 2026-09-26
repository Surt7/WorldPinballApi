using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class OperatorScopeAuthorizationHandler : AuthorizationHandler<OperatorScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperatorScopeRequirement requirement)
    {
        var scopeClaims = context.User.FindAll("scope").Select(c => c.Value);

        if (scopeClaims.Any(s => s == "*" || string.Equals(s, requirement.RequiredScope, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}