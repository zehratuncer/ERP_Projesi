import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5000/api';

  /**
   * Ürün listesini biçimlendirilmiş Excel dosyası olarak indirir.
   */
  downloadProductsExcel(searchTerm?: string, isCriticalOnly?: boolean): Observable<Blob> {
    let params = new HttpParams();
    if (searchTerm) params = params.set('search', searchTerm);
    if (isCriticalOnly !== undefined) params = params.set('isCriticalOnly', isCriticalOnly.toString());

    return this.http.get(`${this.baseUrl}/products/export-excel`, {
      params,
      responseType: 'blob'
    });
  }

  /**
   * Stok hareket geçmişini biçimlendirilmiş Excel dosyası olarak indirir.
   */
  downloadStockMovementsExcel(
    productId?: string,
    transactionType?: number,
    startDate?: string,
    endDate?: string
  ): Observable<Blob> {
    let params = new HttpParams();
    if (productId) params = params.set('productId', productId);
    if (transactionType !== undefined) params = params.set('transactionType', transactionType.toString());
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);

    return this.http.get(`${this.baseUrl}/inventory/export-excel`, {
      params,
      responseType: 'blob'
    });
  }

  /**
   * Analitik raporu biçimlendirilmiş Excel dosyası olarak indirir.
   */
  downloadReportExcel(reportType: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/reports/${reportType}/export-excel`, {
      responseType: 'blob'
    });
  }

  /**
   * Satın alma talebinin kurumsal antetli PDF çıktısını Blob olarak alır.
   */
  getPurchaseRequestPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/purchase-requests/${id}/export-pdf`, {
      responseType: 'blob'
    });
  }

  /**
   * Stok hareketine ait Mal Kabul / Stok Fişi PDF belgesini Blob olarak alır.
   */
  getStockReceiptPdf(transactionId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/inventory/transactions/${transactionId}/export-pdf`, {
      responseType: 'blob'
    });
  }

  /**
   * Gelen Blob verisini tarayıcıda dosya indirme olarak tetikler.
   */
  saveBlobAsFile(blob: Blob, defaultFileName: string): void {
    const blobUrl = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = blobUrl;
    link.download = defaultFileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    setTimeout(() => URL.revokeObjectURL(blobUrl), 1000);
  }
}
