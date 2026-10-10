namespace WorldPinball.Api.Contracts.Sessions;

public sealed record KickPlayerResponse(Guid SessionId, int SlotNumber);
