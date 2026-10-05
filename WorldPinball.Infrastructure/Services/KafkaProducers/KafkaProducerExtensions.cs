using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.KafkaProducers;

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
                EnableIdempotence = true,
                MaxInFlight = 5 ,// Preserves strict ordering while maximizing network pipelining
                CompressionType = CompressionType.Lz4, // High compression speed with minimal CPU overhead
                LingerMs = 20, // Wait up to 20ms to allow batch accumulation
                BatchSize = 1024 * 1024 // 1MB batch size for high-throughput buffering
            };
            return new ProducerBuilder<string, string>(producerConfig).Build();
        });

        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }
}
