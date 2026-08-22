import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import { Supplier, CreateSupplierRequest, UpdateSupplierRequest } from '../models/supplier.model';
import { Product } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/suppliers';

  getSuppliers(search?: string, onlyActive?: boolean): Observable<ApiResponse<Supplier[]>> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (onlyActive !== undefined && onlyActive !== null) params = params.set('onlyActive', onlyActive);

    return this.http.get<ApiResponse<Supplier[]>>(this.apiUrl, { params });
  }

  getSupplierById(id: string): Observable<ApiResponse<Supplier>> {
    return this.http.get<ApiResponse<Supplier>>(`${this.apiUrl}/${id}`);
  }

  getSupplierProducts(id: string): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${this.apiUrl}/${id}/products`);
  }

  createSupplier(request: CreateSupplierRequest): Observable<ApiResponse<Supplier>> {
    return this.http.post<ApiResponse<Supplier>>(this.apiUrl, request);
  }

  updateSupplier(id: string, request: UpdateSupplierRequest): Observable<ApiResponse<Supplier>> {
    return this.http.put<ApiResponse<Supplier>>(`${this.apiUrl}/${id}`, request);
  }

  deleteSupplier(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}
