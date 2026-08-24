import { Component, ElementRef, HostListener, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PosService } from '../../core/services/pos.service';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';
import { 
  CartItem, 
  PosProduct, 
  PaymentMethod, 
  SaleReceipt, 
  DailyPosSummary, 
  SaleHistoryItem 
} from '../../core/models/pos.model';
import { Product } from '../../core/models/product.model';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.scss'
})
export class PosComponent implements OnInit {
  private posService = inject(PosService);
  private productService = inject(ProductService);
  private toastService = inject(ToastService);

  @ViewChild('barcodeInput') barcodeInputRef!: ElementRef<HTMLInputElement>;
  @ViewChild('receivedAmountInput') receivedAmountInputRef!: ElementRef<HTMLInputElement>;

  // Tabs
  activeTab = signal<'pos' | 'summary' | 'history'>('pos');

  // POS State
  barcodeQuery = '';
  cart = signal<CartItem[]>([]);
  selectedPaymentMethod = signal<PaymentMethod>(PaymentMethod.Cash);
  customerName = '';
  generalDiscount = 0;
  receivedAmount = 0;
  isLoading = signal<boolean>(false);

  // Quick Product Search Modal
  isProductSearchOpen = signal<boolean>(false);
  productSearchQuery = '';
  availableProducts = signal<Product[]>([]);
  isProductsLoading = signal<boolean>(false);

  // Payment / Complete Sale Modal
  isPaymentModalOpen = signal<boolean>(false);

  // Receipt Modal
  isReceiptModalOpen = signal<boolean>(false);
  currentReceipt = signal<SaleReceipt | null>(null);

  // Daily Summary State
  dailySummary = signal<DailyPosSummary | null>(null);
  selectedSummaryDate = new Date().toISOString().split('T')[0];

  // History State
  salesHistory = signal<SaleHistoryItem[]>([]);
  historyStartDate = '';
  historyEndDate = '';
  historyPaymentMethod: PaymentMethod | null = null;
  historySearch = '';

  PaymentMethod = PaymentMethod;

  ngOnInit(): void {
    this.focusBarcodeInput();
  }

  // Keyboard Shortcuts (F2: Satış/Ödeme, F4: Sepeti Temizle, Esc: Modal Kapat)
  @HostListener('window:keydown', ['$event'])
  handleKeyboardShortcuts(event: KeyboardEvent) {
    if (event.key === 'F2') {
      event.preventDefault();
      if (this.cart().length > 0 && !this.isReceiptModalOpen()) {
        this.openPaymentModal();
      }
    } else if (event.key === 'F4') {
      event.preventDefault();
      if (this.cart().length > 0) {
        this.clearCart();
      }
    } else if (event.key === 'Escape') {
      if (this.isReceiptModalOpen()) {
        this.closeReceiptModal();
      } else if (this.isPaymentModalOpen()) {
        this.closePaymentModal();
      } else if (this.isProductSearchOpen()) {
        this.closeProductSearchModal();
      }
    }
  }

  focusBarcodeInput(): void {
    setTimeout(() => {
      if (this.barcodeInputRef && this.barcodeInputRef.nativeElement) {
        this.barcodeInputRef.nativeElement.focus();
      }
    }, 100);
  }

