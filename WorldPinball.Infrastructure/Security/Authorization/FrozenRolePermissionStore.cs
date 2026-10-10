using System.Collections.Frozen;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class FrozenRolePermissionStore : IRolePermissionStore
{
    private readonly FrozenDictionary<string, FrozenSet<string>> _rolePermissions;

    public FrozenRolePermissionStore(IDictionary<string, IEnumerable<string>> rolePermissions)
    {
        // ordinal ignore case because JWT can send lower case and different encoding
        // it's better to rely on ASCI or Unicode in that case.
        _rolePermissions = rolePermissions.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPermission(string role, string permission) =>
        _rolePermissions.TryGetValue(role, out var perms) && perms.Contains(permission);

    public bool HasPermission(IEnumerable<string> roles, string permission) =>
        roles.Any(r => HasPermission(r, permission));

    public IReadOnlySet<string> GetPermissions(string role) =>
        _rolePermissions.TryGetValue(role, out var perms) ? perms : FrozenSet<string>.Empty;
}