import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { 
  PurchaseRequest, 
  PurchaseRequestListItem, 
  CreatePurchaseRequestRequest, 
  UpdatePurchaseRequestRequest,
  RequestStatus,
  RequestPriority
} from '../models/purchase-request.model';

@Injectable({
  providedIn: 'root'
})
export class PurchaseRequestService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/purchase-requests';

  getPurchaseRequests(
    status?: RequestStatus,
    department?: string,
    priority?: RequestPriority,
    startDate?: string,
    endDate?: string,
    search?: string
  ): Observable<ApiResponse<PurchaseRequestListItem[]>> {
    let params = new HttpParams();
    if (status !== undefined && status !== null) params = params.set('status', status.toString());
    if (department) params = params.set('department', department);
    if (priority !== undefined && priority !== null) params = params.set('priority', priority.toString());
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (search) params = params.set('search', search);

    return this.http.get<ApiResponse<PurchaseRequestListItem[]>>(this.apiUrl, { params });
  }

  getPurchaseRequestById(id: string): Observable<ApiResponse<PurchaseRequest>> {
    return this.http.get<ApiResponse<PurchaseRequest>>(`${this.apiUrl}/${id}`);
  }

  createPurchaseRequest(request: CreatePurchaseRequestRequest): Observable<ApiResponse<PurchaseRequest>> {
    return this.http.post<ApiResponse<PurchaseRequest>>(this.apiUrl, request);
  }

  updatePurchaseRequest(id: string, request: UpdatePurchaseRequestRequest): Observable<ApiResponse<PurchaseRequest>> {
    return this.http.put<ApiResponse<PurchaseRequest>>(`${this.apiUrl}/${id}`, request);
  }

  cancelPurchaseRequest(id: string, reason?: string): Observable<ApiResponse<boolean>> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);

    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}/cancel`, { params });
  }
}
