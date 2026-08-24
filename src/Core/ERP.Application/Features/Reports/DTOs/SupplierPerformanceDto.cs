namespace ERP.Application.Features.Reports.DTOs;

public class SupplierPerformanceItemDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public int SuppliedProductCount { get; set; }
    public int CompletedRequestCount { get; set; }
    public int PendingRequestCount { get; set; }
    public decimal TotalSuppliedAmount { get; set; }
    public double AverageDeliveryDays { get; set; }
    public double FulfillmentRate { get; set; } // Sipariş Karşılama Oranı %
    public double ReliabilityScore { get; set; } // 100 üzerinden güvenilirlik puanı
    public string PerformanceGrade { get; set; } = "A"; // A (Mükemmel), B (İyi), C (Gelişmeli)
}

public class SupplierPerformanceDto
{
    public int TotalSuppliers { get; set; }
    public double AverageOverallFulfillmentRate { get; set; }
    public decimal TotalProcuredVolume { get; set; }
    public List<SupplierPerformanceItemDto> Suppliers { get; set; } = new();
}
