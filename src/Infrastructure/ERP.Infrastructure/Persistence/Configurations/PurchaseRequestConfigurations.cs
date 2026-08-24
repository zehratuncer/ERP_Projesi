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
    }
}
