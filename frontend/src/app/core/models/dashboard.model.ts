import { Product } from './product.model';
import { StockMovement } from './inventory.model';

export interface DashboardSummary {
  totalProductsCount: number;
  criticalStockCount: number;
  totalSuppliersCount: number;
  totalInventoryQuantity: number;
  totalInventoryValue: number;
  recentStockMovements: StockMovement[];
  criticalStockAlerts: Product[];
}
