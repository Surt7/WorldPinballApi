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
        try
        {
            // Enter the loop immediately and wait for EITHER channel OR timer
            while (!stoppingToken.IsCancellationRequested)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                try
                {
                    // The missing line: WAIT for signal or 5s timeout
                    await channel.Reader.WaitToReadAsync(timeoutCts.Token);
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // 5-second fallback reached
                    // No action required: 5s timeout reached, proceed to batch polling
                    logger.LogDebug("Outbox fallback: no signal received within 5s, running periodic check.");
                }
                
                // Drain all pending signals from buffer
                while (channel.Reader.TryRead(out _)) { }

                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                    await processor.ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
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