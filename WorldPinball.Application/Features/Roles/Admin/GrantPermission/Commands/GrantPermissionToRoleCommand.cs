namespace WorldPinball.Application.Features.Roles.Admin.GrantPermission.Commands;

public sealed record GrantPermissionToRoleCommand(
    string Role,
    string Permission
);