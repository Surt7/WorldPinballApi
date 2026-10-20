using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Events;

namespace WorldPinball.Logistics.Infrastructure.Services.RabbitMq;

public class LogisticsShipmentEventProcessor(
    ILogger<LogisticsShipmentEventProcessor> logger
    ):ILogisticsShipmentEventProcessor
{
    public async Task ProcessMessageAsync(string eventType, string jsonPayload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(eventType) || string.IsNullOrWhiteSpace(jsonPayload))
        {
            logger.LogWarning("Received empty event type or payload.");
            return;
        }

        try
        {
            switch (eventType)
            {
                case nameof(ShipmentCreatedDomainEvent):
                    HandleCreated(jsonPayload);
                    break;
                case nameof(ShipmentCancelledDomainEvent):
                    HandleCancelled(jsonPayload);
                    break;
                default:
                    LogUnknown(eventType);
                    break;
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize event {EventType}", eventType);
        }
        
        
        await Task.CompletedTask;
    }



    private bool HandleCreated(string jsonPayload)
    {
        var created = JsonSerializer.Deserialize<ShipmentCreatedDomainEvent>(jsonPayload);
        if (created is null) return false;
        logger.LogInformation("Shipment created event received for Order {OrderId}", created.OrderId);
        return true;
    }

    private bool HandleCancelled(string jsonPayload)
    {
        var cancelled = JsonSerializer.Deserialize<ShipmentCancelledDomainEvent>(jsonPayload);
        if (cancelled is null) return false;
        logger.LogInformation("Shipment cancelled event received for Order {OrderId}, Reason: {Reason}", 
            cancelled.OrderId, cancelled.Reason);
        return true;
    }
    
    private void LogUnknown(string eventType) =>
        logger.LogWarning("Unknown event type: {EventType}", eventType);
}