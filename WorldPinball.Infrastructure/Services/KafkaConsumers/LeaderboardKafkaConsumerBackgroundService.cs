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


        try // cancellation 
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try // topic
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
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    // Topic does not exist yet; wait for it to be auto-created
                    logger.LogWarning("Kafka topic 'game-sessions' not available yet. Retrying in 2s...");
                    await Task.Delay(2000, stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Kafka consumption error: {Reason}", ex.Error.Reason);
                    await Task.Delay(1000, stoppingToken);
                }
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