using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Features.Reports.Queries.GetDeadStock;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.UnitTests.Common;
using Xunit;

namespace ERP.UnitTests.Reports;

public class AnalyticsReportCalculationTests
{
    [Fact]
    public async Task GetDeadStock_ShouldIdentifyInactiveProductsAndCalculateTiedUpCapital()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var products = context.Products.Take(2).ToList();
        var p1 = products[0];
        var p2 = products[1];

        // Add a recent outbound movement for p1 (Today)
        context.InventoryTransactions.Add(new InventoryTransaction
        {
            ProductId = p1.Id,
            Quantity = 5,
            TransactionType = TransactionType.Out,
            TransactionDate = DateTime.UtcNow.AddDays(-2),
            Description = "Aktif Satış"
        });

        // Add an old outbound movement for p2 (120 days ago)
        context.InventoryTransactions.Add(new InventoryTransaction
        {
            ProductId = p2.Id,
            Quantity = 2,
            TransactionType = TransactionType.Out,
            TransactionDate = DateTime.UtcNow.AddDays(-120),
            Description = "Eski Hareket"
        });

        await context.SaveChangesAsync();

        var handler = new GetDeadStockQueryHandler(context);
        var query = new GetDeadStockQuery(90); // 90 days threshold

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // p1 had a movement 2 days ago, so it should NOT be in 90-day dead stock
        result.Data!.DeadStockItems.Any(i => i.ProductId == p1.Id).Should().BeFalse();

        // p2 had no movement in 90 days, so it SHOULD be in dead stock
        var deadP2 = result.Data.DeadStockItems.FirstOrDefault(i => i.ProductId == p2.Id);
        deadP2.Should().NotBeNull();
        deadP2!.TotalTiedUpValue.Should().Be(p2.CurrentStock * p2.UnitPrice);
        deadP2.DaysInactive.Should().BeGreaterThanOrEqualTo(90);
    }
}
