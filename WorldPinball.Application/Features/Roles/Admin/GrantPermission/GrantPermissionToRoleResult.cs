namespace WorldPinball.Application.Features.Roles.Admin.GrantPermission;

public abstract record GrantPermissionToRoleResult
{
    private GrantPermissionToRoleResult() { }

    public sealed record Success : GrantPermissionToRoleResult;
    public sealed record AlreadyExists : GrantPermissionToRoleResult;
    public sealed record InvalidData(string Reason) : GrantPermissionToRoleResult;
}
