using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Infrastructure.OptionConfigurations;

namespace WorldPinball.Logistics.Infrastructure.Services.RabbitMq;

public sealed class RabbitMqLogisticsEventPublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqLogisticsEventPublisher> logger
    ) :ILogisticsEventPublisher, IAsyncDisposable
{
    private bool _disposed;
    private readonly RabbitMqOptions _options = options.Value;
    private readonly ILogger<RabbitMqLogisticsEventPublisher> _logger = logger;
    private IConnection? _connection;
    
    private async Task<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }
        _connection = await _factory.CreateConnectionAsync(ct);
        return _connection;
    }

    private readonly ConnectionFactory _factory = new()
    {
        HostName = options.Value.HostName,
        Port = options.Value.Port,
        UserName = options.Value.UserName,
        Password = options.Value.Password
    };

    public async Task PublishAsync(string topic, string key, string message, CancellationToken ct = default)
    {
        
        
        // Defensive clause guard
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (!_disposed)
        {
            var connection = await GetConnectionAsync(ct);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

            await channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: ct);

            var body = System.Text.Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                ContentType = "application/json",
                MessageId = key
            };

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: topic,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);

            _logger.LogInformation("Published logistics event {Key} to {Exchange}/{RoutingKey}", key,
                _options.ExchangeName, topic);
        }
        else
        {
            throw new ObjectDisposedException(nameof(RabbitMqLogisticsEventPublisher));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}