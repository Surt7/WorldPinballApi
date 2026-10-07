using WorldPinball.Application.Features.Roles.Admin.GrantPermission.Commands;

namespace WorldPinball.Application.Features.Roles.Admin.GrantPermission;

public interface IGrantPermissionToRoleHandler
{
    Task<GrantPermissionToRoleResult> HandleAsync(
        GrantPermissionToRoleCommand command, 
        CancellationToken ct = default);
}