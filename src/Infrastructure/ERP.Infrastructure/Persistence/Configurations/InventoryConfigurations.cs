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

        // Seed Sample Products for MVP
        builder.HasData(
            new Product
            {
                Id = SampleProduct1Id,
                Code = "PRD-001",
                Name = "M4 Çelik Civata (100 lük Paket)",
                Description = "Yüksek dayanımlı paslanmaz çelik bağlantı elemanı",
                Unit = "Paket",
                CurrentStock = 8, // Kritik stokta (MinStockLevel: 25)
                MinStockLevel = 25,
                UnitPrice = 120.50m,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct2Id,
                Code = "PRD-042",
                Name = "Hidrolik Yağ (20L Varil)",
                Description = "Endüstriyel pres ve tezgahlar için ISO VG 46 yağ",
                Unit = "Varil",
                CurrentStock = 2, // Kritik stokta (MinStockLevel: 5)
                MinStockLevel = 5,
                UnitPrice = 2450.00m,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct3Id,
                Code = "PRD-089",
                Name = "Endüstriyel Rulman 6204-2RS",
                Description = "Çift tarafı kauçuk kapaklı bilyalı sabit rulman",
                Unit = "Adet",
                CurrentStock = 12, // Kritik stokta (MinStockLevel: 30)
                MinStockLevel = 30,
                UnitPrice = 85.00m,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Product
            {
                Id = SampleProduct4Id,
                Code = "PRD-105",
                Name = "Elektrik Motoru 1.5kW 1400d/d",
                Description = "Üç fazlı sincap kafesli asenkron motor",
                Unit = "Adet",
                CurrentStock = 45, // Normal stok
                MinStockLevel = 10,
                UnitPrice = 4200.00m,
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

        // Seed initial transactions
        builder.HasData(
            new InventoryTransaction
            {
                Id = Guid.Parse("cccccccc-1111-1111-1111-111111111111"),
                ProductId = ProductConfiguration.SampleProduct1Id,
                Quantity = 50,
                TransactionType = TransactionType.In,
                Description = "Açılış stok girişi",
                TransactionDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                UserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new InventoryTransaction
            {
                Id = Guid.Parse("cccccccc-2222-2222-2222-222222222222"),
                ProductId = ProductConfiguration.SampleProduct1Id,
                Quantity = 42,
                TransactionType = TransactionType.Out,
                Description = "Montaj hattına sevk",
                TransactionDate = new DateTime(2026, 1, 3, 14, 30, 0, DateTimeKind.Utc),
                UserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 1, 3, 14, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
