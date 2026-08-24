import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response.model';
import {
  StockTurnoverDto,
  SeasonalDemandTrendsDto,
  DeadStockDto,
  SupplierPerformanceDto,
  CategoryProfitabilityDto
} from '../models/reports.model';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/reports';

  getStockTurnover(startDate?: string, endDate?: string, topN: number = 10): Observable<ApiResponse<StockTurnoverDto>> {
    let params = new HttpParams().set('topN', topN.toString());
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);

    return this.http.get<ApiResponse<StockTurnoverDto>>(`${this.apiUrl}/stock-turnover`, { params });
  }

  getSeasonalDemandTrends(year?: number): Observable<ApiResponse<SeasonalDemandTrendsDto>> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());

    return this.http.get<ApiResponse<SeasonalDemandTrendsDto>>(`${this.apiUrl}/seasonal-trends`, { params });
  }

  getDeadStock(inactiveDays: number = 90): Observable<ApiResponse<DeadStockDto>> {
    const params = new HttpParams().set('inactiveDays', inactiveDays.toString());
    return this.http.get<ApiResponse<DeadStockDto>>(`${this.apiUrl}/dead-stock`, { params });
  }

  getSupplierPerformance(): Observable<ApiResponse<SupplierPerformanceDto>> {
    return this.http.get<ApiResponse<SupplierPerformanceDto>>(`${this.apiUrl}/supplier-performance`);
  }

  getCategoryAnalytics(): Observable<ApiResponse<CategoryProfitabilityDto>> {
    return this.http.get<ApiResponse<CategoryProfitabilityDto>>(`${this.apiUrl}/category-analytics`);
  }

  exportToCsv(filename: string, headers: string[], rows: (string | number)[][]): void {
    const csvContent = '\uFEFF' + [
      headers.join(';'),
      ...rows.map(row => row.map(cell => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(';'))
    ].join('\r\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `${filename}_${new Date().toISOString().split('T')[0]}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
