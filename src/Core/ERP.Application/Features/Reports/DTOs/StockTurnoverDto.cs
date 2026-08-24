namespace ERP.Application.Features.Reports.DTOs;

public class ProductTurnoverItemDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int TotalSoldQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TurnoverRate { get; set; }
    public double DaysToSellOut { get; set; }
    public string VelocityCategory { get; set; } = "Normal"; // Hızlı (Fast), Normal, Yavaş (Slow)
}

public class CategoryTurnoverDto
{
    public string Category { get; set; } = string.Empty;
    public int TotalSoldQuantity { get; set; }
    public int CurrentStock { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public double TurnoverRate { get; set; }
}

public class StockTurnoverDto
{
    public double OverallTurnoverRate { get; set; }
    public double AverageDaysToSell { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal TotalSalesRevenue { get; set; }
    public List<ProductTurnoverItemDto> TopFastMovingProducts { get; set; } = new();
    public List<ProductTurnoverItemDto> TopSlowMovingProducts { get; set; } = new();
    public List<CategoryTurnoverDto> TurnoverByCategory { get; set; } = new();
}
