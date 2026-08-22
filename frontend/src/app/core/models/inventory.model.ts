export enum TransactionType {
  In = 1,
  Out = 2,
  Adjustment = 3
}

export interface StockMovement {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  unit: string;
  quantity: number;
  transactionType: TransactionType;
  transactionTypeName: string;
  description?: string;
  transactionDate: string;
  userName?: string;
}

export interface CreateStockMovementRequest {
  productId: string;
  quantity: number;
  transactionType: TransactionType;
  description?: string;
}
