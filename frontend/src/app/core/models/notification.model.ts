export enum NotificationType {
  Info = 1,
  Warning = 2,
  StockAlert = 3,
  ApprovalNeeded = 4,
  Success = 5
}

export interface NotificationItem {
  id: string;
  userId?: string;
  roleName?: string;
  title: string;
  message: string;
  type: NotificationType;
  typeName: string;
  isRead: boolean;
  actionUrl?: string;
  createdDate: string;
  timeAgo: string;
}

export interface UnreadNotificationCount {
  count: number;
}
