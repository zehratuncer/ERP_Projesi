using ERP.Application.Common.Interfaces;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.BackgroundServices;

public class StockAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockAlertBackgroundService> _logger;

    public StockAlertBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<StockAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kırtasiye Stok Takip & Alarm Arka Plan Servisi Başlatıldı.");

        // Uygulama ilk ayağa kalktığında 10 saniye bekle
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckLowStockProductsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok kontrol arka plan görevi yürütülürken hata oluştu: {Message}", ex.Message);
            }

            // Her 4 saatte bir periyodik olarak kontrol et
            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }

    private async Task CheckLowStockProductsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var lowStockProducts = await context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.CurrentStock <= p.MinStockLevel)
            .ToListAsync(cancellationToken);

        if (lowStockProducts.Count > 0)
        {
            var productNames = string.Join(", ", lowStockProducts.Take(3).Select(p => $"{p.Name} ({p.CurrentStock} {p.Unit})"));
            var remainingCount = lowStockProducts.Count - 3;
            var summary = remainingCount > 0 ? $"{productNames} ve {remainingCount} diğer ürün" : productNames;

            var title = "⚠️ Kritik Kırtasiye Stok Alarmı";
            var message = $"Depoda kritik eşiğin altına düşen {lowStockProducts.Count} çeşit ürün bulunuyor: {summary}.";

            // Admin ve Depo Sorumlularına bildirim gönder
            await notificationService.SendNotificationAsync(
                null,
                "Admin",
                title,
                message,
                NotificationType.StockAlert,
                "/inventory",
                cancellationToken);

            await notificationService.SendNotificationAsync(
                null,
                "WarehouseManager",
                title,
                message,
                NotificationType.StockAlert,
                "/inventory",
                cancellationToken);

            _logger.LogInformation("Kritik stok alarmı verildi: {Count} adet ürün eşiğin altında.", lowStockProducts.Count);
        }
    }
}
