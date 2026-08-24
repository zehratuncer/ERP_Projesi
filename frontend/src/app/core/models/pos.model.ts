export enum PaymentMethod {
  Cash = 1,
  CreditCard = 2,
  Split = 3,
  OnAccount = 4
}

export interface PosProduct {
  id: string;
  code: string;
  name: string;
  description?: string;
  unit: string;
  currentStock: number;
  minStockLevel: number;
  unitPrice: number;
  isLowStock: boolean;
  isActive: boolean;
  supplierName?: string;
}

export interface CartItem {
  product: PosProduct;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  totalPrice: number;
}

export interface CompleteSaleItemRequest {
  productId: string;
  quantity: number;
  customUnitPrice?: number;
  discountRate: number;
}

export interface CompleteSaleRequest {
  items: CompleteSaleItemRequest[];
  paymentMethod: PaymentMethod;
  customerName?: string;
  generalDiscountAmount: number;
}

export interface SaleItemDto {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  discountRate: number;
}

export interface SaleReceipt {
  id: string;
  receiptNumber: string;
  saleDate: string;
  cashierUserId?: string;
  cashierName: string;
  customerName?: string;
  paymentMethod: PaymentMethod;
  paymentMethodName: string;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  items: SaleItemDto[];
  criticalStockAlerts: string[];
}

export interface TopSellingProduct {
  productId: string;
  productCode: string;
  productName: string;
  totalQuantitySold: number;
  totalRevenue: number;
}

export interface DailyPosSummary {
  date: string;
  totalRevenue: number;
  totalSalesCount: number;
  totalItemsSold: number;
  cashTotal: number;
  creditCardTotal: number;
  splitTotal: number;
  onAccountTotal: number;
  totalDiscountsGiven: number;
  topSellingProducts: TopSellingProduct[];
}

export interface SaleHistoryItem {
  id: string;
  receiptNumber: string;
  saleDate: string;
  cashierName: string;
  customerName?: string;
  paymentMethod: PaymentMethod;
  paymentMethodName: string;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  itemCount: number;
}
