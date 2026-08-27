using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Export.DTOs;
using ERP.Application.Features.Inventory.Commands.CreateStockMovement;
using ERP.Application.Features.Pos.Commands.CompleteSale;
using ERP.Application.Features.PurchaseRequests.Commands.ApprovePurchaseRequest;
using ERP.Application.Features.PurchaseRequests.Commands.ConvertPurchaseRequestToInventory;
using ERP.Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Infrastructure.Services.Export;
using ERP.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ERP.UnitTests.Workflows;

public class E2EWorkflowScenariosTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Guid _cashierUserId = Guid.NewGuid();
    private readonly Guid _managerUserId = Guid.NewGuid();

    public E2EWorkflowScenariosTests()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns(_cashierUserId);
    }

    [Fact]
    public async Task Scenario1_CompletePosSaleAndInventoryDeduction_WorkflowTest()
    {
        // 1. Arrange - Setup Database with initial stationery products
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var products = context.Products.Take(3).ToList();
        products.Should().HaveCount(3);

        var p1 = products[0]; // e.g. A4 Kağıt
        var p2 = products[1]; // e.g. Kurşun Kalem
        var p3 = products[2]; // e.g. Telli Dosya

        p1.CurrentStock = 50;
        p1.UnitPrice = 780m;

        p2.CurrentStock = 40;
        p2.UnitPrice = 65m;

        p3.CurrentStock = 100;
        p3.UnitPrice = 15m;

        await context.SaveChangesAsync();

        // 2. Act - Cashier scans 3 stationery items into cart and completes sale
        var items = new List<CompleteSaleItemRequest>
        {
            new(p1.Id, 5, null, 0m),     // 5 * 780 = 3900 TL
            new(p2.Id, 10, null, 10m),   // 10 * 65 = 650 TL - %10 (65) = 585 TL
            new(p3.Id, 20, null, 0m)     // 20 * 15 = 300 TL
        };

        var handler = new CompleteSaleCommandHandler(context, _currentUserServiceMock.Object);
        var command = new CompleteSaleCommand(
            Items: items,
            PaymentMethod: PaymentMethod.Cash,
            CustomerName: "Ahmet Yılmaz (Bireysel)",
            GeneralDiscountAmount: 85m // Ek 85 TL indirim -> Toplam: (3900 + 585 + 300) - 85 = 4700 TL
        );

        var response = await handler.Handle(command, CancellationToken.None);

        // 3. Assert - Verify Sale, Receipt, Stock Deduction, and Transaction Logs
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();

        var receipt = response.Data!;
        receipt.TotalAmount.Should().Be(4850m); // Brüt: 3900 + 650 + 300
        receipt.DiscountAmount.Should().Be(150m); // 65 + 85
        receipt.FinalAmount.Should().Be(4700m);
        receipt.PaymentMethod.Should().Be(PaymentMethod.Cash);
        receipt.Items.Should().HaveCount(3);
        receipt.ReceiptNumber.Should().StartWith("FIS-");

        // Verify that stock levels in DB are accurately deducted
        var updatedP1 = await context.Products.FindAsync(p1.Id);
        var updatedP2 = await context.Products.FindAsync(p2.Id);
        var updatedP3 = await context.Products.FindAsync(p3.Id);

        updatedP1!.CurrentStock.Should().Be(45); // 50 - 5
        updatedP2!.CurrentStock.Should().Be(30); // 40 - 10
        updatedP3!.CurrentStock.Should().Be(80); // 100 - 20

        // Verify Inventory Transactions
        var transactions = await context.InventoryTransactions
            .Where(t => t.TransactionType == TransactionType.Out)
            .ToListAsync();

        transactions.Should().HaveCountGreaterOrEqualTo(3);
        transactions.Any(t => t.ProductId == p1.Id && t.Quantity == 5).Should().BeTrue();
        transactions.Any(t => t.ProductId == p2.Id && t.Quantity == 10).Should().BeTrue();
        transactions.Any(t => t.ProductId == p3.Id && t.Quantity == 20).Should().BeTrue();
    }

    [Fact]
    public async Task Scenario2_LowStockAlertAndPurchaseRequestCreation_WorkflowTest()
    {
        // 1. Arrange - Setup product near critical threshold
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var product = context.Products.First();
        product.CurrentStock = 18;
        product.MinStockLevel = 15;
        await context.SaveChangesAsync();

        // 2. Act - Stock out movement drops stock below critical threshold
        var stockMovementHandler = new CreateStockMovementCommandHandler(context, _currentUserServiceMock.Object);
        var stockOutCommand = new CreateStockMovementCommand(
            ProductId: product.Id,
            Quantity: 10, // 18 - 10 = 8 (which is < 15, low stock!)
            TransactionType: TransactionType.Out,
            Description: "Hızlı mağaza sarfiyatı"
        );

        var stockOutResponse = await stockMovementHandler.Handle(stockOutCommand, CancellationToken.None);
        stockOutResponse.IsSuccess.Should().BeTrue();

        // Assert critical stock state (CurrentStock <= MinStockLevel)
        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct!.CurrentStock.Should().Be(8);
        (updatedProduct.CurrentStock <= updatedProduct.MinStockLevel).Should().BeTrue();

        // 3. Act - Create Purchase Request for the low-stock item
        var createRequestHandler = new CreatePurchaseRequestCommandHandler(context, _currentUserServiceMock.Object, _notificationServiceMock.Object);
        var purchaseRequestCommand = new CreatePurchaseRequestCommand(
            Department: "Mağaza & Satış",
            Priority: RequestPriority.High,
            RequiredDate: DateTime.UtcNow.AddDays(3),
            Note: "Kritik stok ikmali - Acil",
            Items: new List<CreatePurchaseRequestItemRequest>
            {
                new(product.Id, 50, product.Unit, product.UnitPrice, "Kritik stok seviyesine indiği için ikmal talebi")
            },
            SubmitForApproval: true
        );

        var requestResult = await createRequestHandler.Handle(purchaseRequestCommand, CancellationToken.None);

        // 4. Assert - Request created in PendingApproval status
        requestResult.IsSuccess.Should().BeTrue();
        requestResult.Data.Should().NotBeNull();
        requestResult.Data!.Status.Should().Be(RequestStatus.PendingApproval);
        requestResult.Data.Priority.Should().Be(RequestPriority.High);
        requestResult.Data.TotalEstimatedAmount.Should().Be(50 * product.UnitPrice);
    }

    [Fact]
    public async Task Scenario3_ManagerApprovalAndGoodsReceiptStockIncrease_WorkflowTest()
    {
        // 1. Arrange - Setup database with pending purchase request
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var product = context.Products.First();
        product.CurrentStock = 10;
        await context.SaveChangesAsync();

        var purchaseRequest = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = "PR-20260827-001",
            Department = "Kırtasiye Satın Alma",
            Priority = RequestPriority.Urgent,
            Status = RequestStatus.PendingApproval,
            TotalEstimatedAmount = 2500m,
            RequesterUserId = _cashierUserId,
            RequiredDate = DateTime.UtcNow.AddDays(2),
            Items = new List<PurchaseRequestItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    RequestedQuantity = 25,
                    Unit = product.Unit,
                    EstimatedUnitPrice = 100m,
                    EstimatedTotalPrice = 2500m
                }
            }
        };

        context.PurchaseRequests.Add(purchaseRequest);
        await context.SaveChangesAsync();

        // 2. Act - Manager reviews and Approves request
        var managerUserMock = new Mock<ICurrentUserService>();
        managerUserMock.Setup(x => x.UserId).Returns(_managerUserId);

        var approveHandler = new ApprovePurchaseRequestCommandHandler(context, managerUserMock.Object, _notificationServiceMock.Object);
        var approveCommand = new ApprovePurchaseRequestCommand(purchaseRequest.Id, "Bütçe onaylandı, sipariş verilebilir.");

        var approveResult = await approveHandler.Handle(approveCommand, CancellationToken.None);

        // Assert approval
        approveResult.IsSuccess.Should().BeTrue();
        approveResult.Data!.Status.Should().Be(RequestStatus.Approved);

        // 3. Act - Warehouse receives the goods (Convert to inventory)
        var convertHandler = new ConvertPurchaseRequestToInventoryCommandHandler(context, managerUserMock.Object);
        var convertCommand = new ConvertPurchaseRequestToInventoryCommand(purchaseRequest.Id, "Mal kabul yapıldı, depoya teslim alındı.");

        var convertResult = await convertHandler.Handle(convertCommand, CancellationToken.None);

        // 4. Assert - Status becomes Completed and stock is increased by 25
        convertResult.IsSuccess.Should().BeTrue();
        convertResult.Data!.Status.Should().Be(RequestStatus.Completed);

        var finalProduct = await context.Products.FindAsync(product.Id);
        finalProduct!.CurrentStock.Should().Be(35); // 10 + 25

        var stockInTransaction = await context.InventoryTransactions
            .FirstOrDefaultAsync(t => t.ProductId == product.Id && t.Description.Contains("PR-20260827-001"));

        stockInTransaction.Should().NotBeNull();
        stockInTransaction!.Quantity.Should().Be(25);
    }

    [Fact]
    public void Scenario4_DocumentExport_PdfAndExcelGeneration_WorkflowTest()
    {
        // 1. PDF Export Test (QuestPDF)
        var pdfService = new QuestPdfReportService();
        var pdfDto = new PurchaseRequestPdfDto
        {
            Id = Guid.NewGuid(),
            RequestNumber = "PR-20260827-099",
            Department = "Kurumsal Satış & Kırtasiye",
            Priority = "Yüksek",
            Status = "Onaylandı",
            RequesterName = "Zehra Tunçer",
            CreatedDate = DateTime.UtcNow,
            Description = "Yeni eğitim dönemi için kırtasiye malzemeleri temini.",
            TotalEstimatedAmount = 15400m,
            Items = new List<PurchaseRequestPdfItemDto>
            {
                new()
                {
                    ItemIndex = 1,
                    ProductCode = "KRT-001",
                    ProductName = "Fotokopi Kağıdı A4 80gr 500lü Koli",
                    Quantity = 10,
                    Unit = "Koli",
                    EstimatedUnitPrice = 780m,
                    EstimatedTotalPrice = 7800m,
                    Note = "Çift taraflı baskıya uygun"
                },
                new()
                {
                    ItemIndex = 2,
                    ProductCode = "KRT-002",
                    ProductName = "Fosforlu Kalem Seti 4 Renk",
                    Quantity = 100,
                    Unit = "Set",
                    EstimatedUnitPrice = 76m,
                    EstimatedTotalPrice = 7600m,
                    Note = "Sarı, Yeşil, Pembe, Turuncu"
                }
            },
            Approvals = new List<PurchaseRequestPdfApprovalDto>
            {
                new()
                {
                    StepNumber = 1,
                    StepName = "Departman Müdürü Onayı",
                    ApproverName = "Yönetici Admin",
                    Action = "Onaylandı",
                    ActionDate = DateTime.UtcNow,
                    Comment = "Bütçe uygundur."
                }
            }
        };

        var pdfBytes = pdfService.GeneratePurchaseRequestPdf(pdfDto);

        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);
        // PDF header check: %PDF (0x25, 0x50, 0x44, 0x46)
        pdfBytes[0].Should().Be(0x25);
        pdfBytes[1].Should().Be(0x50);
        pdfBytes[2].Should().Be(0x44);
        pdfBytes[3].Should().Be(0x46);

        // 2. Excel Export Test (ClosedXML)
        var excelService = new ClosedXmlExcelExportService();
        var products = new List<ProductExportDto>
        {
            new()
            {
                Code = "KRT-001",
                Name = "A4 Kağıt",
                Description = "80gr Koli",
                Unit = "Koli",
                CurrentStock = 50,
                MinStockLevel = 10,
                UnitPrice = 780m,
                TotalStockValue = 39000m,
                SupplierName = "Hedef Kırtasiye",
                Status = "Aktif"
            },
            new()
            {
                Code = "KRT-002",
                Name = "Tükenmez Kalem Mavi",
                Description = "0.7mm",
                Unit = "Paket",
                CurrentStock = 120,
                MinStockLevel = 20,
                UnitPrice = 45m,
                TotalStockValue = 5400m,
                SupplierName = "Adel Kalemcilik",
                Status = "Aktif"
            }
        };

        var excelBytes = excelService.ExportProductsToExcel(products);

        excelBytes.Should().NotBeNull();
        excelBytes.Length.Should().BeGreaterThan(500);
        // Zip / XLSX header check: PK (0x50, 0x4B)
        excelBytes[0].Should().Be(0x50);
        excelBytes[1].Should().Be(0x4B);
    }
}
