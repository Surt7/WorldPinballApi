namespace WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken;

public abstract record CreateOperatorTokenResult
{
    public sealed record Success(
        Guid TokenId,
        string Name,
        string RawToken,
        string Prefix,
        string Scopes,
        DateTime? ExpiresAtUtc
    ) : CreateOperatorTokenResult;
    
    public sealed record InvalidScopes(string Reason) : CreateOperatorTokenResult;
};