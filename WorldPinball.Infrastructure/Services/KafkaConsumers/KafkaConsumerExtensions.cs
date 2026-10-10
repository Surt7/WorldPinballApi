using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WorldPinball.Infrastructure.Services.KafkaConsumers;

public static class KafkaConsumerExtensions
{
    public static IServiceCollection AddLeaderboardKafkaConsumer(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "worldpinball-leaderboard-updater",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                // Cooperative Sticky prevent the rebalancing overhead
                PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
                
                // Only read committed messages up to Last Stable Offset
                IsolationLevel = IsolationLevel.ReadCommitted 
            };
            return new ConsumerBuilder<string, string>(consumerConfig).Build();
        });
        services.AddScoped<ILeaderboardScoreEventProcessor, LeaderboardScoreEventProcessor>();
        services.AddHostedService<LeaderboardKafkaConsumerBackgroundService>();
        
        return services;
    }
}