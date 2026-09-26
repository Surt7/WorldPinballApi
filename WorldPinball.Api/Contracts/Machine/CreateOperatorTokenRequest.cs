namespace WorldPinball.Api.Contracts.Machine;

public sealed record CreateOperatorTokenRequest(
    string Name,
    string? Scopes = "*",
    int? ValidityDays = null
    );