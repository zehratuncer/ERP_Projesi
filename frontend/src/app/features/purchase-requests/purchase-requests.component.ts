import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PurchaseRequestService } from '../../core/services/purchase-request.service';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';
import { 
  PurchaseRequest, 
  PurchaseRequestListItem, 
  RequestPriority, 
  RequestStatus,
  ApprovalAction,
  ApprovalHistoryDto,
  CreatePurchaseRequestItemRequest
} from '../../core/models/purchase-request.model';
import { Product } from '../../core/models/product.model';

interface FormItem {
  productId: string;
  productCode: string;
  productName: string;
  currentStock: number;
  minStockLevel: number;
  requestedQuantity: number;
  unit: string;
  estimatedUnitPrice: number;
  notes?: string;
}

@Component({
  selector: 'app-purchase-requests',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './purchase-requests.component.html',
  styleUrl: './purchase-requests.component.scss'
})
export class PurchaseRequestsComponent implements OnInit {
  private requestService = inject(PurchaseRequestService);
  private productService = inject(ProductService);
  private toastService = inject(ToastService);
  public authService = inject(AuthService);

  // Active View Tab: 'all' (Tüm Talepler) | 'inbox' (Onayımı Bekleyenler) | 'approved' (Mal Kabul Bekleyenler)
  activeTab = signal<'all' | 'inbox' | 'approved'>('inbox');

  // Lists & State
  requests = signal<PurchaseRequestListItem[]>([]);
  products = signal<Product[]>([]);
  isLoading = signal<boolean>(false);

  // Filters
  selectedStatus: RequestStatus | null = null;
  selectedPriority: RequestPriority | null = null;
  selectedDepartment = '';
  searchQuery = '';
  startDate = '';
  endDate = '';

  // Form State (Create / Edit Modal)
  isFormModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  editingRequestId: string | null = null;
  formDepartment = 'Kırtasiye Mağaza';
  formPriority = RequestPriority.Medium;
  formRequiredDate = '';
  formNote = '';
  formItems = signal<FormItem[]>([]);

  // Detail & Audit Timeline Modal
  isDetailModalOpen = signal<boolean>(false);
  selectedRequestDetail = signal<PurchaseRequest | null>(null);

  // Quick Approval Modal
  isApproveModalOpen = signal<boolean>(false);
  approvingRequest = signal<PurchaseRequestListItem | PurchaseRequest | null>(null);
  approvalComment = '';

  // Quick Reject Modal
  isRejectModalOpen = signal<boolean>(false);
  rejectingRequest = signal<PurchaseRequestListItem | PurchaseRequest | null>(null);
  rejectReason = '';

  // Stock Receiving / Convert to Inventory Modal
  isConvertModalOpen = signal<boolean>(false);
  convertingRequest = signal<PurchaseRequestListItem | PurchaseRequest | null>(null);
  convertingDetail = signal<PurchaseRequest | null>(null);
  convertNote = '';

  // Cancel Modal
  isCancelModalOpen = signal<boolean>(false);
  cancelingRequestId: string | null = null;
  cancelingRequestNumber = '';
  cancelReason = '';

  // Threshold constant for multi-level approval
  readonly HighAmountThreshold = 10000;

  RequestStatus = RequestStatus;
  RequestPriority = RequestPriority;
  ApprovalAction = ApprovalAction;

  departments = [
    'Kırtasiye Mağaza',
    'Merkez Depo & Lojistik',
    'Okul & Kurumsal Sevkiyat',
    'İdari İşler & Satın Alma',
    'Muhasebe & Finans'
  ];

  // Filtered requests based on active tab
  displayedRequests = computed(() => {
    const list = this.requests();
    const tab = this.activeTab();

    if (tab === 'inbox') {
      return list.filter(r => r.status === RequestStatus.PendingApproval);
    } else if (tab === 'approved') {
      return list.filter(r => r.status === RequestStatus.Approved);
    }
    return list;
  });

  ngOnInit(): void {
    this.loadRequests();
    this.loadProducts();
  }

