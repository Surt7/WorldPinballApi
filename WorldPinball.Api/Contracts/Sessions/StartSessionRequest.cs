namespace WorldPinball.Api.Contracts.Sessions;

public sealed record StartSessionRequest(string ModeCode, string ClientVersion) : CreateGameSessionRequest(ModeCode, ClientVersion);
