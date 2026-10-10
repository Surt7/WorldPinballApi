namespace WorldPinball.Application.Interfaces.Services;

public interface ILiveTelemetryService
{
    // Ingests real-time score delta into Redis and notifies spectators via SignalR
    Task<long> IngestDeltaAsync(Guid sessionId, int slotNumber, long pointsDelta, string triggerEvent, CancellationToken token = default);
}