using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
