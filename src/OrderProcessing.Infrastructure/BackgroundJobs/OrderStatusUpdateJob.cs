using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessing.Application.Interfaces;

namespace OrderProcessing.Infrastructure.BackgroundJobs;

public sealed class OrderStatusUpdateJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderStatusUpdateJob> _logger;

    public OrderStatusUpdateJob(IServiceScopeFactory scopeFactory, ILogger<OrderStatusUpdateJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderStatusUpdateJob started. Will run every {Interval} minutes.", Interval.TotalMinutes);

        using var timer = new PeriodicTimer(Interval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunJobAsync(stoppingToken);
        }
    }

    private async Task RunJobAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderStatusUpdateJob: starting run at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            await orderService.ProcessPendingOrdersAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "OrderStatusUpdateJob: unhandled error during run");
        }
    }
}
