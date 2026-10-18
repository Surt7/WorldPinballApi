namespace WorldPinball.Logistics.Application.Interfaces;

public interface ILogisticsShipmentEventProcessor
{
    Task ProcessMessageAsync(string eventType, string jsonPayload, CancellationToken ct = default);
}