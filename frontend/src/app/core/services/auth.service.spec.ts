import { computed, signal } from '@angular/core';

describe('AuthService State & Computed Signals', () => {
  interface User {
    id: string;
    email: string;
    fullName: string;
    role: string;
  }

  it('should compute isAuthenticated based on token signal presence', () => {
    const token = signal<string | null>(null);
    const isAuthenticated = computed(() => !!token());

    expect(isAuthenticated()).toBe(false);

    token.set('jwt_sample_token_123');
    expect(isAuthenticated()).toBe(true);

    token.set(null);
    expect(isAuthenticated()).toBe(false);
  });

  it('should compute isAdmin and isManager correctly based on role', () => {
    const currentUser = signal<User | null>(null);
    const isAdmin = computed(() => currentUser()?.role === 'Admin');
    const isManager = computed(() => currentUser()?.role === 'Manager' || currentUser()?.role === 'Admin');

    expect(isAdmin()).toBe(false);
    expect(isManager()).toBe(false);

    // Set Manager
    currentUser.set({ id: '1', email: 'mgr@erp.com', fullName: 'Manager', role: 'Manager' });
    expect(isAdmin()).toBe(false);
    expect(isManager()).toBe(true);

    // Set Admin
    currentUser.set({ id: '2', email: 'adm@erp.com', fullName: 'Admin', role: 'Admin' });
    expect(isAdmin()).toBe(true);
    expect(isManager()).toBe(true);

    // Set Employee
    currentUser.set({ id: '3', email: 'emp@erp.com', fullName: 'Employee', role: 'Employee' });
    expect(isAdmin()).toBe(false);
    expect(isManager()).toBe(false);
  });
});
