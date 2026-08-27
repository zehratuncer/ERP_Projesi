using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.ReceiptNumber)
            .IsUnique();

        builder.Property(s => s.CustomerName)
            .HasMaxLength(150);

        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.FinalAmount)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.CashierUser)
            .WithMany()
            .HasForeignKey(s => s.CashierUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Items)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Sales
        builder.HasData(
            new Sale
            {
                Id = Guid.Parse("77777777-1111-1111-1111-111111111111"),
                ReceiptNumber = "FIS-20260827-001",
                CustomerName = "Mehmet Demir (Perakende Müşteri)",
                TotalAmount = 940.00m,
                DiscountAmount = 0.00m,
                FinalAmount = 940.00m,
                PaymentMethod = ERP.Domain.Enums.PaymentMethod.Cash,
                SaleDate = new DateTime(2026, 8, 27, 9, 15, 0, DateTimeKind.Utc),
                CashierUserId = UserConfiguration.CashierUserId,
                CreatedDate = new DateTime(2026, 8, 27, 9, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Sale
            {
                Id = Guid.Parse("77777777-2222-2222-2222-222222222222"),
                ReceiptNumber = "FIS-20260827-002",
                CustomerName = "Ayşe Yılmaz (Öğrenci Velisi)",
                TotalAmount = 1060.00m,
                DiscountAmount = 0.00m,
                FinalAmount = 1060.00m,
                PaymentMethod = ERP.Domain.Enums.PaymentMethod.CreditCard,
                SaleDate = new DateTime(2026, 8, 27, 10, 30, 0, DateTimeKind.Utc),
                CashierUserId = UserConfiguration.CashierUserId,
                CreatedDate = new DateTime(2026, 8, 27, 10, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Sale
            {
                Id = Guid.Parse("77777777-3333-3333-3333-333333333333"),
                ReceiptNumber = "FIS-20260826-003",
                CustomerName = "Özel Bilim Koleji (Kurumsal)",
                TotalAmount = 4250.00m,
                DiscountAmount = 0.00m,
                FinalAmount = 4250.00m,
                PaymentMethod = ERP.Domain.Enums.PaymentMethod.Cash,
                SaleDate = new DateTime(2026, 8, 26, 16, 45, 0, DateTimeKind.Utc),
                CashierUserId = UserConfiguration.AdminUserId,
                CreatedDate = new DateTime(2026, 8, 26, 16, 45, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(si => si.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(si => si.DiscountRate)
            .HasPrecision(18, 2);

        builder.HasOne(si => si.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Sale Items
        builder.HasData(
            // FIS-001 items
            new SaleItem
            {
                Id = Guid.Parse("33333333-1111-1111-1111-111111111111"),
                SaleId = Guid.Parse("77777777-1111-1111-1111-111111111111"),
                ProductId = ProductConfiguration.SampleProduct2Id,
                Quantity = 1,
                UnitPrice = 360.00m,
                DiscountRate = 0.00m,
                TotalPrice = 360.00m,
                CreatedDate = new DateTime(2026, 8, 27, 9, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new SaleItem
            {
                Id = Guid.Parse("33333333-2222-2222-2222-222222222222"),
                SaleId = Guid.Parse("77777777-1111-1111-1111-111111111111"),
                ProductId = ProductConfiguration.SampleProduct3Id,
                Quantity = 2,
                UnitPrice = 290.00m,
                DiscountRate = 0.00m,
                TotalPrice = 580.00m,
                CreatedDate = new DateTime(2026, 8, 27, 9, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            // FIS-002 items
            new SaleItem
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                SaleId = Guid.Parse("77777777-2222-2222-2222-222222222222"),
                ProductId = ProductConfiguration.SampleProduct7Id,
                Quantity = 1,
                UnitPrice = 850.00m,
                DiscountRate = 0.00m,
                TotalPrice = 850.00m,
                CreatedDate = new DateTime(2026, 8, 27, 10, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new SaleItem
            {
                Id = Guid.Parse("33333333-4444-4444-4444-444444444444"),
                SaleId = Guid.Parse("77777777-2222-2222-2222-222222222222"),
                ProductId = ProductConfiguration.SampleProduct6Id,
                Quantity = 1,
                UnitPrice = 210.00m,
                DiscountRate = 0.00m,
                TotalPrice = 210.00m,
                CreatedDate = new DateTime(2026, 8, 27, 10, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            // FIS-003 items
            new SaleItem
            {
                Id = Guid.Parse("33333333-5555-5555-5555-555555555555"),
                SaleId = Guid.Parse("77777777-3333-3333-3333-333333333333"),
                ProductId = ProductConfiguration.SampleProduct1Id,
                Quantity = 5,
                UnitPrice = 780.00m,
                DiscountRate = 0.00m,
                TotalPrice = 3900.00m,
                CreatedDate = new DateTime(2026, 8, 26, 16, 45, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new SaleItem
            {
                Id = Guid.Parse("33333333-6666-6666-6666-666666666666"),
                SaleId = Guid.Parse("77777777-3333-3333-3333-333333333333"),
                ProductId = ProductConfiguration.SampleProduct5Id,
                Quantity = 2,
                UnitPrice = 175.00m,
                DiscountRate = 0.00m,
                TotalPrice = 350.00m,
                CreatedDate = new DateTime(2026, 8, 26, 16, 45, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
