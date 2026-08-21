import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, StatCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private toastService = inject(ToastService);

  totalProducts = 142;
  criticalStockCount = 4;
  totalSuppliers = 18;

  criticalProducts = [
    { code: 'PRD-001', name: 'M4 Çelik Civata (100 lük Paket)', currentStock: 8, minStock: 25, unit: 'Paket' },
    { code: 'PRD-042', name: 'Hidrolik Yağ (20L Varil)', currentStock: 2, minStock: 5, unit: 'Varil' },
    { code: 'PRD-089', name: 'Endüstriyel Rulman 6204-2RS', currentStock: 12, minStock: 30, unit: 'Adet' },
    { code: 'PRD-114', name: 'Sızdırmazlık Contası NBR 70', currentStock: 15, minStock: 50, unit: 'Adet' }
  ];

  recentMovements = [
    { id: 1, type: 'IN', productName: 'M4 Çelik Civata', quantity: 50, user: 'Ahmet Yılmaz (Depo Sorumlusu)', time: '10 dakika önce' },
    { id: 2, type: 'OUT', productName: 'Hidrolik Yağ', quantity: 3, user: 'Mehmet Demir (Üretim)', time: '45 dakika önce' },
    { id: 3, type: 'OUT', productName: 'Endüstriyel Rulman', quantity: 10, user: 'Ali Kaya (Montaj)', time: '2 saat önce' },
    { id: 4, type: 'IN', productName: 'Sızdırmazlık Contası', quantity: 100, user: 'Ahmet Yılmaz (Depo Sorumlusu)', time: '4 saat önce' }
  ];

  ngOnInit() {
    this.toastService.info('ERP Dashboard verileri güncellendi.', 3000);
  }
}
