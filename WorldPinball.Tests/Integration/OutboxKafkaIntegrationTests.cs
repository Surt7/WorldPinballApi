using Confluent.Kafka;
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
        
     
        
        var producerConfig  = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        var producer = new ProducerBuilder<string,string>(producerConfig ).Build();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = $"test-group-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string , string> (consumerConfig).Build();

        consumer.Subscribe("game-sessions");
        
        var publisher = new KafkaEventPublisher(producer);
        var processor = new OutboxProcessor(dbContext, publisher, TimeProvider.System);
        
        await processor.ProcessBatchAsync(CancellationToken.None);


        var updatedMessage = await dbContext.OutboxMessages.FindAsync(message.Id);

        updatedMessage.Should().NotBeNull();
        updatedMessage!.ProcessedAtUtc.Should().NotBeNull();
        updatedMessage.Error.Should().BeNull();
        
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        ConsumeResult<string, string>? foundMessage = null;
        while (!cts.IsCancellationRequested)
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(1));
            if (result?.Message.Key == message.Id.ToString())
            {
                foundMessage = result;
                break;
            }
        }
        foundMessage.Should().NotBeNull("Expected message was not published to Kafka topic within timeout.");
    }
}