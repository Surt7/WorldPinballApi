using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Interfaces.Services;
using System.Threading;

namespace WorldPinball.Infrastructure.Services.Outbox;

public sealed class OutboxCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOutboxCleanupChannel outboxCleanupChannel,
    ILogger<OutboxCleanupBackgroundService> logger
    ):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutCts.CancelAfter(TimeSpan.FromHours(1));
            
            try { await outboxCleanupChannel.Reader.WaitToReadAsync(timeoutCts.Token); }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { }
            
           

            while (outboxCleanupChannel.Reader.TryRead(out _)) ;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var cleaner = scope.ServiceProvider.GetRequiredService<IOutboxCleaner>();
                await cleaner.CleanupBatchAsync(TimeSpan.FromDays(7), ct: stoppingToken);
                
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error while executing outbox cleanup.");
            }
        }
    }
    
}