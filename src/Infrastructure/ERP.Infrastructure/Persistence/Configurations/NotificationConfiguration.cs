using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.RoleName)
            .HasMaxLength(50);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(300);

        builder.Property(n => n.Type)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedDate);

        // Seed Notifications
        builder.HasData(
            new Notification
            {
                Id = Guid.Parse("44444444-1111-1111-1111-111111111111"),
                UserId = UserConfiguration.AdminUserId,
                RoleName = ERP.Domain.Constants.Roles.Admin,
                Title = "Kritik Stok Uyarısı",
                Message = "[KRT-001] Copier Bond A4 Kağıt stok miktarı (8 Koli) kritik eşik seviyesinin (25 Koli) altına düştü!",
                Type = ERP.Domain.Enums.NotificationType.StockAlert,
                ActionUrl = "/inventory",
                IsRead = false,
                CreatedDate = new DateTime(2026, 8, 27, 8, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Notification
            {
                Id = Guid.Parse("44444444-2222-2222-2222-222222222222"),
                UserId = UserConfiguration.AdminUserId,
                RoleName = ERP.Domain.Constants.Roles.Admin,
                Title = "Onayınızı Bekleyen Satın Alma Talebi",
                Message = "TALEP-20260827-001 numaralı ve ₺14.500,00 tutarındaki satın alma talebi Genel Satın Alma Direktörü onayınızı beklemektedir.",
                Type = ERP.Domain.Enums.NotificationType.ApprovalNeeded,
                ActionUrl = "/purchasing",
                IsRead = false,
                CreatedDate = new DateTime(2026, 8, 27, 8, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Notification
            {
                Id = Guid.Parse("44444444-3333-3333-3333-333333333333"),
                UserId = UserConfiguration.ManagerUserId,
                RoleName = ERP.Domain.Constants.Roles.Manager,
                Title = "Kritik Stok Uyarısı",
                Message = "[KRT-042] Faber-Castell 2B Sınav Kurşun Kalem stok miktarı (5 Kutu) kritik eşik seviyesinin (20 Kutu) altına düştü!",
                Type = ERP.Domain.Enums.NotificationType.StockAlert,
                ActionUrl = "/inventory",
                IsRead = false,
                CreatedDate = new DateTime(2026, 8, 27, 8, 15, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Notification
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                UserId = UserConfiguration.CashierUserId,
                RoleName = ERP.Domain.Constants.Roles.Employee,
                Title = "Talep Onaylandı",
                Message = "TALEP-20260826-002 numaralı satın alma talebiniz Şube Müdürü Ahmet Yılmaz tarafından onaylanmıştır.",
                Type = ERP.Domain.Enums.NotificationType.Info,
                ActionUrl = "/purchasing",
                IsRead = true,
                CreatedDate = new DateTime(2026, 8, 26, 15, 30, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
