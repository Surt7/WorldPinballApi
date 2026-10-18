using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using WorldPinball.Domain.Entities;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Infrastructure.Services;

public class OutboxCleanerTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly AppDbContext _db;
    private readonly OutboxCleaner _sut;

    public OutboxCleanerTests()
    {
        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 9, 29, 12, 0, 0, TimeSpan.Zero));
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new OutboxCleaner(_db, _timeProvider);
    }

    [Fact]
    public async Task CleanUpBatchAsync_WhenProcessedMessagesOlderThanRetention_ShouldDeleteThem()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var message = new OutboxMessage(Guid.NewGuid(), "Event", "{}", now.AddDays(-8));
        message.MarkAsProcessed(now.AddDays(-8));
        _db.OutboxMessages.Add(message);
        await _db.SaveChangesAsync();

        var deletedCount = await _sut.CleanupBatchAsync(TimeSpan.FromDays(7));

        deletedCount.Should().Be(1);
        var remaining = await _db.OutboxMessages.ToListAsync();
        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task CleanupBatchAsync_WhenProcessedMessagesWithinRetention_ShouldNotDelete()
    {
        
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var message = new OutboxMessage(Guid.NewGuid(), "Event", "{}", now.AddDays(-2));
        message.MarkAsProcessed(now.AddDays(-2));
        
        _db.OutboxMessages.Add(message);
        await _db.SaveChangesAsync();
        
        var deletedCount = await _sut.CleanupBatchAsync(TimeSpan.FromDays(7));
        
        deletedCount.Should().Be(0);
        var remaining = await _db.OutboxMessages.ToListAsync();
        remaining.Should().ContainSingle();
    }

    [Fact]
    public async Task CleanupBatchAsync_WhenMoreMessagesThanBatchSize_ShouldOnlyDeleteBatchSize()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        for (int i = 0; i < 5; i++)
        {
            var message = new OutboxMessage(Guid.NewGuid(), "Event", "{}", now.AddDays(-10));
            message.MarkAsProcessed(now.AddDays(-10));
            _db.OutboxMessages.Add(message);
            
        }
        await _db.SaveChangesAsync();
        
        var deletedCount = await _sut.CleanupBatchAsync(TimeSpan.FromDays(7),batchSize: 2);
        
        
       deletedCount.Should().Be(2);
       
       var remaining = await _db.OutboxMessages.ToListAsync();
       remaining.Should().HaveCount(3);
    }
    
}