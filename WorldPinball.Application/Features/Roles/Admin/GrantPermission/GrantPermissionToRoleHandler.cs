
using WorldPinball.Application.Features.Roles.Admin.GrantPermission.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Application.Features.Roles.Admin.GrantPermission;

public sealed class GrantPermissionToRoleHandler(
    IRolePermissionRepository repository,
    IRolePermissionReloader reloader) : IGrantPermissionToRoleHandler
{
    public async Task<GrantPermissionToRoleResult> HandleAsync(
        GrantPermissionToRoleCommand command, CancellationToken ct = default)
    {
        if (await repository.ExistsAsync(command.Role, command.Permission, ct))
            return new GrantPermissionToRoleResult.AlreadyExists();
        
        await repository.AddAsync(command.Role, command.Permission, ct);
        
        await reloader.ReloadFromDatabaseAsync(ct);
        
        
        return new GrantPermissionToRoleResult.Success();
    }
}