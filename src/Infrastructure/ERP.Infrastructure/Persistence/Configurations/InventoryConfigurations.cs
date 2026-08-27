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
    }
}

