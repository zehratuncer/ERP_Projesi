import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { User, LoginRequest, LoginResponse } from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  private tokenKey = 'erp_access_token';
  private userKey = 'erp_current_user';
  private rememberMeKey = 'erp_remember_me';

  currentUser = signal<User | null>(this.getStoredUser());
  token = signal<string | null>(this.getStoredToken());

  isAuthenticated = computed(() => !!this.token());
  isAdmin = computed(() => this.currentUser()?.role === 'Admin');
  isManager = computed(() => this.currentUser()?.role === 'Manager' || this.currentUser()?.role === 'Admin');

  constructor(private http: HttpClient, private router: Router) {
    // Sayfa ilk yüklendiğinde token varsa backend'den profil doğrulaması yap
    if (this.token()) {
      this.fetchCurrentUser().subscribe();
    }
  }

  login(credentials: LoginRequest, rememberMe: boolean = true): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.setSession(response.data.token, response.data.user, rememberMe);
        }
      })
    );
  }

  fetchCurrentUser(): Observable<ApiResponse<User> | null> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/me`).pipe(
      tap(response => {
        if (response.isSuccess && response.data) {
          this.currentUser.set(response.data);
          const storage = this.getStorage();
          storage.setItem(this.userKey, JSON.stringify(response.data));
        }
      }),
      catchError(() => {
        // Token geçersizse oturumu temizle
        this.logout();
        return of(null);
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.rememberMeKey);
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.userKey);

    this.token.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  setSession(token: string, user: User, rememberMe: boolean = true) {
    const storage = rememberMe ? localStorage : sessionStorage;

    localStorage.setItem(this.rememberMeKey, rememberMe ? 'true' : 'false');
    storage.setItem(this.tokenKey, token);
    storage.setItem(this.userKey, JSON.stringify(user));

    this.token.set(token);
    this.currentUser.set(user);
  }

  getToken(): string | null {
    return this.token() || this.getStoredToken();
  }

  private getStorage(): Storage {
    const remember = localStorage.getItem(this.rememberMeKey) === 'true';
    return remember ? localStorage : sessionStorage;
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(this.tokenKey) || sessionStorage.getItem(this.tokenKey);
  }

  private getStoredUser(): User | null {
    const raw = localStorage.getItem(this.userKey) || sessionStorage.getItem(this.userKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }
}
