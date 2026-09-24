namespace WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;

public sealed record AddPlayerCommand(
    Guid SessionId,
    int SlotNumber,
    Guid? UserId = null,
    string? GuestInitials = null,
    bool RequiresRegisteredUser = false
);