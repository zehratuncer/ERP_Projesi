import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Observable, tap } from 'rxjs';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';
import { NotificationItem, UnreadNotificationCount, NotificationType } from '../models/notification.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  private apiUrl = 'http://localhost:5000/api/notifications';
  private hubUrl = 'http://localhost:5000/hubs/notifications';
  private hubConnection: HubConnection | null = null;

  // Reactive State Signals
  notifications = signal<NotificationItem[]>([]);
  unreadCount = signal<number>(0);
  isConnected = signal<boolean>(false);

  constructor() {
    // Kullanıcı oturum açtığında SignalR bağlantısını kur ve bildirimleri yükle
    if (this.authService.isAuthenticated()) {
      this.initSignalR();
      this.loadNotifications();
      this.loadUnreadCount();
    }
  }

  /**
   * SignalR Hub bağlantısını başlatır ve ReceiveNotification olayını dinler.
   */
  public initSignalR(): void {
    if (this.hubConnection && this.hubConnection.state === HubConnectionState.Connected) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() || ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: NotificationItem) => {
      this.handleIncomingNotification(notification);
    });

    this.hubConnection
      .start()
      .then(() => {
        this.isConnected.set(true);
      })
      .catch((err) => {
        this.isConnected.set(false);
        console.warn('SignalR Hub bağlantı hatası (Yeniden denenecek):', err);
      });

    this.hubConnection.onreconnected(() => {
      this.isConnected.set(true);
      this.loadUnreadCount();
    });

    this.hubConnection.onclose(() => {
      this.isConnected.set(false);
    });
  }

  /**
   * SignalR bağlantısını sonlandırır.
   */
  public stopSignalR(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
      this.isConnected.set(false);
    }
  }

  /**
   * Anlık gelen bildirimi işler, listeye ekler ve kullanıcıya görsel uyarı sunar.
   */
  private handleIncomingNotification(notification: NotificationItem): void {
    // Listeye başa ekle
    this.notifications.update((prev) => [notification, ...prev]);
    this.unreadCount.update((count) => count + 1);

    // Toast bildirimi göster
    if (notification.type === NotificationType.StockAlert) {
      this.toastService.warning(`${notification.title}: ${notification.message}`);
    } else if (notification.type === NotificationType.ApprovalNeeded) {
      this.toastService.info(`${notification.title}: ${notification.message}`);
    } else if (notification.type === NotificationType.Success) {
      this.toastService.success(`${notification.title}: ${notification.message}`);
    } else if (notification.type === NotificationType.Warning) {
      this.toastService.warning(`${notification.title}: ${notification.message}`);
    } else {
      this.toastService.info(`${notification.title}: ${notification.message}`);
    }

    this.playNotificationSound();
  }

  /**
   * Bildirim geldiğinde hafif bir ses efekti çalar.
   */
  private playNotificationSound(): void {
    try {
      const audioCtx = new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)();
      const osc = audioCtx.createOscillator();
      const gain = audioCtx.createGain();
      osc.connect(gain);
      gain.connect(audioCtx.destination);
      osc.type = 'sine';
      osc.frequency.setValueAtTime(587.33, audioCtx.currentTime); // D5
      osc.frequency.setValueAtTime(880, audioCtx.currentTime + 0.08); // A5
      gain.gain.setValueAtTime(0.05, audioCtx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + 0.35);
      osc.start();
      osc.stop(audioCtx.currentTime + 0.35);
    } catch {
      // AudioContext engellenirse sessizce geç
    }
  }

  /**
   * Backend'den son bildirimleri çeker.
   */
  loadNotifications(limit: number = 20): Observable<ApiResponse<NotificationItem[]>> {
    return this.http.get<ApiResponse<NotificationItem[]>>(`${this.apiUrl}?limit=${limit}`).pipe(
      tap((res) => {
        if (res.isSuccess && res.data) {
          this.notifications.set(res.data);
        }
      })
    );
  }

  /**
   * Okunmamış bildirim sayısını çeker.
   */
  loadUnreadCount(): Observable<ApiResponse<UnreadNotificationCount>> {
    return this.http.get<ApiResponse<UnreadNotificationCount>>(`${this.apiUrl}/unread-count`).pipe(
      tap((res) => {
        if (res.isSuccess && res.data) {
          this.unreadCount.set(res.data.count);
        }
      })
    );
  }

  /**
   * Bildirimi okundu olarak işaretler.
   */
  markAsRead(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap((res) => {
        if (res.isSuccess) {
          this.notifications.update((items) =>
            items.map((item) => (item.id === id ? { ...item, isRead: true } : item))
          );
          this.unreadCount.update((c) => Math.max(0, c - 1));
        }
      })
    );
  }

  /**
   * Tüm bildirimleri okundu olarak işaretler.
   */
  markAllAsRead(): Observable<ApiResponse<number>> {
    return this.http.put<ApiResponse<number>>(`${this.apiUrl}/read-all`, {}).pipe(
      tap((res) => {
        if (res.isSuccess) {
          this.notifications.update((items) =>
            items.map((item) => ({ ...item, isRead: true }))
          );
          this.unreadCount.set(0);
          this.toastService.success('Tüm bildirimler okundu olarak işaretlendi.');
        }
      })
    );
  }
}
