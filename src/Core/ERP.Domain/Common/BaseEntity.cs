namespace ERP.Domain.Common;

/// <summary>
/// Tüm veri tabanı varlıklarının (Entities) miras alacağı temel sınıf.
/// Ortak alanlar ve soft-delete mekanizması burada tanımlanır.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; } = false;
}
