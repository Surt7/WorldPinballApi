namespace WorldPinball.Application.Features.GameSessions.Public.KickPlayer;

public abstract record KickPlayerResult
{
    private KickPlayerResult() {}

    public sealed record Success(Guid SessionId, int SlotNumber) : KickPlayerResult;

    public sealed record SessionNotFound(Guid SessionId) : KickPlayerResult;
    
    public sealed record InvalidSlot(int SlotNumber) : KickPlayerResult;
    
    public sealed record PlayerNotFound(int SlotNumber) : KickPlayerResult;
    
    public sealed record CannotRemovePlayerActiveSession(Guid SessionId) : KickPlayerResult;
};