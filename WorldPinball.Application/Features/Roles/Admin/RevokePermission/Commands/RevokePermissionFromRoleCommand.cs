namespace WorldPinball.Application.Features.Roles.Admin.RevokePermission.Commands;

public sealed record RevokePermissionFromRoleCommand(string Role, string Permission);