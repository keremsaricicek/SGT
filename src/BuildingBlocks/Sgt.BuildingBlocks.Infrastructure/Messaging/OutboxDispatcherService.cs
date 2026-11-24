using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Outbox'ı sürekli boşaltan arka plan servisi. Boşta 1 saniye bekler.</summary>
public sealed partial class OutboxDispatcherService<TContext>(
    OutboxProcessor<TContext> processor,
    ILogger<OutboxDispatcherService<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = 0;
            try
            {
                processed = await processor.ProcessBatch(BatchSize, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
#pragma warning disable CA1031 // Servis düşmemeli; hata loglanır ve döngü sürer.
            catch (Exception e)
#pragma warning restore CA1031
            {
                LogBatchFailed(logger, typeof(TContext).Name, e);
            }

            if (processed < BatchSize)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox işlenemedi: {Context}")]
    private static partial void LogBatchFailed(ILogger logger, string context, Exception exception);
}
