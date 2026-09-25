using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WorldPinball.Domain.Entities;
using WorldPinball.Infrastructure;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Integration;

public class OutboxKafkaIntegrationTests
{
    

    [Fact]
    public async Task ProcessBatchAsync_WhenUnprocessedMessageExists_PublishesToKafkaAndMarksProcessed()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<OutboxKafkaIntegrationTests>() 
            .AddEnvironmentVariables() 
            .Build();

   
        
        var connectionString = config.GetRequiredPostgresConnectionString();
        
        var message = new OutboxMessage(
            Guid.NewGuid(), 
            "GameSessionStartedDomainEvent", 
            "{}", DateTime.UtcNow);
        
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.MigrateAsync();
        dbContext.OutboxMessages.Add(message);
        await dbContext.SaveChangesAsync();

        var adminConfig = new AdminClientConfig { BootstrapServers = "localhost:9092" };
        using (var adminClient = new AdminClientBuilder(adminConfig).Build())
        {
            try
            {
                await adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = "game-sessions", NumPartitions = 1, ReplicationFactor = 1 }
                });
            }
            catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                // Topic already exists
            }
        }

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = $"test-group-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

        consumer.Subscribe("game-sessions");
        
        var publisher = new KafkaEventPublisher(producer);
        var processor = new OutboxProcessor(dbContext, publisher, TimeProvider.System);
        
        await processor.ProcessBatchAsync(CancellationToken.None);

        var updatedMessage = await dbContext.OutboxMessages.FindAsync(message.Id);
        await dbContext.Entry(updatedMessage!).ReloadAsync();

        updatedMessage.Should().NotBeNull();
        updatedMessage!.Error.Should().BeNull();
        updatedMessage.ProcessedAtUtc.Should().NotBeNull();
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        ConsumeResult<string, string>? foundMessage = null;
        while (!cts.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result?.Message.Key == message.Id.ToString())
                {
                    foundMessage = result;
                    break;
                }
            }
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                // Wait for topic metadata propagation
                await Task.Delay(200);
            }
        }
        foundMessage.Should().NotBeNull("Expected message was not published to Kafka topic within timeout.");
    }
}