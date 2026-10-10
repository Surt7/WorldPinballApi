using WorldPinball.Application.Features.Roles.Admin.RevokePermission.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.Roles.Admin.RevokePermission;

public sealed class RevokePermissionFromRoleHandler(
    IRolePermissionRepository repository,
    IRolePermissionReloader reloader) : IRevokePermissionFromRoleHandler
{
    public async Task<RevokePermissionFromRoleResult> HandleAsync(
        RevokePermissionFromRoleCommand command, CancellationToken ct = default)
    {
        if (!await repository.ExistsAsync(command.Role, command.Permission, ct))
            return new RevokePermissionFromRoleResult.NotFound();

        await repository.RemoveAsync(command.Role, command.Permission, ct);
        await reloader.ReloadFromDatabaseAsync(ct);

        return new RevokePermissionFromRoleResult.Success();
    }
}
