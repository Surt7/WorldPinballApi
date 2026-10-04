using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Outbox;

public static class KafkaProducerExtensions
{
    public static IServiceCollection AddKafkaProducer(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
                Acks = Acks.All,
                EnableIdempotence = true
            };
            return new ProducerBuilder<string, string>(producerConfig).Build();
        });

        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }
}
