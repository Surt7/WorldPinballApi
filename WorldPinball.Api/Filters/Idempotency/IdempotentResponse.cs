namespace WorldPinball.Api.Filters.Idempotency;

public sealed record IdempotentResponse(
    int StatusCode, 
    string? ContentType, 
    string? Body
);