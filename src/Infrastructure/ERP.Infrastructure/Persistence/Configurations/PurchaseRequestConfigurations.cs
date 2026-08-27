using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(pr => pr.RequestNumber)
            .IsUnique();

        builder.Property(pr => pr.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pr => pr.TotalEstimatedAmount)
            .HasPrecision(18, 2);

        builder.Property(pr => pr.Note)
            .HasMaxLength(1000);

        builder.Property(pr => pr.CurrentApprovalStep)
            .HasDefaultValue(1);

        builder.HasOne(pr => pr.RequesterUser)
            .WithMany()
            .HasForeignKey(pr => pr.RequesterUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(pr => pr.Items)
            .WithOne(pri => pri.PurchaseRequest)
            .HasForeignKey(pri => pri.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pr => pr.ApprovalHistories)
            .WithOne(ah => ah.PurchaseRequest)
            .HasForeignKey(ah => ah.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Purchase Requests
        builder.HasData(
            new PurchaseRequest
            {
                Id = Guid.Parse("88888888-1111-1111-1111-111111111111"),
                RequestNumber = "TALEP-20260827-001",
                Department = "Merkez Mağaza Satış & Depo",
                Status = ERP.Domain.Enums.RequestStatus.PendingApproval,
                TotalEstimatedAmount = 14500.00m,
                Note = "Kritik stok seviyesine düşen A4 fotokopi kağıdı ve kurşun kalemler için acil tedarik talebi.",
                CurrentApprovalStep = 2, // 10.000 TL üzeri Genel Müdür onayı
                RequesterUserId = UserConfiguration.CashierUserId,
                CreatedDate = new DateTime(2026, 8, 27, 8, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new PurchaseRequest
            {
                Id = Guid.Parse("88888888-2222-2222-2222-222222222222"),
                RequestNumber = "TALEP-20260826-002",
                Department = "Okul & Kurumsal Satış",
                Status = ERP.Domain.Enums.RequestStatus.Approved,
                TotalEstimatedAmount = 5800.00m,
                Note = "Gıpta spiralli defter stok takviyesi (Onaylandı - Mal kabul yapılabilir).",
                CurrentApprovalStep = 1,
                RequesterUserId = UserConfiguration.CashierUserId,
                CreatedDate = new DateTime(2026, 8, 26, 14, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new PurchaseRequest
            {
                Id = Guid.Parse("88888888-3333-3333-3333-333333333333"),
                RequestNumber = "TALEP-20260825-003",
                Department = "Yönetim & İdari İşler",
                Status = ERP.Domain.Enums.RequestStatus.Rejected,
                TotalEstimatedAmount = 25500.00m,
                Note = "Lüks dolmakalem ve özel masaüstü deri setleri talebi.",
                CurrentApprovalStep = 2,
                RequesterUserId = UserConfiguration.ManagerUserId,
                CreatedDate = new DateTime(2026, 8, 25, 11, 20, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new PurchaseRequest
            {
                Id = Guid.Parse("88888888-4444-4444-4444-444444444444"),
                RequestNumber = "TALEP-20260824-004",
                Department = "Merkez Mağaza Satış & Depo",
                Status = ERP.Domain.Enums.RequestStatus.Completed,
                TotalEstimatedAmount = 8750.00m,
                Note = "Pritt yapıştırıcı ve zımba makinesi dönem başı siparişi (Depoya teslim alındı).",
                CurrentApprovalStep = 1,
                RequesterUserId = UserConfiguration.CashierUserId,
                CreatedDate = new DateTime(2026, 8, 24, 9, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}

public class PurchaseRequestItemConfiguration : IEntityTypeConfiguration<PurchaseRequestItem>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestItem> builder)
    {
        builder.ToTable("PurchaseRequestItems");

        builder.HasKey(pri => pri.Id);

        builder.Property(pri => pri.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(pri => pri.EstimatedUnitPrice)
            .HasPrecision(18, 2);

        builder.Property(pri => pri.EstimatedTotalPrice)
            .HasPrecision(18, 2);

        builder.Property(pri => pri.Notes)
            .HasMaxLength(500);

        builder.HasOne(pri => pri.PurchaseRequest)
            .WithMany(pr => pr.Items)
            .HasForeignKey(pri => pri.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pri => pri.Product)
            .WithMany()
            .HasForeignKey(pri => pri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Purchase Request Items
        builder.HasData(
            // Items for TALEP-001 (Pending)
            new PurchaseRequestItem
            {
                Id = Guid.Parse("66666666-1111-1111-1111-111111111111"),
                PurchaseRequestId = Guid.Parse("88888888-1111-1111-1111-111111111111"),
                ProductId = ProductConfiguration.SampleProduct1Id,
                RequestedQuantity = 15,
                Unit = "Koli",
                EstimatedUnitPrice = 780.00m,
                EstimatedTotalPrice = 11700.00m,
                Notes = "Fotokopi kağıdı tükenmek üzere, acil sevk gerekli",
                CreatedDate = new DateTime(2026, 8, 27, 8, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new PurchaseRequestItem
            {
                Id = Guid.Parse("66666666-2222-2222-2222-222222222222"),
                PurchaseRequestId = Guid.Parse("88888888-1111-1111-1111-111111111111"),
                ProductId = ProductConfiguration.SampleProduct2Id,
                RequestedQuantity = 10,
                Unit = "Kutu",
                EstimatedUnitPrice = 280.00m, // İskontolu birim fiyat
                EstimatedTotalPrice = 2800.00m,
                Notes = "Sınav haftası için 2B kurşun kalem desteği",
                CreatedDate = new DateTime(2026, 8, 27, 8, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            // Item for TALEP-002 (Approved)
            new PurchaseRequestItem
            {
                Id = Guid.Parse("66666666-3333-3333-3333-333333333333"),
                PurchaseRequestId = Guid.Parse("88888888-2222-2222-2222-222222222222"),
                ProductId = ProductConfiguration.SampleProduct3Id,
                RequestedQuantity = 20,
                Unit = "Paket",
                EstimatedUnitPrice = 290.00m,
                EstimatedTotalPrice = 5800.00m,
                Notes = "Okul açılış sezonu defter takviyesi",
                CreatedDate = new DateTime(2026, 8, 26, 14, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            // Item for TALEP-004 (Completed)
            new PurchaseRequestItem
            {
                Id = Guid.Parse("66666666-4444-4444-4444-444444444444"),
                PurchaseRequestId = Guid.Parse("88888888-4444-4444-4444-444444444444"),
                ProductId = ProductConfiguration.SampleProduct5Id,
                RequestedQuantity = 50,
                Unit = "Adet",
                EstimatedUnitPrice = 175.00m,
                EstimatedTotalPrice = 8750.00m,
                Notes = "Kurumsal büro zımba teslimatı yapıldı",
                CreatedDate = new DateTime(2026, 8, 24, 9, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
