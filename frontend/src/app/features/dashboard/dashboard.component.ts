import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { DashboardService } from '../../core/services/dashboard.service';
import { ProductService } from '../../core/services/product.service';
import { SupplierService } from '../../core/services/supplier.service';
import { ToastService } from '../../core/services/toast.service';
import { DashboardSummary } from '../../core/models/dashboard.model';
import { TransactionType } from '../../core/models/inventory.model';
import { Product, CreateProductRequest } from '../../core/models/product.model';
import { Supplier, CreateSupplierRequest } from '../../core/models/supplier.model';

export type StatModalType = 'products' | 'critical-stock' | 'suppliers' | 'inventory-qty' | 'inventory-val';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StatCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private productService = inject(ProductService);
  private supplierService = inject(SupplierService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  summary: DashboardSummary | null = null;
  isLoading = true;
  hasError = false;
  lastUpdated: Date = new Date();

  readonly TransactionType = TransactionType;

  // Stat detail modals
  activeDetailModal: StatModalType | null = null;
  isDetailLoading = false;
  detailSearchQuery = '';
  modalProducts: Product[] = [];
  modalSuppliers: Supplier[] = [];

  // Quick Action Modal: Create Product
  isCreateProductModalOpen = false;
  isProductSubmitting = false;
  productForm: CreateProductRequest = {
    code: '',
    name: '',
    description: '',
    unit: 'Adet',
    initialStock: 0,
    minStockLevel: 5,
    unitPrice: 0,
    supplierId: undefined
  };

  // Quick Action Modal: Create Supplier
  isCreateSupplierModalOpen = false;
  isSupplierSubmitting = false;
  supplierForm: CreateSupplierRequest = {
    name: '',
    contactPerson: '',
    email: '',
    phone: '',
    address: '',
    taxNumber: ''
  };

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

  // ==========================================
  // STAT DETAIL MODALS
  // ==========================================
  openStatModal(type: StatModalType) {
    this.activeDetailModal = type;
    this.detailSearchQuery = '';
    this.isDetailLoading = true;
    this.cdr.markForCheck();

    if (type === 'suppliers') {
      this.supplierService.getSuppliers().subscribe({
        next: (res) => {
          this.modalSuppliers = res.data || [];
          this.isDetailLoading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.isDetailLoading = false;
          this.toastService.error('Tedarikçi listesi yüklenemedi.');
          this.cdr.markForCheck();
        }
      });
    } else {
      this.productService.getProducts().subscribe({
        next: (res) => {
          this.modalProducts = res.data || [];
          this.isDetailLoading = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.isDetailLoading = false;
          this.toastService.error('Ürün listesi yüklenemedi.');
          this.cdr.markForCheck();
        }
      });
    }
  }

  closeDetailModal() {
    this.activeDetailModal = null;
    this.detailSearchQuery = '';
    this.cdr.markForCheck();
  }

  get filteredModalProducts(): Product[] {
    let list = this.modalProducts;
    if (this.activeDetailModal === 'critical-stock') {
      list = list.filter(p => p.currentStock <= p.minStockLevel);
    }
    if (!this.detailSearchQuery.trim()) {
      return list;
    }
    const q = this.detailSearchQuery.toLowerCase();
    return list.filter(p =>
      p.name.toLowerCase().includes(q) ||
      p.code.toLowerCase().includes(q) ||
      (p.description && p.description.toLowerCase().includes(q)) ||
      (p.supplierName && p.supplierName.toLowerCase().includes(q))
    );
  }

  get filteredModalSuppliers(): Supplier[] {
    if (!this.detailSearchQuery.trim()) {
      return this.modalSuppliers;
    }
    const q = this.detailSearchQuery.toLowerCase();
    return this.modalSuppliers.filter(s =>
      s.name.toLowerCase().includes(q) ||
      (s.contactPerson && s.contactPerson.toLowerCase().includes(q)) ||
      (s.email && s.email.toLowerCase().includes(q)) ||
      (s.taxNumber && s.taxNumber.includes(q))
    );
  }

  // ==========================================
  // QUICK ACCESS: CREATE PRODUCT MODAL
  // ==========================================
  openCreateProductModal() {
    const randomSuffix = Math.floor(1000 + Math.random() * 9000);
    this.productForm = {
      code: `KRT-${randomSuffix}`,
      name: '',
      description: '',
      unit: 'Adet',
      initialStock: 0,
      minStockLevel: 10,
      unitPrice: 0,
      supplierId: undefined
    };
    this.isCreateProductModalOpen = true;

    // Preload suppliers for dropdown
    if (this.modalSuppliers.length === 0) {
      this.supplierService.getSuppliers(undefined, true).subscribe(res => {
        this.modalSuppliers = res.data || [];
        this.cdr.markForCheck();
      });
    }
    this.cdr.markForCheck();
  }

  closeCreateProductModal() {
    this.isCreateProductModalOpen = false;
    this.cdr.markForCheck();
  }

  saveProduct() {
    if (!this.productForm.name?.trim()) {
      this.toastService.warning('Lütfen ürün adını giriniz.');
      return;
    }
    if (!this.productForm.code?.trim()) {
      this.toastService.warning('Lütfen ürün kodunu giriniz.');
      return;
    }
    if (this.productForm.unitPrice < 0) {
      this.toastService.warning('Birim fiyat negatif olamaz.');
      return;
    }

    this.isProductSubmitting = true;
    this.cdr.markForCheck();

    this.productService.createProduct(this.productForm).subscribe({
      next: (res) => {
        this.isProductSubmitting = false;
        if (res.isSuccess) {
          this.toastService.success(`"${this.productForm.name}" başarıyla eklendi!`);
          this.closeCreateProductModal();
          this.loadDashboardData();
        } else {
          this.toastService.error(res.message || 'Ürün kaydedilemedi.');
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isProductSubmitting = false;
        this.toastService.error(err.error?.message || 'Ürün kaydedilirken sunucu hatası oluştu.');
        this.cdr.markForCheck();
      }
    });
  }

  // ==========================================
  // QUICK ACCESS: CREATE SUPPLIER MODAL
  // ==========================================
  openCreateSupplierModal() {
    this.supplierForm = {
      name: '',
      contactPerson: '',
      email: '',
      phone: '',
      address: '',
      taxNumber: ''
    };
    this.isCreateSupplierModalOpen = true;
    this.cdr.markForCheck();
  }

  closeCreateSupplierModal() {
    this.isCreateSupplierModalOpen = false;
    this.cdr.markForCheck();
  }

  saveSupplier() {
    if (!this.supplierForm.name?.trim()) {
      this.toastService.warning('Lütfen firma adını giriniz.');
      return;
    }

    this.isSupplierSubmitting = true;
    this.cdr.markForCheck();

    this.supplierService.createSupplier(this.supplierForm).subscribe({
      next: (res) => {
        this.isSupplierSubmitting = false;
        if (res.isSuccess) {
          this.toastService.success(`"${this.supplierForm.name}" tedarikçisi başarıyla eklendi!`);
          this.closeCreateSupplierModal();
          this.loadDashboardData();
        } else {
          this.toastService.error(res.message || 'Tedarikçi kaydedilemedi.');
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isSupplierSubmitting = false;
        this.toastService.error(err.error?.message || 'Tedarikçi kaydedilirken sunucu hatası oluştu.');
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
