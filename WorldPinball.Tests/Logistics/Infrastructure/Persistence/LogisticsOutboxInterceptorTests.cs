namespace WorldPinball.Tests.Logistics.Persistence;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Logistics.Application.Interfaces;
using WorldPinball.Logistics.Domain.Entities;
using WorldPinball.Logistics.Domain.Events;
using WorldPinball.Logistics.Infrastructure.Persistence;
using WorldPinball.Logistics.Infrastructure.Persistence.Interceptors;


public class LogisticsOutboxInterceptorTests
{
    
        private readonly FakeTimeProvider _timeProvider = new();
        private readonly ILogisticsOutboxChannel _channel = Substitute.For<ILogisticsOutboxChannel>();
        private readonly LogisticsDbContext _db;
        
        
        public LogisticsOutboxInterceptorTests()
        {
            _timeProvider.SetUtcNow(new DateTimeOffset(2026, 10, 2, 12, 0, 0, TimeSpan.Zero));
            var interceptor = new ConvertDomainEventsToOutboxMessagesInterceptor(_timeProvider, _channel);
            var options = new DbContextOptionsBuilder<LogisticsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;
            _db = new LogisticsDbContext(options);
        }


        [Fact]
        public async Task SavingChangesAsync_WhenEntityHasDomainEvents_ShouldCreateOutboxMessageAndNotifyChannel()
        {
            // Arrange
            var shipment = Shipment.Create("WC-98421", "1610 W Carroll Ave, Chicago, IL 60612, USA");
            shipment.Cancel("Machine damaged in warehouse");
            _db.Shipments.Add(shipment);
            
            
            // Act
            await _db.SaveChangesAsync();
            
            
            
            // Assert
            var outboxMessage = await _db.OutboxMessages.FirstOrDefaultAsync();
            outboxMessage.Should().NotBeNull();
            outboxMessage!.Type.Should().Be(nameof(ShipmentCancelledDomainEvent));
            
            // Assert
            shipment.DomainEvents.Should().BeEmpty();
            
            // Assert
            await _channel.Received(1).TryNotifyAsync(Arg.Any<CancellationToken>());
        }
        
        
        [Fact]
        public async Task SaveChangesFailedSync_WhenSaveFails_ShouldNotNotifyChannel()
        {
            
           
            var shipment = Shipment.Create("WC-99999", "742 Evergreen Terrace, Springfield, USA");
            shipment.Cancel("Order cancelled by customer");
            _db.Shipments.Add(shipment);
            
            
            
            // to simulate a cancellation
            Func<Task> act = () => _db.SaveChangesAsync(new CancellationToken(canceled: true));


          await act.Should().ThrowAsync<OperationCanceledException>();
            await _channel.DidNotReceive().TryNotifyAsync(Arg.Any<CancellationToken>());
        }
}