  setTab(tab: 'all' | 'inbox' | 'approved'): void {
    this.activeTab.set(tab);
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.requestService.getPurchaseRequests(
      this.selectedStatus || undefined,
      this.selectedDepartment || undefined,
      this.selectedPriority || undefined,
      this.startDate || undefined,
      this.endDate || undefined,
      this.searchQuery || undefined
    ).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.requests.set(res.data);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  loadProducts(callback?: () => void): void {
    this.productService.getProducts(undefined, true).subscribe({
      next: (res) => {
        if (res.isSuccess && res.data) {
          this.products.set(res.data);
          if (callback) callback();
        }
      }
    });
  }

  // ----------------------------------------------------
  // STATS & COMPUTATIONS
  // ----------------------------------------------------
  getTotalCount(): number {
    return this.requests().length;
  }

  getPendingCount(): number {
    return this.requests().filter(r => r.status === RequestStatus.PendingApproval).length;
  }

  getHighValuePendingCount(): number {
    return this.requests().filter(r => r.status === RequestStatus.PendingApproval && r.totalEstimatedAmount > this.HighAmountThreshold).length;
  }

  getApprovedPendingStockCount(): number {
    return this.requests().filter(r => r.status === RequestStatus.Approved).length;
  }

  getCompletedCount(): number {
    return this.requests().filter(r => r.status === RequestStatus.Completed).length;
  }

  getTotalBudget(): number {
    return this.requests().reduce((sum, r) => sum + r.totalEstimatedAmount, 0);
  }

  getPendingTotalBudget(): number {
    return this.requests()
      .filter(r => r.status === RequestStatus.PendingApproval)
      .reduce((sum, r) => sum + r.totalEstimatedAmount, 0);
  }

  isHighValue(amount: number): boolean {
    return amount > this.HighAmountThreshold;
  }

  // ----------------------------------------------------
  // APPROVAL WORKFLOW ACTIONS
  // ----------------------------------------------------
  openApproveModal(reqItem: PurchaseRequestListItem | PurchaseRequest): void {
    this.approvingRequest.set(reqItem);
    this.approvalComment = '';
    this.isApproveModalOpen.set(true);
  }

  closeApproveModal(): void {
    this.isApproveModalOpen.set(false);
    this.approvingRequest.set(null);
  }

  submitApprove(): void {
    const item = this.approvingRequest();
    if (!item) return;

    this.isLoading.set(true);
    this.requestService.approvePurchaseRequest(item.id, this.approvalComment).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess) {
          this.toastService.success(res.message || 'Satın alma talebi onaylandı.');
          this.closeApproveModal();
          this.loadRequests();
          if (this.isDetailModalOpen() && this.selectedRequestDetail()?.id === item.id && res.data) {
            this.selectedRequestDetail.set(res.data);
          }
        } else {
          this.toastService.error(res.message || 'Onaylama işlemi başarısız.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || err.error?.title || 'Onaylanırken hata oluştu.';
        this.toastService.error(msg);
      }
    });
  }

  openRejectModal(reqItem: PurchaseRequestListItem | PurchaseRequest): void {
    this.rejectingRequest.set(reqItem);
    this.rejectReason = '';
    this.isRejectModalOpen.set(true);
  }

  closeRejectModal(): void {
    this.isRejectModalOpen.set(false);
    this.rejectingRequest.set(null);
  }

  submitReject(): void {
    const item = this.rejectingRequest();
    if (!item) return;

    if (!this.rejectReason || this.rejectReason.trim().length < 5) {
      this.toastService.warning('Lütfen en az 5 karakterden oluşan geçerli bir ret gerekçesi giriniz.');
      return;
    }

    this.isLoading.set(true);
    this.requestService.rejectPurchaseRequest(item.id, this.rejectReason).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess) {
          this.toastService.success(res.message || 'Talep reddedildi.');
          this.closeRejectModal();
          this.loadRequests();
          if (this.isDetailModalOpen() && this.selectedRequestDetail()?.id === item.id && res.data) {
            this.selectedRequestDetail.set(res.data);
          }
        } else {
          this.toastService.error(res.message || 'Red işlemi başarısız.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || err.error?.title || 'Reddedilirken hata oluştu.';
        this.toastService.error(msg);
      }
    });
  }

  // ----------------------------------------------------
  // AUTOMATION / INVENTORY RECEIVING ACTIONS
  // ----------------------------------------------------
  openConvertModal(reqItem: PurchaseRequestListItem | PurchaseRequest): void {
    this.convertingRequest.set(reqItem);
    this.convertNote = '';
    this.isLoading.set(true);

    this.requestService.getPurchaseRequestById(reqItem.id).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.convertingDetail.set(res.data);
          this.isConvertModalOpen.set(true);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeConvertModal(): void {
    this.isConvertModalOpen.set(false);
    this.convertingRequest.set(null);
    this.convertingDetail.set(null);
  }

  submitConvertToInventory(): void {
    const item = this.convertingRequest();
    if (!item) return;

    this.isLoading.set(true);
    this.requestService.convertToInventory(item.id, this.convertNote).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess) {
          this.toastService.success(res.message || 'Mal kabul işlemi tamamlandı ve stoklar güncellendi.');
          this.closeConvertModal();
          this.loadRequests();
          if (this.isDetailModalOpen() && this.selectedRequestDetail()?.id === item.id && res.data) {
            this.selectedRequestDetail.set(res.data);
          }
        } else {
          this.toastService.error(res.message || 'Stok giriş işlemi başarısız.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || err.error?.title || 'Stok girişi yapılırken hata oluştu.';
        this.toastService.error(msg);
      }
    });
  }

  // ----------------------------------------------------
  // CREATE / EDIT FORM MODAL
  // ----------------------------------------------------
  openCreateModal(): void {
    this.isEditMode.set(false);
    this.editingRequestId = null;
    this.formDepartment = 'Kırtasiye Mağaza';
    this.formPriority = RequestPriority.Medium;
    this.formRequiredDate = '';
    this.formNote = '';
    this.formItems.set([]);

    if (this.products().length === 0) {
      this.loadProducts(() => {
        this.addNewItemRow();
        this.isFormModalOpen.set(true);
      });
    } else {
      this.addNewItemRow();
      this.isFormModalOpen.set(true);
    }
  }

  openEditModal(reqItem: PurchaseRequestListItem): void {
    if (reqItem.status !== RequestStatus.Draft && reqItem.status !== RequestStatus.PendingApproval) {
      this.toastService.warning('Yalnızca Taslak veya Onay Bekleyen talepler düzenlenebilir.');
      return;
    }

    this.isLoading.set(true);
    this.requestService.getPurchaseRequestById(reqItem.id).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          const data = res.data;
          this.isEditMode.set(true);
          this.editingRequestId = data.id;
          this.formDepartment = data.department;
          this.formPriority = data.priority;
          this.formRequiredDate = data.requiredDate ? data.requiredDate.split('T')[0] : '';
          this.formNote = data.note || '';

          const items: FormItem[] = data.items.map(item => ({
            productId: item.productId,
            productCode: item.productCode,
            productName: item.productName,
            currentStock: item.currentStock,
            minStockLevel: item.minStockLevel,
            requestedQuantity: item.requestedQuantity,
            unit: item.unit,
            estimatedUnitPrice: item.estimatedUnitPrice,
            notes: item.notes
          }));

          this.formItems.set(items);
          this.isFormModalOpen.set(true);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeFormModal(): void {
    this.isFormModalOpen.set(false);
  }

  addNewItemRow(): void {
    const current = [...this.formItems()];
    const defaultProduct = this.products()[0];

    const newItem: FormItem = {
      productId: defaultProduct ? defaultProduct.id : '',
      productCode: defaultProduct ? defaultProduct.code : '',
      productName: defaultProduct ? defaultProduct.name : '',
      currentStock: defaultProduct ? defaultProduct.currentStock : 0,
      minStockLevel: defaultProduct ? defaultProduct.minStockLevel : 0,
      requestedQuantity: 10,
      unit: defaultProduct ? defaultProduct.unit : 'Adet',
      estimatedUnitPrice: defaultProduct ? defaultProduct.unitPrice : 0,
      notes: ''
    };

    this.formItems.set([...current, newItem]);
  }

  onProductSelected(item: FormItem, productId: string): void {
    const prod = this.products().find(p => p.id === productId);
    if (prod) {
      item.productId = prod.id;
      item.productCode = prod.code;
      item.productName = prod.name;
      item.currentStock = prod.currentStock;
      item.minStockLevel = prod.minStockLevel;
      item.unit = prod.unit;
      item.estimatedUnitPrice = prod.unitPrice;
    }
  }

  removeItemRow(index: number): void {
    const current = [...this.formItems()];
    if (current.length === 1) {
      this.toastService.warning('Talebin en az 1 ürün kalemi olmalıdır.');
      return;
    }
    current.splice(index, 1);
    this.formItems.set(current);
  }

  getFormEstimatedTotal(): number {
    return this.formItems().reduce((sum, i) => sum + (Number(i.requestedQuantity || 0) * Number(i.estimatedUnitPrice || 0)), 0);
  }

  saveRequest(submitForApproval: boolean): void {
    if (!this.formDepartment || !this.formDepartment.trim()) {
      this.toastService.warning('Lütfen departman seçiniz.');
      return;
    }

    if (this.formItems().length === 0) {
      this.toastService.warning('Lütfen en az bir ürün kalemi ekleyiniz.');
      return;
    }

    for (const item of this.formItems()) {
      if (!item.productId) {
        this.toastService.warning('Lütfen ürün seçiniz.');
        return;
      }
      if (!item.requestedQuantity || Number(item.requestedQuantity) <= 0) {
        this.toastService.warning('Talep miktarı 0\'dan büyük olmalıdır.');
        return;
      }
    }

    this.isLoading.set(true);

    const itemsPayload: CreatePurchaseRequestItemRequest[] = this.formItems().map(i => ({
      productId: i.productId,
      requestedQuantity: Number(i.requestedQuantity),
      unit: i.unit || 'Adet',
      estimatedUnitPrice: Number(i.estimatedUnitPrice || 0),
      notes: i.notes?.trim() || undefined
    }));

    const parsedDate = this.formRequiredDate ? new Date(this.formRequiredDate).toISOString() : undefined;

    if (this.isEditMode() && this.editingRequestId) {
      const updatePayload = {
        id: this.editingRequestId,
        department: this.formDepartment.trim(),
        priority: Number(this.formPriority),
        requiredDate: parsedDate,
        note: this.formNote?.trim() || undefined,
        items: itemsPayload,
        submitForApproval
      };

      this.requestService.updatePurchaseRequest(this.editingRequestId, updatePayload).subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res.isSuccess) {
            this.toastService.success(res.message || 'Talep başarıyla güncellendi.');
            this.closeFormModal();
            this.loadRequests();
          } else {
            this.toastService.error(res.message || 'Güncelleme başarısız.');
          }
        },
        error: (err) => {
          this.isLoading.set(false);
          const msg = err.error?.message || err.error?.title || 'Talep güncellenirken hata oluştu.';
          this.toastService.error(msg);
        }
      });
    } else {
      const createPayload = {
        department: this.formDepartment.trim(),
        priority: Number(this.formPriority),
        requiredDate: parsedDate,
        note: this.formNote?.trim() || undefined,
        items: itemsPayload,
        submitForApproval
      };

      this.requestService.createPurchaseRequest(createPayload).subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res.isSuccess) {
            this.toastService.success(res.message || 'Talep başarıyla oluşturuldu.');
            this.closeFormModal();
            this.loadRequests();
          } else {
            this.toastService.error(res.message || 'Oluşturma başarısız.');
          }
        },
        error: (err) => {
          this.isLoading.set(false);
          const msg = err.error?.message || err.error?.title || 'Talep oluşturulurken hata oluştu.';
          this.toastService.error(msg);
        }
      });
    }
  }

  // ----------------------------------------------------
  // DETAIL & AUDIT TIMELINE MODAL
  // ----------------------------------------------------
  openDetailModal(reqItem: PurchaseRequestListItem): void {
    this.isLoading.set(true);
    this.requestService.getPurchaseRequestById(reqItem.id).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess && res.data) {
          this.selectedRequestDetail.set(res.data);
          this.isDetailModalOpen.set(true);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  closeDetailModal(): void {
    this.isDetailModalOpen.set(false);
    this.selectedRequestDetail.set(null);
  }

  // ----------------------------------------------------
  // CANCEL MODAL
  // ----------------------------------------------------
  openCancelModal(reqItem: PurchaseRequestListItem): void {
    if (reqItem.status === RequestStatus.Completed || reqItem.status === RequestStatus.Cancelled) {
      this.toastService.warning('Bu talep iptal edilemez.');
      return;
    }

    this.cancelingRequestId = reqItem.id;
    this.cancelingRequestNumber = reqItem.requestNumber;
    this.cancelReason = '';
    this.isCancelModalOpen.set(true);
  }

  closeCancelModal(): void {
    this.isCancelModalOpen.set(false);
    this.cancelingRequestId = null;
  }

  submitCancel(): void {
    if (!this.cancelingRequestId) return;

    this.isLoading.set(true);
    this.requestService.cancelPurchaseRequest(this.cancelingRequestId, this.cancelReason).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess) {
          this.toastService.success(res.message || 'Talep iptal edildi.');
          this.closeCancelModal();
          this.loadRequests();
        } else {
          this.toastService.error(res.message || 'İptal işlemi başarısız.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'İptal edilirken hata oluştu.';
        this.toastService.error(msg);
      }
    });
  }
}
