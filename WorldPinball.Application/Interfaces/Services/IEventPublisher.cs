namespace WorldPinball.Application.Interfaces.Services;

public interface IEventPublisher
{
    Task PublishAsync(string topic, string key, string payload, CancellationToken ct = default);
}