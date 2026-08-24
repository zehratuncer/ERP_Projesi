namespace ERP.Application.Features.Pos.DTOs;

public class DailyPosSummaryDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalSalesCount { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal CashTotal { get; set; }
    public decimal CreditCardTotal { get; set; }
    public decimal SplitTotal { get; set; }
    public decimal OnAccountTotal { get; set; }
    public decimal TotalDiscountsGiven { get; set; }
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
}

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
