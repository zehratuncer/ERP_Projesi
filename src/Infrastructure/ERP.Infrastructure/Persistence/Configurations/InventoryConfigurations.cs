using ERP.Domain.Entities;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public static readonly Guid SampleProduct1Id = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111");
    public static readonly Guid SampleProduct2Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
    public static readonly Guid SampleProduct3Id = Guid.Parse("bbbbbbbb-3333-3333-3333-333333333333");
    public static readonly Guid SampleProduct4Id = Guid.Parse("bbbbbbbb-4444-4444-4444-444444444444");
    public static readonly Guid SampleProduct5Id = Guid.Parse("bbbbbbbb-5555-5555-5555-555555555555");

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        // Seed Stationery Products
        builder.HasData(
            new Product
            {
                Id = SampleProduct1Id,
                Code = "KRT-001",
                Name = "Copier Bond A4 80gr Fotokopi Kağıdı (5'li Koli)",
                Description = "Yüksek beyazlıkta 80gr 2500 yaprak lazer/inkjet fotokopi kağıdı",
                Unit = "Koli",
                CurrentStock = 8, // Kritik stokta (MinStockLevel: 25)
                MinStockLevel = 25,
                UnitPrice = 780.00m,
                SupplierId = SupplierConfiguration.SampleSupplier2Id,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct2Id,
                Code = "KRT-042",
                Name = "Faber-Castell 2B Sınav Kurşun Kalem (72'li Kutu)",
                Description = "Özel SV yapıştırma kırılmaya dirençli sınav ve çizim kalemi",
                Unit = "Kutu",
                CurrentStock = 5, // Kritik stokta (MinStockLevel: 20)
                MinStockLevel = 20,
                UnitPrice = 360.00m,
                SupplierId = SupplierConfiguration.SampleSupplier1Id,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct3Id,
                Code = "KRT-089",
                Name = "Gıpta Spiralli A4 Çizgili Defter 96 Yaprak (10'lu Paket)",
                Description = "Sert kapak, mikroperforeli kaliteli 1. hamur kağıt okul ve ofis defteri",
                Unit = "Paket",
                CurrentStock = 12, // Kritik stokta (MinStockLevel: 30)
                MinStockLevel = 30,
                UnitPrice = 290.00m,
                SupplierId = SupplierConfiguration.SampleSupplier3Id,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct4Id,
                Code = "KRT-114",
                Name = "Pritt Stick Kuru Yapıştırıcı 43gr (24'lü Stand)",
                Description = "Solventsiz, yıkanabilir ve kokusuz güçlü kırtasiye yapıştırıcı standı",
                Unit = "Stand",
                CurrentStock = 18,
                MinStockLevel = 10,
                UnitPrice = 950.00m,
                SupplierId = SupplierConfiguration.SampleSupplier1Id,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct5Id,
                Code = "KRT-205",
                Name = "Maped Ağır Büro Zımba Makinesi No:24/6",
                Description = "Maksimum 25 sayfa kapasiteli metal iç mekanizmalı masaüstü zımba",
                Unit = "Adet",
                CurrentStock = 35,
                MinStockLevel = 10,
                UnitPrice = 175.00m,
                SupplierId = SupplierConfiguration.SampleSupplier1Id,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasMaxLength(250);

        builder.HasOne(t => t.Product)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed initial stationery transactions
        builder.HasData(
            new InventoryTransaction
            {
                Id = Guid.Parse("cccccccc-1111-1111-1111-111111111111"),
                ProductId = ProductConfiguration.SampleProduct1Id,
                Quantity = 100,
                TransactionType = TransactionType.In,
                Description = "Okul açılış sezonu toptan A4 kağıt girişi",
                TransactionDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                UserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new InventoryTransaction
            {
                Id = Guid.Parse("cccccccc-2222-2222-2222-222222222222"),
                ProductId = ProductConfiguration.SampleProduct1Id,
                Quantity = 50,
                TransactionType = TransactionType.Out,
                Description = "Atatürk Anadolu Lisesi kurumsal dönem başı sipariş sevkiyatı",
                TransactionDate = new DateTime(2026, 1, 3, 14, 30, 0, DateTimeKind.Utc),
                UserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 1, 3, 14, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new InventoryTransaction
            {
                Id = Guid.Parse("cccccccc-3333-3333-3333-333333333333"),
                ProductId = ProductConfiguration.SampleProduct2Id,
                Quantity = 15,
                TransactionType = TransactionType.Out,
                Description = "Kurumsal ofis sınav & test kalemi teslimatı",
                TransactionDate = new DateTime(2026, 1, 4, 11, 15, 0, DateTimeKind.Utc),
                UserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 1, 4, 11, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new InventoryTransaction
            {
                Id = Guid.Parse("cccccccc-4444-4444-4444-444444444444"),
                ProductId = ProductConfiguration.SampleProduct3Id,
                Quantity = 2,
                TransactionType = TransactionType.Adjustment,
                Description = "Depoda ambalajı hasar gören defter paketi düzeltmesi",
                TransactionDate = new DateTime(2026, 1, 5, 16, 0, 0, DateTimeKind.Utc),
                UserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 1, 5, 16, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
