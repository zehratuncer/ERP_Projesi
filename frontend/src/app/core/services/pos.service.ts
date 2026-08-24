import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { 
  PosProduct, 
  CompleteSaleRequest, 
  SaleReceipt, 
  DailyPosSummary, 
  SaleHistoryItem,
  PaymentMethod 
} from '../models/pos.model';

@Injectable({
  providedIn: 'root'
})
export class PosService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/pos';

  getProductByBarcode(barcodeOrCode: string): Observable<ApiResponse<PosProduct>> {
    return this.http.get<ApiResponse<PosProduct>>(`${this.apiUrl}/product/${encodeURIComponent(barcodeOrCode.trim())}`);
  }

  completeSale(request: CompleteSaleRequest): Observable<ApiResponse<SaleReceipt>> {
    return this.http.post<ApiResponse<SaleReceipt>>(`${this.apiUrl}/complete-sale`, request);
  }

  getReceipt(receiptNumber: string): Observable<ApiResponse<SaleReceipt>> {
    return this.http.get<ApiResponse<SaleReceipt>>(`${this.apiUrl}/receipt/${encodeURIComponent(receiptNumber)}`);
  }

  getDailySummary(date?: string): Observable<ApiResponse<DailyPosSummary>> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);

    return this.http.get<ApiResponse<DailyPosSummary>>(`${this.apiUrl}/daily-summary`, { params });
  }

  getSalesHistory(
    startDate?: string, 
    endDate?: string, 
    paymentMethod?: PaymentMethod, 
    search?: string
  ): Observable<ApiResponse<SaleHistoryItem[]>> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (paymentMethod !== undefined && paymentMethod !== null) params = params.set('paymentMethod', paymentMethod.toString());
    if (search) params = params.set('search', search);

    return this.http.get<ApiResponse<SaleHistoryItem[]>>(`${this.apiUrl}/history`, { params });
  }
}
