using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Infrastructure.Security.Authorization;

public sealed class DynamicRolePermissionStore: IRolePermissionStore, IRolePermissionStoreReloader, IRolePermissionReloader
{
    /* "volatile" keyword like C/C++ means we forbid any CPU local registry
     it allows availability to all remain core and swap pointer atomicity.
     */
    private volatile FrozenRolePermissionStore _current;
    private readonly IServiceScopeFactory? _scopeFactory;
    
    public DynamicRolePermissionStore(
        IServiceScopeFactory? scopeFactory = null,
        IDictionary<string, IEnumerable<string>>? initial = null)
    {
        _scopeFactory = scopeFactory;
        _current = new FrozenRolePermissionStore(initial ?? new Dictionary<string, IEnumerable<string>>());
    }

    public DynamicRolePermissionStore(IDictionary<string, IEnumerable<string>> initial)
        : this(null, initial) { }

    public void Reload(IDictionary<string, IEnumerable<string>> data) =>
        _current = new FrozenRolePermissionStore(data);

    public async Task ReloadFromDatabaseAsync(CancellationToken ct = default)
    {
        if (_scopeFactory is null) return;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            /* reminder : we use createScope / factory and locator
             for resolver because frozen are singleton while
             AppDbContext is scoped, we have to be careful about dependence captivity 
            */
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var list = await db.RolePermissions.AsNoTracking().ToListAsync(ct);
            
            var grouped = list
                .GroupBy(rp => rp.Role)
                .ToDictionary(g => g.Key,
                    g => g.Select(rp => rp.Permission));
            
            Reload(grouped);
        }
        catch
        {
            // Resilient fallback: If database is not ready or migrating, keep existing in-memory store
        }
    }

    public bool HasPermission(string role, string permission) => 
        _current.HasPermission(role, permission);

    public bool HasPermission(IEnumerable<string> roles, string permission) => 
        _current.HasPermission(roles, permission);

    public IReadOnlySet<string> GetPermissions(string role) => 
        _current.GetPermissions(role);
    
    
}