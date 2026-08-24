using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ApprovalWorkflowConfiguration : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public static readonly Guid DefaultWorkflowId = Guid.Parse("99999999-9999-9999-9999-999999999991");

    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("ApprovalWorkflows");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Description)
            .HasMaxLength(500);

        builder.Property(w => w.MinAmount)
            .HasPrecision(18, 2);

        builder.Property(w => w.MaxAmount)
            .HasPrecision(18, 2);

        builder.HasMany(w => w.Steps)
            .WithOne(s => s.ApprovalWorkflow)
            .HasForeignKey(s => s.ApprovalWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Default Workflow
        builder.HasData(
            new ApprovalWorkflow
            {
                Id = DefaultWorkflowId,
                Name = "Standart Kırtasiye Onay Akışı",
                Description = "Limit bazlı kademeli kırtasiye satın alma onay iş akışı (10.000 TL altı Şube Müdürü, üzeri Genel Müdür/Direktör).",
                MinAmount = 0m,
                MaxAmount = null,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}

public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public static readonly Guid Step1Id = Guid.Parse("99999999-9999-9999-9999-999999999992");
    public static readonly Guid Step2Id = Guid.Parse("99999999-9999-9999-9999-999999999993");

    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StepName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.MinAmount)
            .HasPrecision(18, 2);

        builder.Property(s => s.MaxAmount)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.Role)
            .WithMany()
            .HasForeignKey(s => s.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed Steps
        builder.HasData(
            new ApprovalStep
            {
                Id = Step1Id,
                ApprovalWorkflowId = ApprovalWorkflowConfiguration.DefaultWorkflowId,
                StepNumber = 1,
                StepName = "Birim / Şube Müdürü Onayı",
                RoleId = RoleConfiguration.ManagerRoleId,
                IsRequired = true,
                MinAmount = 0m,
                MaxAmount = 10000m,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new ApprovalStep
            {
                Id = Step2Id,
                ApprovalWorkflowId = ApprovalWorkflowConfiguration.DefaultWorkflowId,
                StepNumber = 2,
                StepName = "Genel Satın Alma Direktörü Onayı",
                RoleId = RoleConfiguration.AdminRoleId,
                IsRequired = true,
                MinAmount = 10000.01m,
                MaxAmount = null,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }
}

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.ToTable("ApprovalHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.StepName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.Comment)
            .HasMaxLength(1000);

        builder.HasOne(h => h.PurchaseRequest)
            .WithMany(pr => pr.ApprovalHistories)
            .HasForeignKey(h => h.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ApproverUser)
            .WithMany()
            .HasForeignKey(h => h.ApproverUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
