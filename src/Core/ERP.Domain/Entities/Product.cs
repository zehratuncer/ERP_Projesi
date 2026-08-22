using ERP.Domain.Common;

namespace ERP.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = "Adet"; // Adet, Paket, Kg, Litre vb.
    public int CurrentStock { get; set; } = 0;
    public int MinStockLevel { get; set; } = 10;
    public decimal UnitPrice { get; set; } = 0.0m;
    public bool IsActive { get; set; } = true;

    // Tedarikçi ilişkisi (opsiyonel veya Faz 4'te Supplier eklendiğinde bağlanacak)
    public Guid? SupplierId { get; set; }

    // Navigation property
    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
