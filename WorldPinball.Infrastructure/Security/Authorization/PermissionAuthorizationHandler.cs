using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class PermissionAuthorizationHandler(IRolePermissionStore store) 
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement req)
    {
        var roles = context.User.FindAll(c => c.Type is ClaimTypes.Role or "roles" or "role")
            .Select(c => c.Value);

        if (store.HasPermission(roles, req.Permission))
        {
            context.Succeed(req);
        }

        return Task.CompletedTask;
    }
}
