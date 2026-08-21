import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { User, LoginRequest, LoginResponse } from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  private tokenKey = 'erp_access_token';
  private userKey = 'erp_current_user';

  currentUser = signal<User | null>(this.getStoredUser());
  token = signal<string | null>(localStorage.getItem(this.tokenKey));

  isAuthenticated = computed(() => !!this.token());
  isAdmin = computed(() => this.currentUser()?.role === 'Admin');

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.setSession(response.data.token, response.data.user);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.token.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  setSession(token: string, user: User) {
    localStorage.setItem(this.tokenKey, token);
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.token.set(token);
    this.currentUser.set(user);
  }

  getToken(): string | null {
    return this.token() || localStorage.getItem(this.tokenKey);
  }

  private getStoredUser(): User | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }
}
