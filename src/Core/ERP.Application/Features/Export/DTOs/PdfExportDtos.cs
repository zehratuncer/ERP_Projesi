namespace ERP.Application.Features.Export.DTOs;

public class PurchaseRequestPdfItemDto
{
    public int ItemIndex { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = "Adet";
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedTotalPrice { get; set; }
    public string? Note { get; set; }
}

public class PurchaseRequestPdfApprovalDto
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public string? Comment { get; set; }
}

public class PurchaseRequestPdfDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? Description { get; set; }
    public decimal TotalEstimatedAmount { get; set; }
    public List<PurchaseRequestPdfItemDto> Items { get; set; } = new();
    public List<PurchaseRequestPdfApprovalDto> Approvals { get; set; } = new();
}

public class StockReceiptPdfDto
{
    public Guid TransactionId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = "Stok Girişi"; // Giriş, Çıkış
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public string? OperatorName { get; set; }
    public string? SupplierOrDepartment { get; set; }
}
