namespace WorldPinball.Application.Features.Roles.Admin.RevokePermission;

public abstract record RevokePermissionFromRoleResult
{
    private RevokePermissionFromRoleResult() { }
    
    public sealed record Success : RevokePermissionFromRoleResult;
    
    public sealed record NotFound : RevokePermissionFromRoleResult;
}