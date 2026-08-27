using ERP.Domain.Constants;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ManagerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid EmployeeRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .HasMaxLength(250);

        // System Roles Seed
        builder.HasData(
            new Role
            {
                Id = AdminRoleId,
                Name = Roles.Admin,
                Description = "Tam yetkili sistem yöneticisi",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Role
            {
                Id = ManagerRoleId,
                Name = Roles.Manager,
                Description = "Departman ve süreç yöneticisi",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Role
            {
                Id = EmployeeRoleId,
                Name = Roles.Employee,
                Description = "Standart sistem personeli",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // System Admin User Seed (Password: Admin123!)
        builder.HasData(
            new User
            {
                Id = AdminUserId,
                Email = "admin@erp.com",
                FullName = "Zehra Tuncer (Sistem Yöneticisi)",
                PasswordHash = "$2a$11$q9o94O6k3Jb9vG6M2dYVn.6F1Z5x6i0q3pQ8nF5g8y8J6m5g8rK2W",
                RoleId = RoleConfiguration.AdminRoleId,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}
