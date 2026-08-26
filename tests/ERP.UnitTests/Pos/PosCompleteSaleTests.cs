using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Pos.Commands.CompleteSale;
using ERP.Domain.Enums;
using ERP.UnitTests.Common;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ERP.UnitTests.Pos;

public class PosCompleteSaleTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly CompleteSaleCommandValidator _validator = new();

    public PosCompleteSaleTests()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task CompleteSale_WithValidItems_ShouldCalculateAmountsAndDeductStock()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var products = context.Products.Take(2).ToList();
        var p1 = products[0];
        var p2 = products[1];

        p1.UnitPrice = 780m;
        p1.CurrentStock = 100;

        p2.UnitPrice = 65m;
        p2.CurrentStock = 50;

        await context.SaveChangesAsync();

        var items = new List<CompleteSaleItemRequest>
        {
            new(p1.Id, 2, null, 0m),     // 2 * 780 = 1560
            new(p2.Id, 4, null, 10m)    // 4 * 65 = 260 - %10 (26) = 234
        };

        var handler = new CompleteSaleCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CompleteSaleCommand(items, PaymentMethod.CreditCard, "Perakende Müşteri", 4m); // 4 TL genel indirim

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // 1560 + 260 = 1820 Gross Total
        result.Data!.TotalAmount.Should().Be(1820m);
        // Total Discount = 26 (item) + 4 (general) = 30m
        result.Data.DiscountAmount.Should().Be(30m);
        // Final Amount = 1820 - 30 = 1790m
        result.Data.FinalAmount.Should().Be(1790m);

        // Stock deduction check
        p1.CurrentStock.Should().Be(98); // 100 - 2
        p2.CurrentStock.Should().Be(46); // 50 - 4

        // Inventory Transactions check
        var transactions = context.InventoryTransactions.Where(t => t.TransactionType == TransactionType.Out).ToList();
        transactions.Should().NotBeEmpty();

        // Sale and SaleItem records check
        var savedSale = context.Sales.FirstOrDefault(s => s.Id == result.Data.Id);
        savedSale.Should().NotBeNull();
        savedSale!.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
    }

    [Fact]
    public async Task CompleteSale_WithInsufficientStock_ShouldThrowBusinessException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var p = context.Products.First();
        p.CurrentStock = 10;
        await context.SaveChangesAsync();

        var items = new List<CompleteSaleItemRequest>
        {
            new(p.Id, 15, null, 0m) // 15 > 10
        };

        var handler = new CompleteSaleCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CompleteSaleCommand(items, PaymentMethod.Cash);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Yetersiz stok!*");
    }

    [Fact]
    public async Task CompleteSale_WhenStockDropsBelowMinimum_ShouldTriggerCriticalAlert()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var p1 = context.Products.First();
        p1.CurrentStock = 100;
        p1.MinStockLevel = 20;
        await context.SaveChangesAsync();

        var items = new List<CompleteSaleItemRequest>
        {
            new(p1.Id, 85, null, 0m) // 100 - 85 = 15 (15 <= 20, critical!)
        };

        var handler = new CompleteSaleCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CompleteSaleCommand(items, PaymentMethod.Cash);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data!.CriticalStockAlerts.Should().NotBeEmpty();
        result.Data.CriticalStockAlerts.First().Should().Contain("kritik stok seviyesine düştü");
        p1.CurrentStock.Should().Be(15);
    }

    [Fact]
    public void CompleteSaleValidator_EmptyItems_ShouldFailValidation()
    {
        var command = new CompleteSaleCommand(new List<CompleteSaleItemRequest>(), PaymentMethod.Cash);
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }
}
