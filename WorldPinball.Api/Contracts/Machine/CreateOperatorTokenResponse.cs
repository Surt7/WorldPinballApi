namespace WorldPinball.Api.Contracts.Machine;

public sealed record CreateOperatorTokenResponse(
    Guid TokenId,
    string Name,
    string RawToken,
    string Prefix,
    string Scopes,
    DateTime? ExpiresAtUtc
);