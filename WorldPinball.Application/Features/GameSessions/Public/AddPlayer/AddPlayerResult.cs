namespace WorldPinball.Application.Features.GameSessions.Public.AddPlayer;

public abstract record AddPlayerResult
{
    private AddPlayerResult() { }
    public sealed record Success(Guid SessionId, int SlotNumber) : AddPlayerResult;
    
    public sealed record SessionNotFound(Guid SessionId) : AddPlayerResult;
    
    public sealed record SessionNotJoinable(Guid SessionId) : AddPlayerResult;
    
    public sealed record SlotAlreadyOccupied(int SlotNumber) : AddPlayerResult;
    
    public sealed record UserAlreadyRegistered(Guid UserId) : AddPlayerResult;
    
    public sealed record InvalidSlot(int SlotNumber) : AddPlayerResult;
    
    public sealed record RegisteredUserRequired(Guid SessionId) : AddPlayerResult;
}