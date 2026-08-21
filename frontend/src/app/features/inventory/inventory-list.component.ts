import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h3>📦 Stok & Ürün Yönetimi</h3>
        <button class="btn btn-primary btn-sm">+ Yeni Ürün Ekle</button>
      </div>
      <div class="card-body">
        <p>Ürün yönetimi ve stok giriş/çıkış modülü (Faz 3 kapsamında backend bağlantısı ile tamamlanacaktır).</p>
      </div>
    </div>
  `
})
export class InventoryListComponent {}
