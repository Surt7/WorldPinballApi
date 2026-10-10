namespace WorldPinball.Logistics.Application.Interfaces;

public interface ILogisticsEventPublisher
{
    Task PublishAsync(string topic, string key, string message,
        CancellationToken ct = default);
}