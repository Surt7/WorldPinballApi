using WorldPinball.Application.Features.Roles.Admin.RevokePermission;
using WorldPinball.Application.Features.Roles.Admin.RevokePermission.Commands;


namespace WorldPinball.Application.Features.Roles.Admin.RevokePermission;

public interface IRevokePermissionFromRoleHandler
{
    Task<RevokePermissionFromRoleResult> HandleAsync(
        RevokePermissionFromRoleCommand command, 
        CancellationToken ct = default);
}
