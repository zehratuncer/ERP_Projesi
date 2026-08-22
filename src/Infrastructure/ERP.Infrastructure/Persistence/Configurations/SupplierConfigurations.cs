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

        // Seed Sample Suppliers for MVP
        builder.HasData(
            new Supplier
            {
                Id = SampleSupplier1Id,
                Name = "Alfa Civata & Bağlantı Elemanları San. Tic. Ltd. Şti.",
                ContactPerson = "Mehmet Yılmaz",
                Email = "siparis@alfacivata.com",
                Phone = "+90 (212) 555 10 20",
                Address = "İkitelli OSB, Metal İş Sanayi Sitesi 12. Blok No:45, Başakşehir/İstanbul",
                TaxNumber = "1234567890",
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Supplier
            {
                Id = SampleSupplier2Id,
                Name = "PetroKimya Endüstriyel Yağlar A.Ş.",
                ContactPerson = "Ayşe Demir",
                Email = "info@petrokimya.com.tr",
                Phone = "+90 (262) 641 33 44",
                Address = "Gebze Organize Sanayi Bölgesi 1000. Sokak No:12, Gebze/Kocaeli",
                TaxNumber = "9876543210",
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Supplier
            {
                Id = SampleSupplier3Id,
                Name = "RulmanTek Makine ve Güç Aktarım Ltd.",
                ContactPerson = "Kemal Kaya",
                Email = "satis@rulmantek.com",
                Phone = "+90 (216) 444 88 99",
                Address = "Dudullu OSB DES Sanayi Sitesi 105. Sokak No:8, Ümraniye/İstanbul",
                TaxNumber = "4567891230",
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
