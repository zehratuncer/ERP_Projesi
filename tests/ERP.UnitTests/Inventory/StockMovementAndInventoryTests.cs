using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Inventory.Commands.CreateStockMovement;
using ERP.Domain.Enums;
using ERP.UnitTests.Common;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ERP.UnitTests.Inventory;

public class StockMovementAndInventoryTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly CreateStockMovementCommandValidator _validator = new();

    public StockMovementAndInventoryTests()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task CreateStockMovement_In_ShouldIncreaseProductCurrentStock()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var product = context.Products.First();
        int initialStock = product.CurrentStock;
        int inQuantity = 50;

        var handler = new CreateStockMovementCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CreateStockMovementCommand(product.Id, inQuantity, TransactionType.In, "Yeni sevkiyat");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        product.CurrentStock.Should().Be(initialStock + inQuantity);

        var transaction = context.InventoryTransactions.FirstOrDefault(t => t.ProductId == product.Id && t.Description == "Yeni sevkiyat");
        transaction.Should().NotBeNull();
        transaction!.TransactionType.Should().Be(TransactionType.In);
        transaction.Quantity.Should().Be(inQuantity);
    }

    [Fact]
    public async Task CreateStockMovement_Out_WithSufficientStock_ShouldDecreaseProductCurrentStock()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var product = context.Products.First();
        product.CurrentStock = 100; // ensure sufficient stock
        await context.SaveChangesAsync();

        int initialStock = product.CurrentStock;
        int outQuantity = 30;

        var handler = new CreateStockMovementCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CreateStockMovementCommand(product.Id, outQuantity, TransactionType.Out, "Şube mağazaya çıkış");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        product.CurrentStock.Should().Be(initialStock - outQuantity); // 70
    }

    [Fact]
    public async Task CreateStockMovement_Out_WithInsufficientStock_ShouldThrowBusinessException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var product = context.Products.First();
        product.CurrentStock = 5;
        await context.SaveChangesAsync();

        int excessiveQuantity = 25; // 25 > 5

        var handler = new CreateStockMovementCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CreateStockMovementCommand(product.Id, excessiveQuantity, TransactionType.Out, "Aşırı çıkış denemesi");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Yetersiz stok!*");
    }

    [Fact]
    public async Task CreateStockMovement_Adjustment_ShouldSetExactStockValue()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var product = context.Products.First();
        int countResult = 42; // Yıl sonu sayım sonucu

        var handler = new CreateStockMovementCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CreateStockMovementCommand(product.Id, countResult, TransactionType.Adjustment, "Yıl Sonu Depo Sayım Düzeltmesi");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        product.CurrentStock.Should().Be(countResult);
    }

    [Fact]
    public async Task CreateStockMovement_NonExistentProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var nonExistentId = Guid.NewGuid();
        var handler = new CreateStockMovementCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CreateStockMovementCommand(nonExistentId, 10, TransactionType.In, "Geçersiz ürün");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void StockMovementValidator_WithInvalidQuantity_ShouldHaveValidationError(int quantity)
    {
        var command = new CreateStockMovementCommand(Guid.NewGuid(), quantity, TransactionType.In, "Not");
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }
}
