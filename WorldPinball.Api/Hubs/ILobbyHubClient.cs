namespace WorldPinball.Api.Hubs;

public interface ILobbyHubClient
{
    Task PlayerJoined(int slotNumber, Guid? userId, string? guestInitials);
    Task PlayerKicked(int slotNumber);
    Task SessionLaunched();
}