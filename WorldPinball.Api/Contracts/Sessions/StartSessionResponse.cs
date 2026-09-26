namespace WorldPinball.Api.Contracts.Sessions;

public sealed record StartSessionResponse(Guid SessionId, long ServerTime) : CreateGameSessionResponse(SessionId, ServerTime);
