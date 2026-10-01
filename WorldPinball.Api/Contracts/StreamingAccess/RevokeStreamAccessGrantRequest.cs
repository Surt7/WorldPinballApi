namespace WorldPinball.Api.Contracts.StreamingAccess;

public sealed record RevokeStreamAccessGrantRequest(
    string Reason
);
