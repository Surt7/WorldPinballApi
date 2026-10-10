namespace WorldPinball.Infrastructure.Security.Authorization;

public interface IRolePermissionStoreReloader
{
    void Reload(IDictionary<string, IEnumerable<string>> rolePermissions);
    
    Task ReloadFromDatabaseAsync(CancellationToken ct = default);
}