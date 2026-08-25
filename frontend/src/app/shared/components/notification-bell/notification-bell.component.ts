import { Component, OnInit, HostListener, ElementRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationItem, NotificationType } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss'
})
export class NotificationBellComponent implements OnInit {
  public notificationService = inject(NotificationService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  isOpen = signal<boolean>(false);
  readonly NotificationType = NotificationType;

  ngOnInit(): void {
    this.notificationService.loadNotifications(10).subscribe();
    this.notificationService.loadUnreadCount().subscribe();
  }

  toggleDropdown(): void {
    this.isOpen.update((v) => !v);
    if (this.isOpen()) {
      this.notificationService.loadNotifications(10).subscribe();
      this.notificationService.loadUnreadCount().subscribe();
    }
  }

  closeDropdown(): void {
    this.isOpen.set(false);
  }

  onNotificationClick(item: NotificationItem): void {
    if (!item.isRead) {
      this.notificationService.markAsRead(item.id).subscribe();
    }
    this.closeDropdown();
    if (item.actionUrl) {
      this.router.navigateByUrl(item.actionUrl);
    }
  }

  markAllAsRead(event: Event): void {
    event.stopPropagation();
    this.notificationService.markAllAsRead().subscribe();
  }

  goToNotificationCenter(): void {
    this.closeDropdown();
    this.router.navigate(['/notifications']);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }
}
