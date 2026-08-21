import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h3>🚚 Tedarikçi Yönetimi</h3>
        <button class="btn btn-primary btn-sm">+ Yeni Tedarikçi Ekle</button>
      </div>
      <div class="card-body">
        <p>Tedarikçi firmalar ve ürün eşleştirmeleri (Faz 4 kapsamında tamamlanacaktır).</p>
      </div>
    </div>
  `
})
export class SupplierListComponent {}
