export enum RequestPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4
}

export enum RequestStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Rejected = 4,
  Completed = 5,
  Cancelled = 6
}

export enum ApprovalAction {
  Approved = 1,
  Rejected = 2,
  Revised = 3
}

export interface ApprovalHistoryDto {
  id: string;
  purchaseRequestId: string;
  approverUserId?: string;
  approverUserName: string;
  stepNumber: number;
  stepName: string;
  action: ApprovalAction;
  actionName: string;
  comment?: string;
  actionDate: string;
}

export interface PurchaseRequestItemDto {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  currentStock: number;
  minStockLevel: number;
  requestedQuantity: number;
  unit: string;
  estimatedUnitPrice: number;
  estimatedTotalPrice: number;
  notes?: string;
}

export interface PurchaseRequest {
  id: string;
  requestNumber: string;
  department: string;
  requesterUserId?: string;
  requesterUserName: string;
  priority: RequestPriority;
  priorityName: string;
  status: RequestStatus;
  statusName: string;
  totalEstimatedAmount: number;
  currentApprovalStep: number;
  requiredDate?: string;
  note?: string;
  createdDate: string;
  items: PurchaseRequestItemDto[];
  approvalHistories: ApprovalHistoryDto[];
}

export interface PurchaseRequestListItem {
  id: string;
  requestNumber: string;
  department: string;
  requesterUserId?: string;
  requesterUserName: string;
  priority: RequestPriority;
  priorityName: string;
  status: RequestStatus;
  statusName: string;
  itemCount: number;
  totalEstimatedAmount: number;
  currentApprovalStep: number;
  requiredDate?: string;
  createdDate: string;
  note?: string;
}

export interface CreatePurchaseRequestItemRequest {
  productId: string;
  requestedQuantity: number;
  unit?: string;
  estimatedUnitPrice?: number;
  notes?: string;
}

export interface CreatePurchaseRequestRequest {
  department: string;
  priority: RequestPriority;
  requiredDate?: string;
  note?: string;
  items: CreatePurchaseRequestItemRequest[];
  submitForApproval: boolean;
}

export interface UpdatePurchaseRequestRequest {
  id: string;
  department: string;
  priority: RequestPriority;
  requiredDate?: string;
  note?: string;
  items: CreatePurchaseRequestItemRequest[];
  submitForApproval: boolean;
}
