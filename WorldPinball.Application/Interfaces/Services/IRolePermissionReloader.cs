namespace WorldPinball.Application.Interfaces.Services;

public interface IRolePermissionReloader
{
    Task ReloadFromDatabaseAsync(CancellationToken ct = default);
}
