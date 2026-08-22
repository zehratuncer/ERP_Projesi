import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { StockMovement, CreateStockMovementRequest } from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/inventory';

  createStockMovement(request: CreateStockMovementRequest): Observable<ApiResponse<StockMovement>> {
    return this.http.post<ApiResponse<StockMovement>>(`${this.apiUrl}/movement`, request);
  }

  getStockMovements(productId?: string, limit: number = 50): Observable<ApiResponse<StockMovement[]>> {
    let params = new HttpParams().set('limit', limit);
    if (productId) params = params.set('productId', productId);

    return this.http.get<ApiResponse<StockMovement[]>>(`${this.apiUrl}/movements`, { params });
  }
}
