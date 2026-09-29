namespace WorldPinball.Api.Hubs;

public interface ILobbyHubClient
{
    Task PlayerJoined(int slotNumber, Guid? userId, string? guestInitials);
    Task PlayerKicked(int slotNumber);
    Task SessionLaunched();
    Task ScoreSubmitted(int slotNumber, long score);
    Task SessionEnded(Guid sessionId);
    Task SessionAborted(Guid sessionId);
    Task MatchFound(Guid sessionId, Guid machineId);
}