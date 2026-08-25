import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification.service';
import { NotificationItem, NotificationType } from '../../core/models/notification.model';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  public notificationService = inject(NotificationService);
  private router = inject(Router);

  // Active filter tab: 'all' | 'unread' | 'approvals' | 'stock'
  activeTab = signal<'all' | 'unread' | 'approvals' | 'stock'>('all');
  searchQuery = signal<string>('');
  isLoading = signal<boolean>(false);

  readonly NotificationType = NotificationType;

  filteredNotifications = computed(() => {
    const list = this.notificationService.notifications();
    const tab = this.activeTab();
    const query = this.searchQuery().trim().toLowerCase();

    return list.filter((item) => {
      // Tab filter
      if (tab === 'unread' && item.isRead) return false;
      if (tab === 'approvals' && item.type !== NotificationType.ApprovalNeeded) return false;
      if (tab === 'stock' && item.type !== NotificationType.StockAlert) return false;

      // Search filter
      if (query) {
        const matchesTitle = item.title.toLowerCase().includes(query);
        const matchesMsg = item.message.toLowerCase().includes(query);
        return matchesTitle || matchesMsg;
      }

      return true;
    });
  });

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.isLoading.set(true);
    this.notificationService.loadNotifications(50).subscribe({
      next: () => this.isLoading.set(false),
      error: () => this.isLoading.set(false)
    });
    this.notificationService.loadUnreadCount().subscribe();
  }

  markAsRead(item: NotificationItem, event?: Event): void {
    if (event) event.stopPropagation();
    if (!item.isRead) {
      this.notificationService.markAsRead(item.id).subscribe();
    }
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  navigateToItem(item: NotificationItem): void {
    this.markAsRead(item);
    if (item.actionUrl) {
      this.router.navigateByUrl(item.actionUrl);
    }
  }
}
