namespace WorldPinball.Api.Contracts.Sessions;

public record CreateGameSessionResponse(Guid SessionId, long ServerTime);
