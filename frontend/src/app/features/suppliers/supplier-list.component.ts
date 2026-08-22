import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupplierService } from '../../core/services/supplier.service';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';
import { Supplier, CreateSupplierRequest, UpdateSupplierRequest } from '../../core/models/supplier.model';
import { Product, UpdateProductRequest } from '../../core/models/product.model';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './supplier-list.component.html',
  styleUrl: './supplier-list.component.scss'
})
export class SupplierListComponent implements OnInit {
  private supplierService = inject(SupplierService);
  private productService = inject(ProductService);
  private toastService = inject(ToastService);

  suppliers: Supplier[] = [];
  filteredSuppliers: Supplier[] = [];
  allProducts: Product[] = [];

  isLoading = false;
  isSubmitting = false;
  searchQuery = '';
  activeTab: 'all' | 'active' | 'inactive' = 'all';

  // Tedarikçi Ekle / Düzenle Modal Durumu
  isSupplierModalOpen = false;
  editingSupplierId: string | null = null;
  supplierForm: {
    name: string;
    contactPerson: string;
    email: string;
    phone: string;
    address: string;
    taxNumber: string;
    isActive: boolean;
  } = {
    name: '',
    contactPerson: '',
    email: '',
    phone: '',
    address: '',
    taxNumber: '',
    isActive: true
  };

  // Tedarikçi Ürünleri Görüntüleme & İlişkilendirme Modal Durumu
  isProductsModalOpen = false;
  selectedSupplierForProducts: Supplier | null = null;
  supplierProducts: Product[] = [];
  isLoadingProducts = false;
  selectedProductIdToAssign: string = '';
  isAssigningProduct = false;

  // Silme Onay Modalı Durumu
  isDeleteModalOpen = false;
  supplierToDelete: Supplier | null = null;

  get totalSuppliersCount(): number {
    return this.suppliers.length;
  }

  get activeSuppliersCount(): number {
    return this.suppliers.filter(s => s.isActive).length;
  }

  get totalSuppliedProductsCount(): number {
    return this.suppliers.reduce((sum, s) => sum + (s.productCount || 0), 0);
  }

  get unassignedOrOtherProducts(): Product[] {
    if (!this.selectedSupplierForProducts) return [];
    return this.allProducts.filter(p => p.supplierId !== this.selectedSupplierForProducts?.id);
  }

  ngOnInit() {
    this.loadSuppliers();
    this.loadAllProducts();
  }

