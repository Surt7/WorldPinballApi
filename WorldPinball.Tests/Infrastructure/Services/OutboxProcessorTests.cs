using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Services;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Infrastructure.Services;

public class OutboxProcessorTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly AppDbContext _db;
    private readonly OutboxProcessor _sut;
    

    public OutboxProcessorTests()
    {
        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero));
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new OutboxProcessor(_db, _publisher, _timeProvider);
    }
    
    [Fact]
    public async Task ProcessBatchAsync_WhenUnprocessedMessagesExist_ShouldPublishAndMarkProcessed()
    {
        // Arrange
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var message = new OutboxMessage(
            Guid.NewGuid(), "GameSessionEndedDomainEvent", "{\"Score\":1000}", now);
        _db.OutboxMessages.Add(message);
        await _db.SaveChangesAsync();
        // Act
        await _sut.ProcessBatchAsync();
        // Assert
        await _publisher.Received(1).PublishAsync(
            "game-sessions", message.Id.ToString(), message.Content, Arg.Any<CancellationToken>());
        var updated = await _db.OutboxMessages.FirstAsync();
        updated.ProcessedAtUtc.Should().Be(now);
        updated.Error.Should().BeNull();
    }


    [Fact]
    public async Task ProcessBatchAsync_WhenPublisherFails_ShouldRecordFailure()
    {
        
        
        _publisher
            .PublishAsync(
                Arg.Any<string>(),            
                Arg.Any<string>(),             
                Arg.Any<string>(),           
                Arg.Any<CancellationToken>())  
            .ThrowsAsync(new InvalidOperationException("Kafka broker connection timeout"));
        
        var message = new OutboxMessage(
            Guid.NewGuid(),
            "GameSessionEndedDomainEvent",
            "{}",
            _timeProvider.GetUtcNow().UtcDateTime
            );

        _db.OutboxMessages.Add(message);
        await _db.SaveChangesAsync();
        
        await _sut.ProcessBatchAsync();


        var updated = await _db.OutboxMessages.FirstAsync();
        
        updated.ProcessedAtUtc.Should().BeNull();
        updated.RetryCount.Should().Be(1);
        updated.Error.Should().Be("Kafka broker connection timeout");
        
     
    }
}