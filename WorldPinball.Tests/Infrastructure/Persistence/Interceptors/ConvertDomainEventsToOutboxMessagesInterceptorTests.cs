using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Events;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Persistence.Interceptors;

namespace WorldPinball.Tests.Infrastructure.Persistence.Interceptors;

public class ConvertDomainEventsToOutboxMessagesInterceptorTests
{
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly IOutboxNotificationChannel _channel = 
        Substitute.For<IOutboxNotificationChannel>();
    private readonly AppDbContext _db;

    public ConvertDomainEventsToOutboxMessagesInterceptorTests()
    {
        _timeProvider.SetUtcNow(
            new DateTimeOffset(
                2026, 9, 25, 12, 0, 0, TimeSpan.Zero
                ));
        
        var interceptor = new ConvertDomainEventsToOutboxMessagesInterceptor(
            _timeProvider,
            _channel);
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        
        _db = new AppDbContext(options);
    }


    [Fact]
    public async Task SavingChangesAsync_WhenEntityHasDomainEvents_ShouldCreateOutboxMessageAndClearEvents()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", now);
        
        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();
        var messages = await _db.OutboxMessages.ToListAsync();
        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(nameof(GameSessionCreatedDomainEvent));
        messages[0].OccurredAtUtc.Should().Be(now);
        session.DomainEvents.Should().BeEmpty();
    }
    
    [Fact]
    public async Task SavingChangesAsync_WhenEntityHasNoDomainEvents_ShouldNotCreateOutboxMessages()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", now);
        session.ClearDomainEvents();
        
        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();
      
        var messages = await _db.OutboxMessages.ToListAsync();
        messages.Should().BeEmpty();
    }
}