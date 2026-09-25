using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Services.Outbox;

public sealed class OutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOutboxNotificationChannel channel,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            // Enter the loop immediately and wait for EITHER channel OR timer
            while (!stoppingToken.IsCancellationRequested)
            {
                
                // Wait for EITHER an instant channel signal OR the fallback 5s timer
                await Task.WhenAny(
                    channel.Reader.WaitToReadAsync(stoppingToken).AsTask(),
                    timer.WaitForNextTickAsync(stoppingToken).AsTask());
                
                
                // Drain all pending signals from buffer
                while (channel.Reader.TryRead(out _)) { }
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                    await processor.ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while processing outbox batch.");
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            logger.LogInformation(
                "-- Outbox background service is shutting down.--> {Message}",
                ex.ToString());
        }
        
    
    }
}