  loadSuppliers() {
    this.isLoading = true;
    this.supplierService.getSuppliers().subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.isSuccess && res.data) {
          this.suppliers = res.data;
        }
        this.applyFilters();
      },
      error: () => {
        this.isLoading = false;
        this.applyFilters();
      }
    });
  }

  loadAllProducts() {
    this.productService.getProducts().subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.allProducts = res.data;
        }
      }
    });
  }

  applyFilters() {
    let list = [...this.suppliers];

    if (this.activeTab === 'active') {
      list = list.filter(s => s.isActive);
    } else if (this.activeTab === 'inactive') {
      list = list.filter(s => !s.isActive);
    }

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase().trim();
      list = list.filter(s =>
        s.name.toLowerCase().includes(q) ||
        (s.contactPerson && s.contactPerson.toLowerCase().includes(q)) ||
        (s.email && s.email.toLowerCase().includes(q)) ||
        (s.phone && s.phone.toLowerCase().includes(q)) ||
        (s.taxNumber && s.taxNumber.toLowerCase().includes(q))
      );
    }

    this.filteredSuppliers = list;
  }

  onSearchChange() {
    this.applyFilters();
  }

  setTab(tab: 'all' | 'active' | 'inactive') {
    this.activeTab = tab;
    this.applyFilters();
  }

  // --- Maskeleme ve Biçimlendirme Yardımcıları ---
  onPhoneInput(event: Event) {
    const input = event.target as HTMLInputElement;
    let digits = input.value.replace(/\D/g, '');
    if (digits.startsWith('90')) {
      digits = digits.substring(2);
    } else if (digits.startsWith('0')) {
      digits = digits.substring(1);
    }
    digits = digits.substring(0, 10);

    let formatted = '';
    if (digits.length > 0) {
      formatted = '+90 (' + digits.substring(0, 3);
      if (digits.length > 3) {
        formatted += ') ' + digits.substring(3, 6);
      }
      if (digits.length > 6) {
        formatted += ' ' + digits.substring(6, 8);
      }
      if (digits.length > 8) {
        formatted += ' ' + digits.substring(8, 10);
      }
    }
    this.supplierForm.phone = formatted;
  }

  onTaxNumberInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').substring(0, 11);
    this.supplierForm.taxNumber = digits;
  }

  // --- Tedarikçi Ekleme / Düzenleme ---
  openCreateModal() {
    this.editingSupplierId = null;
    this.supplierForm = {
      name: '',
      contactPerson: '',
      email: '',
      phone: '',
      address: '',
      taxNumber: '',
      isActive: true
    };
    this.isSupplierModalOpen = true;
  }

  openEditModal(supplier: Supplier) {
    this.editingSupplierId = supplier.id;
    this.supplierForm = {
      name: supplier.name,
      contactPerson: supplier.contactPerson || '',
      email: supplier.email || '',
      phone: supplier.phone || '',
      address: supplier.address || '',
      taxNumber: supplier.taxNumber || '',
      isActive: supplier.isActive
    };
    this.isSupplierModalOpen = true;
  }

  closeSupplierModal() {
    this.isSupplierModalOpen = false;
    this.editingSupplierId = null;
  }

  saveSupplier() {
    if (!this.supplierForm.name.trim()) {
      this.toastService.warning('Lütfen tedarikçi / firma adını giriniz.');
      return;
    }

    this.isSubmitting = true;

    if (this.editingSupplierId) {
      const updateReq: UpdateSupplierRequest = {
        id: this.editingSupplierId,
        name: this.supplierForm.name.trim(),
        contactPerson: this.supplierForm.contactPerson.trim() || undefined,
        email: this.supplierForm.email.trim() || undefined,
        phone: this.supplierForm.phone.trim() || undefined,
        address: this.supplierForm.address.trim() || undefined,
        taxNumber: this.supplierForm.taxNumber.trim() || undefined,
        isActive: this.supplierForm.isActive
      };

      this.supplierService.updateSupplier(this.editingSupplierId, updateReq).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.isSuccess) {
            this.toastService.success('Tedarikçi bilgileri başarıyla güncellendi.');
            this.closeSupplierModal();
            this.loadSuppliers();
            this.loadAllProducts();
          }
        },
        error: () => {
          this.isSubmitting = false;
        }
      });
    } else {
      const createReq: CreateSupplierRequest = {
        name: this.supplierForm.name.trim(),
        contactPerson: this.supplierForm.contactPerson.trim() || undefined,
        email: this.supplierForm.email.trim() || undefined,
        phone: this.supplierForm.phone.trim() || undefined,
        address: this.supplierForm.address.trim() || undefined,
        taxNumber: this.supplierForm.taxNumber.trim() || undefined
      };

      this.supplierService.createSupplier(createReq).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.isSuccess) {
            this.toastService.success('Yeni tedarikçi başarıyla eklendi.');
            this.closeSupplierModal();
            this.loadSuppliers();
            this.loadAllProducts();
          }
        },
        error: () => {
          this.isSubmitting = false;
        }
      });
    }
  }

  // --- Silme İşlemleri ---
  openDeleteModal(supplier: Supplier) {
    this.supplierToDelete = supplier;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.supplierToDelete = null;
  }

  confirmDelete() {
    if (!this.supplierToDelete) return;

    this.isSubmitting = true;
    this.supplierService.deleteSupplier(this.supplierToDelete.id).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.isSuccess) {
          this.toastService.success('Tedarikçi başarıyla silindi.');
          this.closeDeleteModal();
          this.loadSuppliers();
          this.loadAllProducts();
        }
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }

  // --- Tedarikçi Ürünleri Modalı ---
  openProductsModal(supplier: Supplier) {
    this.selectedSupplierForProducts = supplier;
    this.selectedProductIdToAssign = '';
    this.isProductsModalOpen = true;
    this.loadSupplierProducts(supplier.id);
  }

  closeProductsModal() {
    this.isProductsModalOpen = false;
    this.selectedSupplierForProducts = null;
    this.supplierProducts = [];
    this.selectedProductIdToAssign = '';
  }

  loadSupplierProducts(supplierId: string) {
    this.isLoadingProducts = true;
    this.supplierService.getSupplierProducts(supplierId).subscribe({
      next: (res) => {
        this.isLoadingProducts = false;
        if (res.isSuccess && res.data) {
          this.supplierProducts = res.data;
        }
      },
      error: () => {
        this.isLoadingProducts = false;
      }
    });
  }

  assignProduct() {
    if (!this.selectedProductIdToAssign || !this.selectedSupplierForProducts) {
      this.toastService.warning('Lütfen ilişkilendirilecek bir ürün seçiniz.');
      return;
    }

    const prod = this.allProducts.find(p => p.id === this.selectedProductIdToAssign);
    if (!prod) return;

    this.isAssigningProduct = true;
    const updateReq: UpdateProductRequest = {
      id: prod.id,
      name: prod.name,
      description: prod.description,
      unit: prod.unit,
      minStockLevel: prod.minStockLevel,
      unitPrice: prod.unitPrice,
      isActive: prod.isActive,
      supplierId: this.selectedSupplierForProducts.id
    };

    this.productService.updateProduct(prod.id, updateReq).subscribe({
      next: (res) => {
        this.isAssigningProduct = false;
        if (res.isSuccess) {
          this.toastService.success(`'${prod.name}' ürünü '${this.selectedSupplierForProducts?.name}' ile ilişkilendirildi.`);
          this.selectedProductIdToAssign = '';
          this.loadSupplierProducts(this.selectedSupplierForProducts!.id);
          this.loadSuppliers();
          this.loadAllProducts();
        }
      },
      error: () => {
        this.isAssigningProduct = false;
      }
    });
  }

  unassignProduct(product: Product) {
    if (!this.selectedSupplierForProducts) return;

    const updateReq: UpdateProductRequest = {
      id: product.id,
      name: product.name,
      description: product.description,
      unit: product.unit,
      minStockLevel: product.minStockLevel,
      unitPrice: product.unitPrice,
      isActive: product.isActive,
      supplierId: undefined
    };

    this.productService.updateProduct(product.id, updateReq).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.toastService.info(`'${product.name}' ürününün tedarikçi bağı kaldırıldı.`);
          this.loadSupplierProducts(this.selectedSupplierForProducts!.id);
          this.loadSuppliers();
          this.loadAllProducts();
        }
      }
    });
  }
}
