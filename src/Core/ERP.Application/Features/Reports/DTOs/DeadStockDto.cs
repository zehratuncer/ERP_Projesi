namespace ERP.Application.Features.Reports.DTOs;

public class DeadStockItemDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public string Unit { get; set; } = "Adet";
    public decimal UnitPrice { get; set; }
    public decimal TotalTiedUpValue { get; set; } // Bağlanan sermaye (CurrentStock * UnitPrice)
    public DateTime? LastMovementDate { get; set; }
    public int DaysInactive { get; set; }
    public string RiskLevel { get; set; } = "Kritik"; // Yüksek Risk, Orta Risk, Kritik
}

public class DeadStockDto
{
    public int InactiveDaysThreshold { get; set; }
    public int TotalDeadStockCount { get; set; }
    public int TotalDeadStockQuantity { get; set; }
    public decimal TotalTiedUpCapital { get; set; } // Toplam Atıl Stok Değeri
    public List<DeadStockItemDto> DeadStockItems { get; set; } = new();
}
