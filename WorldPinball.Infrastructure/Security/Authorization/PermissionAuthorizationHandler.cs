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
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(req);
            return Task.CompletedTask;
        }

        if (context.User.HasClaim(c => (c.Type is "permission" or "permissions") &&
                                       string.Equals(c.Value, req.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(req);
            return Task.CompletedTask;
        }

        var roles = context.User.FindAll(c => c.Type is ClaimTypes.Role or "roles" or "role")
            .Select(c => c.Value);

        if (store.HasPermission(roles, req.Permission))
        {
            context.Succeed(req);
        }

        return Task.CompletedTask;
    }
}
