using Confluent.Kafka;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.KafkaProducers;

public class KafkaEventPublisher(IProducer<string, string> producer) :IEventPublisher
{
    public async Task PublishAsync(string topic, string key, string payload, CancellationToken ct = default)
    {

        var message = new Message<string, string>
        {
            Key = key,
            Value = payload
        };
        await producer.ProduceAsync(topic, message, ct);
        
    }
}