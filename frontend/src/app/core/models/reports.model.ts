export interface ProductTurnoverItemDto {
  productId: string;
  productCode: string;
  productName: string;
  category: string;
  currentStock: number;
  totalSoldQuantity: number;
  totalRevenue: number;
  turnoverRate: number;
  daysToSellOut: number;
  velocityCategory: string; // Hızlı, Normal, Yavaş
}

export interface CategoryTurnoverDto {
  category: string;
  totalSoldQuantity: number;
  currentStock: number;
  totalSalesAmount: number;
  turnoverRate: number;
}

export interface StockTurnoverDto {
  overallTurnoverRate: number;
  averageDaysToSell: number;
  totalItemsSold: number;
  totalSalesRevenue: number;
  topFastMovingProducts: ProductTurnoverItemDto[];
  topSlowMovingProducts: ProductTurnoverItemDto[];
  turnoverByCategory: CategoryTurnoverDto[];
}

export interface MonthlyDemandDto {
  month: number;
  monthName: string;
  seasonTag: string;
  totalOutboundQuantity: number;
  totalSalesAmount: number;
  transactionCount: number;
}

export interface SeasonalCategoryTrendDto {
  categoryName: string;
  schoolSeasonSales: number;
  examSeasonSales: number;
  officeRoutineSales: number;
  totalSales: number;
  peakSeason: string;
}

export interface SeasonalDemandTrendsDto {
  year: number;
  peakSeasonName: string;
  seasonalityIndex: number;
  monthlyTrends: MonthlyDemandDto[];
  categorySeasonalBreakdown: SeasonalCategoryTrendDto[];
}

export interface DeadStockItemDto {
  productId: string;
  productCode: string;
  productName: string;
  category: string;
  currentStock: number;
  unit: string;
  unitPrice: number;
  totalTiedUpValue: number;
  lastMovementDate?: string;
  daysInactive: number;
  riskLevel: string; // Kritik, Yüksek Risk, Orta Risk
}

export interface DeadStockDto {
  inactiveDaysThreshold: number;
  totalDeadStockCount: number;
  totalDeadStockQuantity: number;
  totalTiedUpCapital: number;
  deadStockItems: DeadStockItemDto[];
}

export interface SupplierPerformanceItemDto {
  supplierId: string;
  supplierName: string;
  contactPerson?: string;
  email?: string;
  suppliedProductCount: number;
  completedRequestCount: number;
  pendingRequestCount: number;
  totalSuppliedAmount: number;
  averageDeliveryDays: number;
  fulfillmentRate: number;
  reliabilityScore: number;
  performanceGrade: string; // A, B, C
}

export interface SupplierPerformanceDto {
  totalSuppliers: number;
  averageOverallFulfillmentRate: number;
  totalProcuredVolume: number;
  suppliers: SupplierPerformanceItemDto[];
}

export interface CategoryProfitabilityItemDto {
  categoryName: string;
  productCount: number;
  totalUnitsSold: number;
  totalRevenue: number;
  estimatedCost: number;
  grossProfit: number;
  profitMarginPercentage: number;
  currentStockValue: number;
}

export interface CategoryProfitabilityDto {
  totalRevenue: number;
  totalGrossProfit: number;
  overallProfitMargin: number;
  totalInventoryValuation: number;
  categories: CategoryProfitabilityItemDto[];
}
