export interface Product {
  id: string;
  code: string;
  name: string;
  description?: string;
  unit: string;
  currentStock: number;
  minStockLevel: number;
  unitPrice: number;
  isActive: boolean;
  isLowStock?: boolean;
  supplierId?: string;
  supplierName?: string;
  createdDate: string;
  updatedDate?: string;
}

export interface CreateProductRequest {
  code: string;
  name: string;
  description?: string;
  unit: string;
  initialStock: number;
  minStockLevel: number;
  unitPrice: number;
  supplierId?: string;
}

export interface UpdateProductRequest {
  id: string;
  name: string;
  description?: string;
  unit: string;
  minStockLevel: number;
  unitPrice: number;
  isActive: boolean;
  supplierId?: string;
}
