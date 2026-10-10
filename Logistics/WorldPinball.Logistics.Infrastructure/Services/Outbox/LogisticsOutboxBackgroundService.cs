using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorldPinball.Logistics.Application.Interfaces;

namespace WorldPinball.Logistics.Infrastructure.Services.Outbox;

public sealed class LogisticsOutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogisticsOutboxChannel channel,
    ILogger<LogisticsOutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {


        while (!stoppingToken.IsCancellationRequested)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));


            try { await channel.Reader.WaitToReadAsync(timeoutCts.Token); }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { }


            while (channel.Reader.TryRead(out _)) { }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ILogisticsOutboxProcessor>();
                await processor.ProcessBatchAsync(stoppingToken);

            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "An error occurred processing batch");
            }
            
        }
    }
}