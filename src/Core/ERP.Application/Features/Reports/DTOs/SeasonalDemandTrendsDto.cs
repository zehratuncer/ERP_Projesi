namespace ERP.Application.Features.Reports.DTOs;

public class MonthlyDemandDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string SeasonTag { get; set; } = string.Empty; // Örn: Okul Açılış Sezonu, Sınav Dönemi, Rutin Ofis
    public int TotalOutboundQuantity { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int TransactionCount { get; set; }
}

public class SeasonalCategoryTrendDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int SchoolSeasonSales { get; set; } // Ağustos - Ekim
    public int ExamSeasonSales { get; set; }   // Ocak - Mart
    public int OfficeRoutineSales { get; set; } // Diğer Aylar
    public int TotalSales { get; set; }
    public string PeakSeason { get; set; } = string.Empty;
}

public class SeasonalDemandTrendsDto
{
    public int Year { get; set; }
    public string PeakSeasonName { get; set; } = "Okul Açılış Sezonu (Ağustos - Ekim)";
    public double SeasonalityIndex { get; set; }
    public List<MonthlyDemandDto> MonthlyTrends { get; set; } = new();
    public List<SeasonalCategoryTrendDto> CategorySeasonalBreakdown { get; set; } = new();
}
