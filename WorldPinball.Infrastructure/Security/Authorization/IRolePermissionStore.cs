namespace WorldPinball.Infrastructure.Security.Authorization;

public interface IRolePermissionStore
{
    bool HasPermission(string role, string permission);
    bool HasPermission(IEnumerable<string> roles, string permission);
    IReadOnlySet<string> GetPermissions(string role);
}