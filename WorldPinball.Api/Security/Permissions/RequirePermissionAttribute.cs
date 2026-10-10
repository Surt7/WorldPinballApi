using Microsoft.AspNetCore.Authorization;

namespace WorldPinball.Api.Security.Permissions;

[AttributeUsage(AttributeTargets.Method |
                AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(Permission permission) 
    : AuthorizeAttribute(policy: permission.ToString())
{
    public Permission Permission { get; } = permission;
}