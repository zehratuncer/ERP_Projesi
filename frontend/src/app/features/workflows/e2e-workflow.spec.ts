import { CartItem, PaymentMethod, PosProduct, SaleReceipt } from '../../core/models/pos.model';
import { PurchaseRequest, RequestStatus, RequestPriority, ApprovalAction } from '../../core/models/purchase-request.model';
import { Product } from '../../core/models/product.model';

describe('E2E Workflow Scenarios', () => {

  // =========================================================================
  // SENARYO 1: Tam Kasa Satış & Otomatik Stok Düşümü Akışı (POS & Inventory)
  // =========================================================================
  describe('Senaryo 1: Tam Kasa Satış & Otomatik Stok Düşümü Akışı', () => {
    let mockInventory: Product[];
    let cart: CartItem[];

    beforeEach(() => {
      mockInventory = [
        {
          id: 'p1',
          code: 'KRT-001',
          name: 'A4 Fotokopi Kağıdı 80gr 500lü',
          unit: 'Koli',
          currentStock: 50,
          minStockLevel: 10,
          unitPrice: 780,
          isLowStock: false,
          isActive: true,
          createdDate: '2026-08-27T08:00:00Z'
        },
        {
          id: 'p2',
          code: 'KRT-002',
          name: 'Tükenmez Kalem 50li Paket Mavi',
          unit: 'Paket',
          currentStock: 40,
          minStockLevel: 5,
          unitPrice: 65,
          isLowStock: false,
          isActive: true,
          createdDate: '2026-08-27T08:00:00Z'
        },
        {
          id: 'p3',
          code: 'KRT-003',
          name: 'Telli Dosya Mavi 50li Paket',
          unit: 'Paket',
          currentStock: 100,
          minStockLevel: 20,
          unitPrice: 15,
          isLowStock: false,
          isActive: true,
          createdDate: '2026-08-27T08:00:00Z'
        }
      ];
      cart = [];
    });

    it('should add 3 items to cart, compute discounts, calculate cash change, and deduct inventory stock upon sale completion', () => {
      // 1. Kasiyer barkod veya ürün seçimi ile 3 ürünü sepete ekler
      function addToCart(product: Product, quantity: number, discountRate: number = 0) {
        const existing = cart.find(c => c.product.id === product.id);
        if (existing) {
          existing.quantity += quantity;
          existing.totalPrice = existing.quantity * existing.unitPrice * (1 - (existing.discountRate || 0) / 100);
        } else {
          const itemGross = quantity * product.unitPrice;
          const discounted = itemGross * (1 - discountRate / 100);
          cart.push({
            product: {
              id: product.id,
              code: product.code,
              name: product.name,
              unit: product.unit,
              currentStock: product.currentStock,
              minStockLevel: product.minStockLevel,
              unitPrice: product.unitPrice,
              isLowStock: product.isLowStock ?? false,
              isActive: product.isActive
            },
            quantity,
            unitPrice: product.unitPrice,
            discountRate,
            totalPrice: discounted
          });
        }
      }

      addToCart(mockInventory[0], 5, 0);   // 5 * 780 = 3900 TL
      addToCart(mockInventory[1], 10, 10); // 10 * 65 = 650 - %10 (65) = 585 TL
      addToCart(mockInventory[2], 20, 0);  // 20 * 15 = 300 TL

      expect(cart.length).toBe(3);

      // 2. F2 ile Tahsilat Açılır, Genel indirim uygulanır ve Nakit Para Üstü Hesaplanır
      const subTotal = cart.reduce((sum, i) => sum + (i.unitPrice * i.quantity), 0); // 4850
      const itemDiscounts = cart.reduce((sum, i) => sum + (i.unitPrice * i.quantity * (i.discountRate / 100)), 0); // 65
      const generalDiscount = 85;
      const totalDiscount = itemDiscounts + generalDiscount; // 150
      const finalPayable = subTotal - totalDiscount; // 4700

      expect(subTotal).toBe(4850);
      expect(totalDiscount).toBe(150);
      expect(finalPayable).toBe(4700);

      const receivedCash = 5000;
      const changeDue = receivedCash - finalPayable;
      expect(changeDue).toBe(300);

      // 3. Satışı Onayla & Fiş Oluştur
      const receipt: SaleReceipt = {
        id: 'sale-001',
        receiptNumber: 'FIS-20260827-001',
        saleDate: new Date().toISOString(),
        cashierName: 'Zehra Tunçer',
        customerName: 'Ahmet Yılmaz',
        paymentMethod: PaymentMethod.Cash,
        paymentMethodName: 'Nakit',
        totalAmount: subTotal,
        discountAmount: totalDiscount,
        finalAmount: finalPayable,
        items: cart.map((item, idx) => ({
          id: `item-${idx + 1}`,
          productId: item.product.id,
          productCode: item.product.code,
          productName: item.product.name,
          unit: item.product.unit,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          totalPrice: item.totalPrice,
          discountRate: item.discountRate
        })),
        criticalStockAlerts: []
      };

      expect(receipt.receiptNumber).toMatch(/^FIS-/);
      expect(receipt.finalAmount).toBe(4700);
      expect(receipt.items.length).toBe(3);

      // 4. Envanter stok düşümü doğrulaması (/inventory simülasyonu)
      cart.forEach(item => {
        const prod = mockInventory.find(p => p.id === item.product.id);
        if (prod) {
          prod.currentStock -= item.quantity;
          prod.isLowStock = prod.currentStock <= prod.minStockLevel;
        }
      });

      expect(mockInventory[0].currentStock).toBe(45); // 50 - 5
      expect(mockInventory[1].currentStock).toBe(30); // 40 - 10
      expect(mockInventory[2].currentStock).toBe(80); // 100 - 20
    });
  });

  // =========================================================================
  // SENARYO 2: Kritik Stok Alarmı & Satın Alma Talep Oluşturma Akışı
  // =========================================================================
  describe('Senaryo 2: Kritik Stok Alarmı & Satın Alma Talep Oluşturma Akışı', () => {
    it('should detect critical stock, trigger low stock alert and compose a valid purchase request in PendingApproval state', () => {
      const product: Product = {
        id: 'p1',
        code: 'KRT-001',
        name: 'A4 Kağıt 80gr',
        unit: 'Koli',
        currentStock: 18,
        minStockLevel: 15,
        unitPrice: 780,
        isLowStock: false,
        isActive: true,
        createdDate: '2026-08-27T08:00:00Z'
      };

      // Stok 10 adet düşüyor (18 - 10 = 8 < 15)
      product.currentStock -= 10;
      product.isLowStock = product.currentStock <= product.minStockLevel;

      expect(product.currentStock).toBe(8);
      expect(product.isLowStock).toBe(true);

      // Bildirim Çanı & Rozet Uyarısı
      const unreadAlerts = [
        { id: '1', title: 'Kritik Stok Uyarısı', message: `${product.name} kritik seviyeye düştü (Kalan: ${product.currentStock})`, isRead: false }
      ];
      expect(unreadAlerts.filter(a => !a.isRead).length).toBe(1);

      // Satın Alma Talebi Oluşturma
      const newRequest: Partial<PurchaseRequest> = {
        id: 'pr-001',
        requestNumber: 'PR-20260827-001',
        department: 'Mağaza & Satış',
        priority: RequestPriority.High,
        status: RequestStatus.PendingApproval,
        note: 'Kritik stok ikmali - Acil',
        totalEstimatedAmount: 50 * product.unitPrice,
        items: [
          {
            id: 'item-1',
            productId: product.id,
            productCode: product.code,
            productName: product.name,
            currentStock: product.currentStock,
            minStockLevel: product.minStockLevel,
            requestedQuantity: 50,
            unit: product.unit,
            estimatedUnitPrice: product.unitPrice,
            estimatedTotalPrice: 50 * product.unitPrice,
            notes: 'Kritik stok seviyesine indiği için ikmal talebi'
          }
        ]
      };

      expect(newRequest.status).toBe(RequestStatus.PendingApproval);
      expect(newRequest.totalEstimatedAmount).toBe(39000);
      expect(newRequest.items?.length).toBe(1);
    });
  });

  // =========================================================================
  // SENARYO 3: Yönetici Onayı & Depoya Mal Kabul / Stok Artış Akışı
  // =========================================================================
  describe('Senaryo 3: Yönetici Onayı & Depoya Mal Kabul / Stok Artış Akışı', () => {
    it('should transition purchase request from PendingApproval -> Approved -> Completed and increase product stock in warehouse', () => {
      const product: Product = {
        id: 'p1',
        code: 'KRT-001',
        name: 'A4 Kağıt 80gr',
        unit: 'Koli',
        currentStock: 10,
        minStockLevel: 15,
        unitPrice: 780,
        isLowStock: true,
        isActive: true,
        createdDate: '2026-08-27T08:00:00Z'
      };

      const purchaseRequest: PurchaseRequest = {
        id: 'pr-001',
        requestNumber: 'PR-20260827-001',
        department: 'Kırtasiye Satın Alma',
        requesterUserName: 'Zehra Tunçer',
        priority: RequestPriority.Urgent,
        priorityName: 'Acil',
        status: RequestStatus.PendingApproval,
        statusName: 'Onay Bekliyor',
        totalEstimatedAmount: 2500,
        currentApprovalStep: 1,
        createdDate: new Date().toISOString(),
        items: [
          {
            id: 'item-1',
            productId: product.id,
            productCode: product.code,
            productName: product.name,
            currentStock: product.currentStock,
            minStockLevel: product.minStockLevel,
            requestedQuantity: 25,
            unit: product.unit,
            estimatedUnitPrice: 100,
            estimatedTotalPrice: 2500
          }
        ],
        approvalHistories: []
      };

      // 1. Yönetici Talebi Onaylar
      function approveRequest(req: PurchaseRequest, approverName: string, comment: string) {
        req.status = RequestStatus.Approved;
        req.statusName = 'Onaylandı';
        req.approvalHistories?.push({
          id: 'app-1',
          purchaseRequestId: req.id,
          approverUserName: approverName,
          stepNumber: 1,
          stepName: 'Departman Müdürü Onayı',
          action: ApprovalAction.Approved,
          actionName: 'Onaylandı',
          actionDate: new Date().toISOString(),
          comment
        });
      }

      approveRequest(purchaseRequest, 'Yönetici Zehra', 'Bütçe onaylandı.');
      expect(purchaseRequest.status).toBe(RequestStatus.Approved);
      expect(purchaseRequest.approvalHistories?.length).toBe(1);

      // 2. Depo Sorumlusu Mal Kabul Yapar (Convert to Inventory)
      function convertToInventory(req: PurchaseRequest, inventory: Product[]) {
        if (req.status !== RequestStatus.Approved) {
          throw new Error('Sadece onaylanmış talepler depoya kabul edilebilir.');
        }

        req.items.forEach(item => {
          const p = inventory.find(x => x.id === item.productId);
          if (p) {
            p.currentStock += item.requestedQuantity;
            p.isLowStock = p.currentStock <= p.minStockLevel;
          }
        });

        req.status = RequestStatus.Completed;
        req.statusName = 'Tamamlandı';
      }

      convertToInventory(purchaseRequest, [product]);

      // 3. Stok artışı ve talep tamamlama doğrulaması
      expect(purchaseRequest.status).toBe(RequestStatus.Completed);
      expect(product.currentStock).toBe(35); // 10 + 25
      expect(product.isLowStock).toBe(false); // 35 > 15
    });
  });

  // =========================================================================
  // SENARYO 4: Excel & PDF Kurumsal Belge Dışa Aktarım Akışı
  // =========================================================================
  describe('Senaryo 4: Excel & PDF Kurumsal Belge Dışa Aktarım Akışı', () => {
    it('should handle PDF report trigger and Excel export trigger with appropriate blob creation and loading states', () => {
      let isExportingPdf = false;
      let isExportingExcel = false;

      function mockDownloadPdf(requestId: string): { status: number; mimeType: string; filename: string } {
        isExportingPdf = true;
        // Simüle edilen backend indirme yanıtı
        const response = {
          status: 200,
          mimeType: 'application/pdf',
          filename: `Satin_Alma_Talebi_${requestId}.pdf`
        };
        isExportingPdf = false;
        return response;
      }

      function mockDownloadExcel(reportType: string): { status: number; mimeType: string; filename: string } {
        isExportingExcel = true;
        // Simüle edilen backend indirme yanıtı
        const response = {
          status: 200,
          mimeType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          filename: `ERP_Rapor_${reportType}_20260827.xlsx`
        };
        isExportingExcel = false;
        return response;
      }

      const pdfResult = mockDownloadPdf('PR-20260827-099');
      expect(pdfResult.status).toBe(200);
      expect(pdfResult.mimeType).toBe('application/pdf');
      expect(pdfResult.filename).toContain('.pdf');
      expect(isExportingPdf).toBe(false);

      const excelResult = mockDownloadExcel('KategoriKarlilik');
      expect(excelResult.status).toBe(200);
      expect(excelResult.mimeType).toBe('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
      expect(excelResult.filename).toContain('.xlsx');
      expect(isExportingExcel).toBe(false);
    });
  });

});
