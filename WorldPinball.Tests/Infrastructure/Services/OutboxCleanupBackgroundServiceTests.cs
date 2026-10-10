using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Infrastructure.Services;

public class OutboxCleanupBackgroundServiceTests
{
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IServiceScope _scope = Substitute.For<IServiceScope>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly IOutboxCleaner _cleaner = Substitute.For<IOutboxCleaner>();
    private readonly OutboxCleanupChannel _channel = new();
    private readonly OutboxCleanupBackgroundService _sut;

    public OutboxCleanupBackgroundServiceTests()
    {
        _scopeFactory.CreateScope().Returns(_scope);
        _scope.ServiceProvider.Returns(_serviceProvider);
        _serviceProvider.GetService(typeof(IOutboxCleaner)).Returns(_cleaner);
        _sut = new OutboxCleanupBackgroundService(
            _scopeFactory, 
            _channel, 
            NullLogger<OutboxCleanupBackgroundService>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSignalReceived_ShouldInvokeCleaner()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        await _channel.RequestCleanupAsync(cts.Token);
        
        await _sut.StartAsync(cts.Token);
        await Task.Delay(100, cts.Token);
        await _sut.StopAsync(CancellationToken.None);
        
        
        // Assert: cleaner should be invoked with default retention (e.g. 7 days)
        await _cleaner.Received().CleanupBatchAsync(
            Arg.Any<TimeSpan>(), 
            Arg.Any<int>(), 
            Arg.Any<CancellationToken>());
    }

}