  // ----------------------------------------------------
  // BARCODE & CART OPERATIONS
  // ----------------------------------------------------
  onBarcodeScan(): void {
    const code = this.barcodeQuery.trim();
    if (!code) return;

    this.isLoading.set(true);
    this.posService.getProductByBarcode(code).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.addProductToCart(res.data);
          this.barcodeQuery = '';
          this.focusBarcodeInput();
        } else {
          this.toastService.error(res.message || 'Ürün bulunamadı.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const errMsg = err.error?.message || `"${code}" kodlu ürün bulunamadı.`;
        this.toastService.error(errMsg);
        this.barcodeQuery = '';
        this.focusBarcodeInput();
      }
    });
  }

  addProductToCart(product: PosProduct, quantityToAdd = 1): void {
    if (product.currentStock <= 0) {
      this.toastService.warning(`"${product.name}" için stok tükenmiş!`);
    }

    const currentCart = [...this.cart()];
    const existingIndex = currentCart.findIndex(item => item.product.id === product.id);

    if (existingIndex > -1) {
      const item = currentCart[existingIndex];
      const newQty = item.quantity + quantityToAdd;

      if (newQty > product.currentStock) {
        this.toastService.warning(`Stok yetersiz! Mevcut stok: ${product.currentStock} ${product.unit}`);
      }

      item.quantity = newQty;
      item.totalPrice = this.calculateItemTotal(item);
      this.cart.set(currentCart);
      this.toastService.info(`"${product.name}" adedi ${newQty} olarak güncellendi.`);
    } else {
      const newItem: CartItem = {
        product,
        quantity: quantityToAdd,
        unitPrice: product.unitPrice,
        discountRate: 0,
        totalPrice: product.unitPrice * quantityToAdd
      };
      this.cart.set([newItem, ...currentCart]);
      this.toastService.success(`"${product.name}" sepete eklendi.`);
    }
  }

  updateQuantity(item: CartItem, delta: number): void {
    const newQty = item.quantity + delta;
    if (newQty <= 0) {
      this.removeItem(item);
      return;
    }

    if (newQty > item.product.currentStock) {
      this.toastService.warning(`Stok uyarısı: Mevcut stok ${item.product.currentStock} ${item.product.unit}`);
    }

    item.quantity = newQty;
    item.totalPrice = this.calculateItemTotal(item);
    this.cart.set([...this.cart()]);
  }

  onQuantityChange(item: CartItem): void {
    if (!item.quantity || item.quantity <= 0) {
      item.quantity = 1;
    }
    item.totalPrice = this.calculateItemTotal(item);
    this.cart.set([...this.cart()]);
  }

  onDiscountChange(item: CartItem): void {
    if (item.discountRate < 0) item.discountRate = 0;
    if (item.discountRate > 100) item.discountRate = 100;
    item.totalPrice = this.calculateItemTotal(item);
    this.cart.set([...this.cart()]);
  }

  calculateItemTotal(item: CartItem): number {
    const gross = item.unitPrice * item.quantity;
    const discount = (gross * (item.discountRate || 0)) / 100;
    return Math.max(0, gross - discount);
  }

  removeItem(item: CartItem): void {
    const updated = this.cart().filter(i => i.product.id !== item.product.id);
    this.cart.set(updated);
    this.toastService.info(`"${item.product.name}" sepetten çıkarıldı.`);
    this.focusBarcodeInput();
  }

  clearCart(): void {
    if (confirm('Sepetteki tüm ürünleri silmek istediğinizden emin misiniz?')) {
      this.cart.set([]);
      this.generalDiscount = 0;
      this.receivedAmount = 0;
      this.customerName = '';
      this.toastService.info('Sepet temizlendi.');
      this.focusBarcodeInput();
    }
  }

  // ----------------------------------------------------
  // TOTALS & CALCULATIONS
  // ----------------------------------------------------
  getSubTotal(): number {
    return this.cart().reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
  }

  getItemDiscountsTotal(): number {
    return this.cart().reduce((sum, item) => {
      const gross = item.unitPrice * item.quantity;
      return sum + (gross * (item.discountRate || 0)) / 100;
    }, 0);
  }

  getFinalTotal(): number {
    const sub = this.getSubTotal();
    const discounts = this.getItemDiscountsTotal() + (this.generalDiscount || 0);
    return Math.max(0, sub - discounts);
  }

  getChangeDue(): number {
    if (this.selectedPaymentMethod() !== PaymentMethod.Cash) return 0;
    const final = this.getFinalTotal();
    return Math.max(0, (this.receivedAmount || 0) - final);
  }

  setPresetReceived(amount: number): void {
    this.receivedAmount = amount;
  }

  setExactAmount(): void {
    this.receivedAmount = this.getFinalTotal();
  }

  // ----------------------------------------------------
  // PAYMENT & COMPLETE SALE
  // ----------------------------------------------------
  openPaymentModal(): void {
    if (this.cart().length === 0) {
      this.toastService.warning('Sepette ürün yok!');
      return;
    }
    this.receivedAmount = this.getFinalTotal();
    this.isPaymentModalOpen.set(true);
    setTimeout(() => {
      if (this.receivedAmountInputRef?.nativeElement) {
        this.receivedAmountInputRef.nativeElement.focus();
        this.receivedAmountInputRef.nativeElement.select();
      }
    }, 100);
  }

  closePaymentModal(): void {
    this.isPaymentModalOpen.set(false);
    this.focusBarcodeInput();
  }

  selectPaymentMethod(method: PaymentMethod): void {
    this.selectedPaymentMethod.set(method);
  }

  submitSale(): void {
    if (this.cart().length === 0) return;

    if (this.selectedPaymentMethod() === PaymentMethod.Cash && this.receivedAmount < this.getFinalTotal()) {
      this.toastService.warning('Alınan nakit tutar toplam tutardan az olamaz!');
      return;
    }

    this.isLoading.set(true);

    const request = {
      items: this.cart().map(item => ({
        productId: item.product.id,
        quantity: item.quantity,
        customUnitPrice: item.unitPrice,
        discountRate: item.discountRate
      })),
      paymentMethod: this.selectedPaymentMethod(),
      customerName: this.customerName.trim() || 'Perakende Müşteri',
      generalDiscountAmount: this.generalDiscount || 0
    };

    this.posService.completeSale(request).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.toastService.success(res.message || 'Satış başarıyla tamamlandı!');
          this.currentReceipt.set(res.data);
          this.isPaymentModalOpen.set(false);
          this.isReceiptModalOpen.set(true);
          
          // Clear cart for next sale
          this.cart.set([]);
          this.generalDiscount = 0;
          this.customerName = '';
        } else {
          this.toastService.error(res.message || 'Satış tamamlanamadı.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.toastService.error(err.error?.message || 'Satış sırasında hata oluştu.');
      }
    });
  }

  // ----------------------------------------------------
  // RECEIPT MODAL & PRINT
  // ----------------------------------------------------
  closeReceiptModal(): void {
    this.isReceiptModalOpen.set(false);
    this.currentReceipt.set(null);
    this.focusBarcodeInput();
  }

  printReceipt(): void {
    window.print();
  }

  // ----------------------------------------------------
  // QUICK PRODUCT SEARCH MODAL
  // ----------------------------------------------------
  openProductSearchModal(): void {
    this.isProductSearchOpen.set(true);
    this.productSearchQuery = '';
    this.loadAvailableProducts();
  }

  closeProductSearchModal(): void {
    this.isProductSearchOpen.set(false);
    this.focusBarcodeInput();
  }

  loadAvailableProducts(): void {
    this.isProductsLoading.set(true);
    this.productService.getProducts(this.productSearchQuery, true).subscribe({
      next: (res) => {
        this.isProductsLoading.set(false);
        if (res.isSuccess && res.data) {
          this.availableProducts.set(res.data);
        }
      },
      error: () => {
        this.isProductsLoading.set(false);
        this.toastService.error('Ürünler yüklenirken hata oluştu.');
      }
    });
  }

  onProductSearchKeyUp(): void {
    this.loadAvailableProducts();
  }

  selectProductFromSearch(p: Product): void {
    const posProd: PosProduct = {
      id: p.id,
      code: p.code,
      name: p.name,
      description: p.description,
      unit: p.unit,
      currentStock: p.currentStock,
      minStockLevel: p.minStockLevel,
      unitPrice: p.unitPrice,
      isLowStock: p.currentStock <= p.minStockLevel,
      isActive: p.isActive,
      supplierName: p.supplierName
    };
    this.addProductToCart(posProd);
    this.closeProductSearchModal();
  }

  // ----------------------------------------------------
  // TAB NAVIGATION & REPORTS
  // ----------------------------------------------------
  switchTab(tab: 'pos' | 'summary' | 'history'): void {
    this.activeTab.set(tab);
    if (tab === 'summary') {
      this.loadDailySummary();
    } else if (tab === 'history') {
      this.loadSalesHistory();
    } else {
      this.focusBarcodeInput();
    }
  }

  loadDailySummary(): void {
    this.isLoading.set(true);
    this.posService.getDailySummary(this.selectedSummaryDate).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.dailySummary.set(res.data);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.error('Günlük özet alınamadı.');
      }
    });
  }

  loadSalesHistory(): void {
    this.isLoading.set(true);
    this.posService.getSalesHistory(
      this.historyStartDate || undefined,
      this.historyEndDate || undefined,
      this.historyPaymentMethod || undefined,
      this.historySearch || undefined
    ).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.salesHistory.set(res.data);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.error('Satış geçmişi alınamadı.');
      }
    });
  }

  viewPastReceipt(receiptNumber: string): void {
    this.isLoading.set(true);
    this.posService.getReceipt(receiptNumber).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.currentReceipt.set(res.data);
          this.isReceiptModalOpen.set(true);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.error('Fiş detayı getirilemedi.');
      }
    });
  }
}
