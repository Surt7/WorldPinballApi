namespace WorldPinball.Api.Contracts.Sessions;

public sealed record AddPlayerRequest(
    int SlotNumber,
    Guid? UserId,
    string? GuestInitials,
    bool RequiresRegisteredUser = false);
