using kr5.Data;
using Microsoft.EntityFrameworkCore;

namespace kr5.Services;

public class OrderProgressService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OrderProgressService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

    public OrderProgressService(IServiceProvider services, ILogger<OrderProgressService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис прогресса заказов запущен");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке заказов");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessOrdersAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductionDbContext>();

        var orders = await context.WorkOrders
            .Include(x => x.ProductionLine)
            .Where(x => x.Status == "InProgress" && x.ProductionLine != null && x.ProductionLine.IsAutomatic)
            .ToListAsync(stoppingToken);

        foreach (var order in orders)
        {
            if (stoppingToken.IsCancellationRequested) break;

            // Увеличиваем прогресс на 20% за тик (для быстрого тестирования)
            order.ProgressPercent = Math.Min(100, order.ProgressPercent + 20);

            if (order.ProgressPercent >= 100)
            {
                order.Status = "Completed";
                if (order.ProductionLine != null)
                {
                    order.ProductionLine.CurrentWorkOrderId = null;
                }
                _logger.LogInformation("Заказ #{OrderId} завершён", order.Id);
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
