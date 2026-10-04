using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace WorldPinball.Infrastructure.Services.KafkaConsumers;

public class LeaderboardKafkaConsumerBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<LeaderboardKafkaConsumerBackgroundService> logger,
    IConsumer<string, string> consumer
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe("game-sessions");
        logger.LogInformation("Leaderboard Kafka consumer started on topic 'game-sessions'.");
        /* As we work on async we have to give back the hand to
         server as Kafka method as it's synchronous
          (by definition , blocking the thread)   */
        await Task.Yield();


        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null) continue;

                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ILeaderboardScoreEventProcessor>();
                await processor.ProcessMessageAsync(result.Message.Value, stoppingToken);

                consumer.Commit(result);
                
                logger.LogDebug("Offset {Offset} committed for partition {Partition}.",
                    result.Offset.Value, result.Partition.Value);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Leaderboard Kafka consumer shutdown requested.");
        }
        finally
        {
            consumer.Close();
        }
    }
}