using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public static readonly Guid SampleSupplier1Id = Guid.Parse("dddddddd-1111-1111-1111-111111111111");
    public static readonly Guid SampleSupplier2Id = Guid.Parse("dddddddd-2222-2222-2222-222222222222");
    public static readonly Guid SampleSupplier3Id = Guid.Parse("dddddddd-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactPerson)
            .HasMaxLength(150);

        builder.Property(s => s.Email)
            .HasMaxLength(150);

        builder.Property(s => s.Phone)
            .HasMaxLength(50);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.TaxNumber)
            .HasMaxLength(50);

        builder.HasMany(s => s.Products)
            .WithOne(p => p.Supplier)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed Stationery Suppliers
        builder.HasData(
            new Supplier
            {
                Id = SampleSupplier1Id,
                Name = "Adel Kalemcilik & Kırtasiye A.Ş.",
                ContactPerson = "Mehmet Yılmaz",
                Email = "siparis@adel.com.tr",
                Phone = "+90 (216) 555 20 20",
                Address = "Saray Mah. Site Yolu Cad. No:5, Ümraniye/İstanbul",
                TaxNumber = "0080012345",
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Supplier
            {
                Id = SampleSupplier2Id,
                Name = "Kopier A4 Kağıt & Ambalaj Sanayi Ltd.",
                ContactPerson = "Ayşe Demir",
                Email = "satis@kopierkagit.com",
                Phone = "+90 (212) 641 10 30",
                Address = "İkitelli OSB, Kağıtçılar Sanayi Sitesi 3. Cadde No:14, Başakşehir/İstanbul",
                TaxNumber = "5840987654",
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Supplier
            {
                Id = SampleSupplier3Id,
                Name = "Gıpta Ofis & Okul Kırtasiye Ürünleri A.Ş.",
                ContactPerson = "Kemal Kaya",
                Email = "info@gipta.com.tr",
                Phone = "+90 (312) 888 40 50",
                Address = "1. Organize Sanayi Bölgesi Dağıstan Cad. No:7, Sincan/Ankara",
                TaxNumber = "4110456789",
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
