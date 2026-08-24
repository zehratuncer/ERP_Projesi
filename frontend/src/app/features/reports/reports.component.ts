import { Component, OnInit, OnDestroy, inject, signal, ElementRef, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportsService } from '../../core/services/reports.service';
import { ToastService } from '../../core/services/toast.service';
import {
  StockTurnoverDto,
  SeasonalDemandTrendsDto,
  DeadStockDto,
  SupplierPerformanceDto,
  CategoryProfitabilityDto
} from '../../core/models/reports.model';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit, OnDestroy {
  private reportsService = inject(ReportsService);
  private toastService = inject(ToastService);

  // Active Report Tab: 'turnover' | 'seasonal' | 'deadstock' | 'suppliers' | 'profitability'
  activeTab = signal<'turnover' | 'seasonal' | 'deadstock' | 'suppliers' | 'profitability'>('profitability');

  // Loading States
  isLoading = signal<boolean>(false);

  // Data Signals
  turnoverData = signal<StockTurnoverDto | null>(null);
  seasonalData = signal<SeasonalDemandTrendsDto | null>(null);
  deadStockData = signal<DeadStockDto | null>(null);
  supplierData = signal<SupplierPerformanceDto | null>(null);
  categoryData = signal<CategoryProfitabilityDto | null>(null);

  // Filter Models
  selectedYear = new Date().getFullYear();
  selectedInactiveDays = 90;
  selectedTimePreset = 'all'; // 'all' | 'year' | '90days' | '30days'
  startDate = '';
  endDate = '';

  // Chart References
  categoryChartRef = viewChild<ElementRef<HTMLCanvasElement>>('categoryChart');
  seasonalChartRef = viewChild<ElementRef<HTMLCanvasElement>>('seasonalChart');
  supplierChartRef = viewChild<ElementRef<HTMLCanvasElement>>('supplierChart');
  turnoverChartRef = viewChild<ElementRef<HTMLCanvasElement>>('turnoverChart');

  private charts: { [key: string]: Chart | null } = {};

  ngOnInit(): void {
    this.loadAllReports();
  }

  ngOnDestroy(): void {
    this.destroyAllCharts();
  }

  setTab(tab: 'turnover' | 'seasonal' | 'deadstock' | 'suppliers' | 'profitability'): void {
    this.activeTab.set(tab);
    setTimeout(() => {
      this.renderActiveTabCharts();
    }, 100);
  }

  loadAllReports(): void {
    this.isLoading.set(true);
    let loadedCount = 0;
    const totalReports = 5;

    const checkComplete = () => {
      loadedCount++;
      if (loadedCount >= totalReports) {
        this.isLoading.set(false);
        setTimeout(() => this.renderActiveTabCharts(), 150);
      }
    };

    // 1. Kategori Kârlılık
    this.reportsService.getCategoryAnalytics().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.categoryData.set(res.data);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // 2. Sezonluk Trendler
    this.reportsService.getSeasonalDemandTrends(this.selectedYear).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.seasonalData.set(res.data);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // 3. Stok Devir Hızı
    this.reportsService.getStockTurnover(this.startDate || undefined, this.endDate || undefined).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.turnoverData.set(res.data);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // 4. Ölü Stoklar
    this.reportsService.getDeadStock(this.selectedInactiveDays).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.deadStockData.set(res.data);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });

    // 5. Tedarikçi Performansı
    this.reportsService.getSupplierPerformance().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.supplierData.set(res.data);
        }
        checkComplete();
      },
      error: () => checkComplete()
    });
  }

  onPresetChange(): void {
    const now = new Date();
    if (this.selectedTimePreset === 'all') {
      this.startDate = '';
      this.endDate = '';
    } else if (this.selectedTimePreset === 'year') {
      this.startDate = `${now.getFullYear()}-01-01`;
      this.endDate = `${now.getFullYear()}-12-31`;
    } else if (this.selectedTimePreset === '90days') {
      const d = new Date();
      d.setDate(d.getDate() - 90);
      this.startDate = d.toISOString().split('T')[0];
      this.endDate = now.toISOString().split('T')[0];
    } else if (this.selectedTimePreset === '30days') {
      const d = new Date();
      d.setDate(d.getDate() - 30);
      this.startDate = d.toISOString().split('T')[0];
      this.endDate = now.toISOString().split('T')[0];
    }
    this.loadAllReports();
  }

  onDeadStockFilterChange(): void {
    this.isLoading.set(true);
    this.reportsService.getDeadStock(this.selectedInactiveDays).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.deadStockData.set(res.data);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ----------------------------------------------------
  // CHART RENDERING (CHART.JS)
  // ----------------------------------------------------
  renderActiveTabCharts(): void {
    const tab = this.activeTab();

    if (tab === 'profitability') {
      this.renderCategoryChart();
    } else if (tab === 'seasonal') {
      this.renderSeasonalChart();
    } else if (tab === 'suppliers') {
      this.renderSupplierChart();
    } else if (tab === 'turnover') {
      this.renderTurnoverChart();
    }
  }

  private destroyChart(key: string): void {
    if (this.charts[key]) {
      this.charts[key]?.destroy();
      this.charts[key] = null;
    }
  }

  private destroyAllCharts(): void {
    Object.keys(this.charts).forEach(key => this.destroyChart(key));
  }

  private renderCategoryChart(): void {
    const canvas = this.categoryChartRef()?.nativeElement;
    const data = this.categoryData();
    if (!canvas || !data || data.categories.length === 0) return;

    this.destroyChart('category');

    const labels = data.categories.map(c => c.categoryName);
    const revenues = data.categories.map(c => c.totalRevenue);
    const stockValues = data.categories.map(c => c.currentStockValue);

    this.charts['category'] = new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Ciro Payı (₺)',
            data: revenues.every(v => v === 0) ? stockValues : revenues,
            backgroundColor: [
              '#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', 
              '#ec4899', '#06b6d4', '#64748b'
            ],
            borderWidth: 2,
            borderColor: '#1e293b'
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#94a3b8', boxWidth: 12, padding: 15 }
          }
        }
      }
    });
  }

  private renderSeasonalChart(): void {
    const canvas = this.seasonalChartRef()?.nativeElement;
    const data = this.seasonalData();
    if (!canvas || !data || data.monthlyTrends.length === 0) return;

    this.destroyChart('seasonal');

    const labels = data.monthlyTrends.map(m => m.monthName);
    const quantities = data.monthlyTrends.map(m => m.totalOutboundQuantity);
    const revenues = data.monthlyTrends.map(m => m.totalSalesAmount);


    this.charts['seasonal'] = new Chart(canvas, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Çıkış / Satış Adedi (Adet)',
            data: quantities,
            borderColor: '#3b82f6',
            backgroundColor: 'rgba(59, 130, 246, 0.15)',
            fill: true,
            tension: 0.4,
            yAxisID: 'y'
          },
          {
            label: 'Aylık Satış Cirosu (₺)',
            data: revenues,
            borderColor: '#10b981',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            fill: false,
            tension: 0.3,
            yAxisID: 'y1'
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          x: { grid: { color: 'rgba(255, 255, 255, 0.05)' }, ticks: { color: '#94a3b8' } },
          y: {
            type: 'linear',
            display: true,
            position: 'left',
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
            ticks: { color: '#94a3b8' }
          },
          y1: {
            type: 'linear',
            display: true,
            position: 'right',
            grid: { drawOnChartArea: false },
            ticks: { color: '#10b981' }
          }
        },
        plugins: {
          legend: { labels: { color: '#94a3b8' } }
        }
      }
    });
  }

  private renderSupplierChart(): void {
    const canvas = this.supplierChartRef()?.nativeElement;
    const data = this.supplierData();
    if (!canvas || !data || data.suppliers.length === 0) return;

    this.destroyChart('supplier');

    const topSuppliers = data.suppliers.slice(0, 7);
    const labels = topSuppliers.map(s => s.supplierName);
    const scores = topSuppliers.map(s => s.reliabilityScore);
    const fulfillment = topSuppliers.map(s => s.fulfillmentRate);

    this.charts['supplier'] = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Güvenilirlik Puanı (100 Üzerinden)',
            data: scores,
            backgroundColor: '#8b5cf6',
            borderRadius: 6
          },
          {
            label: 'Sipariş Karşılama Oranı (%)',
            data: fulfillment,
            backgroundColor: '#3b82f6',
            borderRadius: 6
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          x: { grid: { color: 'rgba(255, 255, 255, 0.05)' }, ticks: { color: '#94a3b8' } },
          y: { max: 100, min: 0, grid: { color: 'rgba(255, 255, 255, 0.05)' }, ticks: { color: '#94a3b8' } }
        },
        plugins: {
          legend: { labels: { color: '#94a3b8' } }
        }
      }
    });
  }

  private renderTurnoverChart(): void {
    const canvas = this.turnoverChartRef()?.nativeElement;
    const data = this.turnoverData();
    if (!canvas || !data || data.turnoverByCategory.length === 0) return;

    this.destroyChart('turnover');

    const labels = data.turnoverByCategory.map(c => c.category);
    const rates = data.turnoverByCategory.map(c => c.turnoverRate);

    this.charts['turnover'] = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Ortalama Stok Devir Hızı Katı',
            data: rates,
            backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#ec4899', '#8b5cf6', '#06b6d4'],
            borderRadius: 6
          }
        ]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          x: { grid: { color: 'rgba(255, 255, 255, 0.05)' }, ticks: { color: '#94a3b8' } },
          y: { grid: { color: 'rgba(255, 255, 255, 0.05)' }, ticks: { color: '#94a3b8' } }
        },
        plugins: {
          legend: { labels: { color: '#94a3b8' } }
        }
      }
    });
  }

  // ----------------------------------------------------
  // EXPORT UTILITIES (CSV / EXCEL & PRINT / PDF)
  // ----------------------------------------------------
  exportCurrentReport(): void {
    const tab = this.activeTab();

    if (tab === 'profitability') {
      const data = this.categoryData();
      if (!data) return;
      const headers = ['Kategori Adı', 'Ürün Çeşidi', 'Satılan Adet', 'Toplam Ciro (TL)', 'Tahmini Maliyet (TL)', 'Brüt Kâr (TL)', 'Kâr Marjı (%)', 'Stok Değeri (TL)'];
      const rows = data.categories.map(c => [
        c.categoryName, c.productCount, c.totalUnitsSold, c.totalRevenue, c.estimatedCost, c.grossProfit, `%${c.profitMarginPercentage}`, c.currentStockValue
      ]);
      this.reportsService.exportToCsv('Kategori_Karlilik_Raporu', headers, rows);
      this.toastService.success('Kategori Kârlılık Raporu Excel/CSV formatında indirildi.');
    } else if (tab === 'seasonal') {
      const data = this.seasonalData();
      if (!data) return;
      const headers = ['Ay', 'Sezon Etiketi', 'Çıkış/Satış Adedi', 'Satış Cirosu (TL)', 'İşlem Sayısı'];
      const rows = data.monthlyTrends.map(m => [
        m.monthName, m.seasonTag, m.totalOutboundQuantity, m.totalSalesAmount, m.transactionCount
      ]);
      this.reportsService.exportToCsv('Sezonluk_Talep_Trendleri', headers, rows);
      this.toastService.success('Sezonluk Talep Raporu Excel/CSV formatında indirildi.');
    }
 else if (tab === 'deadstock') {
      const data = this.deadStockData();
      if (!data) return;
      const headers = ['Ürün Kodu', 'Ürün Adı', 'Kategori', 'Mevcut Stok', 'Birim', 'Birim Fiyat (TL)', 'Bağlanan Sermaye (TL)', 'Hareketsiz Gün', 'Risk Derecesi'];
      const rows = data.deadStockItems.map(d => [
        d.productCode, d.productName, d.category, d.currentStock, d.unit, d.unitPrice, d.totalTiedUpValue, d.daysInactive, d.riskLevel
      ]);
      this.reportsService.exportToCsv('Atil_Olu_Stok_Raporu', headers, rows);
      this.toastService.success('Atıl/Ölü Stok Raporu Excel/CSV formatında indirildi.');
    } else if (tab === 'turnover') {
      const data = this.turnoverData();
      if (!data) return;
      const headers = ['Ürün Kodu', 'Ürün Adı', 'Kategori', 'Mevcut Stok', 'Satılan Miktar', 'Ciro (TL)', 'Devir Hızı', 'Tükenme Süresi (Gün)', 'Hız Sınıfı'];
      const rows = [...data.topFastMovingProducts, ...data.topSlowMovingProducts].map(p => [
        p.productCode, p.productName, p.category, p.currentStock, p.totalSoldQuantity, p.totalRevenue, p.turnoverRate, p.daysToSellOut, p.velocityCategory
      ]);
      this.reportsService.exportToCsv('Stok_Devir_Hizi_Raporu', headers, rows);
      this.toastService.success('Stok Devir Hızı Raporu Excel/CSV formatında indirildi.');
    } else if (tab === 'suppliers') {
      const data = this.supplierData();
      if (!data) return;
      const headers = ['Tedarikçi Adı', 'Ürün Çeşidi', 'Tamamlanan Sipariş', 'Bekleyen Sipariş', 'Tedarik Hacmi (TL)', 'Ortalama Teslimat (Gün)', 'Karşılama (%)', 'Güvenilirlik Puanı', 'Performans Derecesi'];
      const rows = data.suppliers.map(s => [
        s.supplierName, s.suppliedProductCount, s.completedRequestCount, s.pendingRequestCount, s.totalSuppliedAmount, s.averageDeliveryDays, `%${s.fulfillmentRate}`, s.reliabilityScore, s.performanceGrade
      ]);
      this.reportsService.exportToCsv('Tedarikci_Performans_Raporu', headers, rows);
      this.toastService.success('Tedarikçi Performans Raporu Excel/CSV formatında indirildi.');
    }
  }

  printReport(): void {
    window.print();
  }
}
