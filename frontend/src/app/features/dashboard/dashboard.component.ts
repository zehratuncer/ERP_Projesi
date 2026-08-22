import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { DashboardService } from '../../core/services/dashboard.service';
import { ToastService } from '../../core/services/toast.service';
import { DashboardSummary } from '../../core/models/dashboard.model';
import { TransactionType } from '../../core/models/inventory.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, StatCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  summary: DashboardSummary | null = null;
  isLoading = true;
  hasError = false;
  lastUpdated: Date = new Date();

  readonly TransactionType = TransactionType;

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData(isManualRefresh = false) {
    this.isLoading = true;
    this.hasError = false;
    this.cdr.markForCheck();

    this.dashboardService.getDashboardSummary().subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.isSuccess && res.data) {
          this.summary = res.data;
          this.lastUpdated = new Date();
          if (isManualRefresh) {
            this.toastService.success('Dashboard verileri güncellendi.');
          }
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.hasError = true;
        this.toastService.error('Dashboard verileri yüklenirken bir hata oluştu.');
        this.cdr.markForCheck();
      }
    });
  }

  getTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHours = Math.floor(diffMin / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffSec < 60) return 'Az önce';
    if (diffMin < 60) return `${diffMin} dakika önce`;
    if (diffHours < 24) return `${diffHours} saat önce`;
    if (diffDays === 1) return 'Dün';
    if (diffDays < 30) return `${diffDays} gün önce`;
    return date.toLocaleDateString('tr-TR');
  }
}
