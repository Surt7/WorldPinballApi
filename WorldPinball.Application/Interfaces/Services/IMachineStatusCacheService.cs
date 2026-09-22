namespace WorldPinball.Application.Interfaces.Services;

public interface IMachineStatusCacheService
{
    Task<string?> GetStatusAsync(Guid machineId, CancellationToken ct = default);
    Task SetStatusAsync(Guid machineId, string status, TimeSpan ttl, CancellationToken ct = default);
    Task InvalidateStatusAsync(Guid machineId, CancellationToken ct = default);
}