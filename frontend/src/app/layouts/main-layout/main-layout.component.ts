import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss'
})
export class MainLayoutComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  userInitial = computed(() => {
    const name = this.authService.currentUser()?.fullName || 'A';
    return name.charAt(0).toUpperCase();
  });

  currentRouteTitle = computed(() => {
    const url = this.router.url;
    if (url.includes('pos')) return '🛒 Barkodlu Hızlı Kasa (POS)';
    if (url.includes('inventory')) return '📦 Stok & Ürün Yönetimi';
    if (url.includes('suppliers')) return '🚚 Tedarikçi Yönetimi';
    return '📊 Yönetici Dashboard';
  });

  logout() {
    this.authService.logout();
  }
}
