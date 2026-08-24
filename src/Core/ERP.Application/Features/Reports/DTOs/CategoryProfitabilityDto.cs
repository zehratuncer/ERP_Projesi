namespace ERP.Application.Features.Reports.DTOs;

public class CategoryProfitabilityItemDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalUnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal GrossProfit { get; set; }
    public double ProfitMarginPercentage { get; set; }
    public decimal CurrentStockValue { get; set; }
}

public class CategoryProfitabilityDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalGrossProfit { get; set; }
    public double OverallProfitMargin { get; set; }
    public decimal TotalInventoryValuation { get; set; }
    public List<CategoryProfitabilityItemDto> Categories { get; set; } = new();
}
