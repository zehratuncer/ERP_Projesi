using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.PurchaseRequests.Commands.ConvertPurchaseRequestToInventory;
using ERP.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.UnitTests.Common;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ERP.UnitTests.PurchaseRequests;

public class PurchaseRequestWorkflowTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly CreatePurchaseRequestCommandValidator _validator = new();

    public PurchaseRequestWorkflowTests()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task CreatePurchaseRequest_WithValidData_ShouldCreateRequestAndCalculateTotal()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var products = context.Products.Take(2).ToList();
        var p1 = products[0];
        var p2 = products[1];

        var items = new List<CreatePurchaseRequestItemRequest>
        {
            new(p1.Id, 10, p1.Unit, 750m, "Toplu alım iskonto teklifi"), // 10 * 750 = 7500
            new(p2.Id, 20, p2.Unit, 60m, "Okul öncesi stok tamamlama")   // 20 * 60 = 1200
        };

        var handler = new CreatePurchaseRequestCommandHandler(context, _currentUserServiceMock.Object, _notificationServiceMock.Object);
        var command = new CreatePurchaseRequestCommand("Kırtasiye Mağaza", RequestPriority.High, DateTime.UtcNow.AddDays(7), "Okul sezonu hazırlık siparişi", items, true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(RequestStatus.PendingApproval);
        result.Data.TotalEstimatedAmount.Should().Be(8700m); // 7500 + 1200
        result.Data.Items.Should().HaveCount(2);

        var savedRequest = context.PurchaseRequests.FirstOrDefault(r => r.Id == result.Data.Id);
        savedRequest.Should().NotBeNull();
        savedRequest!.Department.Should().Be("Kırtasiye Mağaza");
    }

    [Fact]
    public async Task ConvertToInventory_OnApprovedRequest_ShouldIncreaseProductStockAndSetCompleted()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var p1 = context.Products.First();
        int initialStock = p1.CurrentStock;
        int requestedQty = 25;

        var request = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = "TALEP-20260826-001",
            Department = "Kırtasiye Mağaza",
            Priority = RequestPriority.Medium,
            Status = RequestStatus.Approved, // Ready for receiving
            TotalEstimatedAmount = requestedQty * p1.UnitPrice,
            Items = new List<PurchaseRequestItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = p1.Id,
                    RequestedQuantity = requestedQty,
                    Unit = p1.Unit,
                    EstimatedUnitPrice = p1.UnitPrice
                }
            }
        };

        context.PurchaseRequests.Add(request);
        await context.SaveChangesAsync();

        var handler = new ConvertPurchaseRequestToInventoryCommandHandler(context, _currentUserServiceMock.Object);
        var command = new ConvertPurchaseRequestToInventoryCommand(request.Id, "İrsaliye No: IRS-98765");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be(RequestStatus.Completed);

        // Verify product stock increased
        p1.CurrentStock.Should().Be(initialStock + requestedQty);

        // Verify stock transaction recorded
        var invTx = context.InventoryTransactions.FirstOrDefault(t => t.Description!.Contains("TALEP-20260826-001"));
        invTx.Should().NotBeNull();
        invTx!.TransactionType.Should().Be(TransactionType.In);
        invTx.Quantity.Should().Be(requestedQty);
    }

    [Fact]
    public async Task ConvertToInventory_OnPendingApprovalRequest_ShouldFail()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var p1 = context.Products.First();

        var request = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = "TALEP-20260826-002",
            Department = "Ofis Kırtasiye",
            Priority = RequestPriority.Low,
            Status = RequestStatus.PendingApproval, // NOT approved yet!
            TotalEstimatedAmount = 1000m,
            Items = new List<PurchaseRequestItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = p1.Id, RequestedQuantity = 5, Unit = "Koli", EstimatedUnitPrice = 200m }
            }
        };

        context.PurchaseRequests.Add(request);
        await context.SaveChangesAsync();

        var handler = new ConvertPurchaseRequestToInventoryCommandHandler(context, _currentUserServiceMock.Object);
        var command = new ConvertPurchaseRequestToInventoryCommand(request.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Yalnızca onaylanmış (Approved) durumdaki");
    }

    [Fact]
    public void CreatePurchaseRequestValidator_WithEmptyItems_ShouldFailValidation()
    {
        var command = new CreatePurchaseRequestCommand("Kırtasiye", RequestPriority.Medium, null, null, new List<CreatePurchaseRequestItemRequest>());
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }
}
