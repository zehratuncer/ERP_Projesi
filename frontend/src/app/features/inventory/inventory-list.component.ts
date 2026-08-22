import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { InventoryService } from '../../core/services/inventory.service';
import { SupplierService } from '../../core/services/supplier.service';
import { ToastService } from '../../core/services/toast.service';
import { Product, CreateProductRequest, UpdateProductRequest } from '../../core/models/product.model';
import { StockMovement, CreateStockMovementRequest, TransactionType } from '../../core/models/inventory.model';
import { Supplier } from '../../core/models/supplier.model';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory-list.component.html',
  styleUrl: './inventory-list.component.scss'
})
export class InventoryListComponent implements OnInit {
  private productService = inject(ProductService);
  private inventoryService = inject(InventoryService);
  private supplierService = inject(SupplierService);
  private toastService = inject(ToastService);

  products: Product[] = [];
  filteredProducts: Product[] = [];
  movements: StockMovement[] = [];
  suppliers: Supplier[] = [];

  isLoading = false;
  isSubmitting = false;
  searchQuery = '';
  activeTab: 'all' | 'critical' = 'all';

  // Ürün Ekle / Düzenle Modal Durumu
  isProductModalOpen = false;
  editingProductId: string | null = null;
  productForm: CreateProductRequest = {
    code: '',
    name: '',
    description: '',
    unit: 'Adet',
    initialStock: 0,
    minStockLevel: 10,
    unitPrice: 0,
    supplierId: undefined
  };

  // Stok Hareketi Modal Durumu
  isMovementModalOpen = false;
  selectedProductForMovement: Product | null = null;
  movementForm: CreateStockMovementRequest = {
    productId: '',
    quantity: 1,
    transactionType: TransactionType.In,
    description: ''
  };

  // Hareket Geçmişi Modal Durumu
  isHistoryModalOpen = false;

  get criticalStockCount(): number {
    return this.products.filter(p => p.isLowStock || p.currentStock <= p.minStockLevel).length;
  }

  get totalStockQuantity(): number {
    return this.products.reduce((acc, p) => acc + p.currentStock, 0);
  }

  get resultingStock(): number {
    if (!this.selectedProductForMovement) return 0;
    const current = this.selectedProductForMovement.currentStock;
    const qty = this.movementForm.quantity || 0;

    if (this.movementForm.transactionType === TransactionType.In) {
      return current + qty;
    } else if (this.movementForm.transactionType === TransactionType.Out) {
      return current - qty;
    } else if (this.movementForm.transactionType === TransactionType.Adjustment) {
      return qty;
    }
    return current;
  }

  ngOnInit() {
    this.loadProducts();
    this.loadSuppliers();
  }

  loadProducts() {
    this.isLoading = true;
    this.productService.getProducts().subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.isSuccess && res.data) {
          this.products = res.data;
        }
        this.applyFilters();
      },
      error: () => {
        this.isLoading = false;
        this.applyFilters();
      }
    });
  }

  loadSuppliers() {
    this.supplierService.getSuppliers(undefined, true).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.suppliers = res.data;
        }
      }
    });
  }

  applyFilters() {
    let list = [...this.products];

    if (this.activeTab === 'critical') {
      list = list.filter(p => p.isLowStock || p.currentStock <= p.minStockLevel);
    }

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase().trim();
      list = list.filter(p =>
        p.code.toLowerCase().includes(q) ||
        p.name.toLowerCase().includes(q) ||
        (p.supplierName && p.supplierName.toLowerCase().includes(q))
      );
    }

    this.filteredProducts = list;
  }

  onSearchChange() {
    this.applyFilters();
  }

  setTab(tab: 'all' | 'critical') {
    this.activeTab = tab;
    this.applyFilters();
  }

  // --- Ürün Ekle / Düzenle İşlemleri ---
  openCreateModal() {
    this.editingProductId = null;
    this.productForm = {
      code: '',
      name: '',
      description: '',
      unit: 'Adet',
      initialStock: 0,
      minStockLevel: 10,
      unitPrice: 0,
      supplierId: undefined
    };
    this.isProductModalOpen = true;
  }

  openEditModal(product: Product) {
    this.editingProductId = product.id;
    this.productForm = {
      code: product.code,
      name: product.name,
      description: product.description || '',
      unit: product.unit,
      initialStock: product.currentStock,
      minStockLevel: product.minStockLevel,
      unitPrice: product.unitPrice,
      supplierId: product.supplierId
    };
    this.isProductModalOpen = true;
  }

  closeProductModal() {
    this.isProductModalOpen = false;
    this.editingProductId = null;
  }

  saveProduct() {
    if (!this.productForm.name.trim() || !this.productForm.code.trim()) {
      this.toastService.warning('Lütfen zorunlu alanları doldurunuz.');
      return;
    }

    this.isSubmitting = true;

    if (this.editingProductId) {
      const updateReq: UpdateProductRequest = {
        id: this.editingProductId,
        name: this.productForm.name,
        description: this.productForm.description,
        unit: this.productForm.unit,
        minStockLevel: this.productForm.minStockLevel,
        unitPrice: this.productForm.unitPrice,
        isActive: true,
        supplierId: this.productForm.supplierId || undefined
      };

      this.productService.updateProduct(this.editingProductId, updateReq).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.isSuccess) {
            this.toastService.success('Ürün bilgileri güncellendi.');
            this.closeProductModal();
            this.loadProducts();
          }
        },
        error: () => { this.isSubmitting = false; }
      });
    } else {
      this.productService.createProduct(this.productForm).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.isSuccess) {
            this.toastService.success('Yeni ürün başarıyla eklendi.');
            this.closeProductModal();
            this.loadProducts();
          }
        },
        error: () => { this.isSubmitting = false; }
      });
    }
  }

  deleteProduct(product: Product) {
    if (confirm(`"${product.name}" (${product.code}) ürününü silmek istediğinize emin misiniz?`)) {
      this.productService.deleteProduct(product.id).subscribe({
        next: (res) => {
          if (res.isSuccess) {
            this.toastService.success('Ürün başarıyla silindi.');
            this.loadProducts();
          }
        }
      });
    }
  }

  // --- Stok Hareketi İşlemleri ---
  openMovementModal(product: Product) {
    this.selectedProductForMovement = product;
    this.movementForm = {
      productId: product.id,
      quantity: 1,
      transactionType: TransactionType.In,
      description: ''
    };
    this.isMovementModalOpen = true;
  }

  closeMovementModal() {
    this.isMovementModalOpen = false;
    this.selectedProductForMovement = null;
  }

  saveStockMovement() {
    if (this.resultingStock < 0) {
      this.toastService.error('Yetersiz stok! Mevcut miktardan fazla çıkış yapılamaz.');
      return;
    }

    this.isSubmitting = true;
    this.inventoryService.createStockMovement(this.movementForm).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.isSuccess) {
          this.toastService.success(res.message || 'Stok hareketi işlendi.');
          this.closeMovementModal();
          this.loadProducts();
        }
      },
      error: () => { this.isSubmitting = false; }
    });
  }

  // --- Hareket Geçmişi Modal ---
  openMovementsModal() {
    this.isHistoryModalOpen = true;
    this.inventoryService.getStockMovements().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.movements = res.data;
        }
      }
    });
  }

  closeMovementsModal() {
    this.isHistoryModalOpen = false;
  }
}
