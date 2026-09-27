namespace WorldPinball.Api.Contracts.Sessions;

public sealed record AddPlayerResponse(Guid SessionId, int SlotNumber);
