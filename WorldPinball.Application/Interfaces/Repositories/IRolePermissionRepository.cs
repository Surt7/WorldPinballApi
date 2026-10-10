namespace WorldPinball.Application.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    Task<bool> ExistsAsync(string role, string permission, CancellationToken ct = default);
    Task AddAsync(string role, string permission, CancellationToken ct = default);
    Task RemoveAsync(string role, string permission, CancellationToken ct = default);
}
