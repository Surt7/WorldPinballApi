namespace WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant;

public abstract record CreateStreamAccessGrantResult
{
    private CreateStreamAccessGrantResult() { }

    public sealed record Success(Guid GrantId) : CreateStreamAccessGrantResult;
    public sealed record AlreadyExists(Guid GrantId) : CreateStreamAccessGrantResult;
    public sealed record InvalidData(string Error) : CreateStreamAccessGrantResult;
}