using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class ActiveMachineAuthorizationHandler 
    : AuthorizationHandler<ActiveMachineRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveMachineRequirement requirement)
    {
        var status = context.User.FindFirst("machine_status")?.Value;
        if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}