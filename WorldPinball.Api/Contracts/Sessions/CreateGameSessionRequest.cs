namespace WorldPinball.Api.Contracts.Sessions;

public record CreateGameSessionRequest(string ModeCode, string ClientVersion);
