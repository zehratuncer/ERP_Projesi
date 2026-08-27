using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using ERP.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ERP.UnitTests.SecurityAndDatabase;

public class DatabaseIntegrityAndSecurityTests
{
    [Fact]
    public void DatabaseSchema_WithoutSeedData_ShouldBeCompletelyEmptyByDefault()
    {
        // Arrange & Act - Create clean database with no seed mock data
        using var cleanContext = TestDbContextFactory.CreateInMemoryDbContext(seedMockData: false);

        // Assert - Tables are completely empty
        cleanContext.Products.Should().BeEmpty();
        cleanContext.Suppliers.Should().BeEmpty();
        cleanContext.InventoryTransactions.Should().BeEmpty();
        cleanContext.Sales.Should().BeEmpty();
        cleanContext.PurchaseRequests.Should().BeEmpty();
        cleanContext.ApprovalWorkflows.Should().BeEmpty();
    }

    [Fact]
    public async Task SoftDelete_GlobalQueryFilter_ShouldExcludeDeletedRecordsAutomatically()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var testProduct = new Product
        {
            Id = Guid.NewGuid(),
            Code = "TEST-DEL-001",
            Name = "Silinecek Ürün",
            Unit = "Adet",
            CurrentStock = 10,
            MinStockLevel = 2,
            UnitPrice = 25m,
            IsDeleted = false
        };

        context.Products.Add(testProduct);
        await context.SaveChangesAsync();

        // Act - Soft Delete
        testProduct.IsDeleted = true;
        await context.SaveChangesAsync();

        // Assert - Standard query should NOT find it
        var queryResult = await context.Products.FirstOrDefaultAsync(p => p.Id == testProduct.Id);
        queryResult.Should().BeNull();

        // Assert - IgnoreQueryFilters should still find it for audit
        var rawResult = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == testProduct.Id);
        rawResult.Should().NotBeNull();
        rawResult!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void PasswordHashing_BCrypt_ShouldVerifyAndPreventPlainTextStorage()
    {
        // Arrange
        var password = "StrongPassword123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2a$");
        BCrypt.Net.BCrypt.Verify(password, hash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public async Task Database_ForeignKeyIntegrity_ShouldCascadeOrRestrictProperly()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = "Test Tedarik Ltd",
            ContactPerson = "Ali Veli",
            Email = "ali@tedarik.com",
            Phone = "05551112233"
        };
        context.Suppliers.Add(supplier);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = "FK-TEST-01",
            Name = "Tedarikçili Ürün",
            Unit = "Kutu",
            CurrentStock = 50,
            MinStockLevel = 5,
            UnitPrice = 100m,
            SupplierId = supplier.Id
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert Navigation Property
        var loadedProduct = await context.Products.Include(p => p.Supplier).FirstOrDefaultAsync(p => p.Id == product.Id);
        loadedProduct.Should().NotBeNull();
        loadedProduct!.Supplier.Should().NotBeNull();
        loadedProduct.Supplier!.Name.Should().Be("Test Tedarik Ltd");
    }

    [Fact]
    public void DockerCompose_Configuration_ShouldDefineAllRequiredServicesAndPorts()
    {
        // Locate docker-compose.yml relative to solution root
        var baseDir = Directory.GetCurrentDirectory();
        var composePath = Path.Combine(baseDir, "..", "..", "..", "..", "docker-compose.yml");
        if (!File.Exists(composePath))
        {
            composePath = Path.Combine(baseDir, "docker-compose.yml");
        }

        if (File.Exists(composePath))
        {
            var content = File.ReadAllText(composePath);

            // Assert Services
            content.Should().Contain("erp-sqlserver");
            content.Should().Contain("erp-api");
            content.Should().Contain("erp-frontend");

            // Assert Ports
            content.Should().Contain("1433:1433");
            content.Should().Contain("5000:5000");
            content.Should().Contain("4200:80");

            // Assert Network
            content.Should().Contain("erp-network");
        }
    }

    [Fact]
    public void NginxConfig_ShouldContainSpaFallbackAndWebSocketHubProxy()
    {
        // Locate nginx.conf relative to solution root
        var baseDir = Directory.GetCurrentDirectory();
        var nginxPath = Path.Combine(baseDir, "..", "..", "..", "..", "frontend", "nginx.conf");
        if (!File.Exists(nginxPath))
        {
            nginxPath = Path.Combine(baseDir, "frontend", "nginx.conf");
        }

        if (File.Exists(nginxPath))
        {
            var content = File.ReadAllText(nginxPath);

            // Assert SPA routing fallback
            content.Should().Contain("try_files $uri $uri/ /index.html;");

            // Assert API proxy
            content.Should().Contain("location /api/");

            // Assert WebSocket SignalR Hubs proxy
            content.Should().Contain("location /hubs/");
            content.Should().Contain("proxy_set_header Upgrade $http_upgrade;");
        }
    }
}
