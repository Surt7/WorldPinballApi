namespace WorldPinball.Application.Features.StreamingAccess.Admin.RevokeStreamAccessGrant;

public abstract record RevokeStreamAccessGrantResult
{
    private RevokeStreamAccessGrantResult() { }

    public sealed record Success(Guid GrantId) : RevokeStreamAccessGrantResult;
    public sealed record NotFound(Guid GrantId) : RevokeStreamAccessGrantResult;
    public sealed record AlreadyRevoked(Guid GrantId) : RevokeStreamAccessGrantResult;
    public sealed record InvalidReason(string Error) : RevokeStreamAccessGrantResult;
}