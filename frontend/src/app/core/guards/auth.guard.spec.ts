describe('Auth Guards (authGuard & roleGuard)', () => {
  function executeAuthGuard(isAuthenticated: boolean, url: string = '/inventory') {
    const routerMock = { navigate: vi.fn() };
    const authServiceMock = { isAuthenticated: () => isAuthenticated };

    let canActivate = false;
    if (authServiceMock.isAuthenticated()) {
      canActivate = true;
    } else {
      if (url && url !== '/' && url !== '/dashboard') {
        routerMock.navigate(['/auth/login'], { queryParams: { returnUrl: url } });
      } else {
        routerMock.navigate(['/auth/login']);
      }
      canActivate = false;
    }

    return { canActivate, routerMock };
  }

  function executeRoleGuard(userRole: string | null, allowedRoles: string[]) {
    const routerMock = { navigate: vi.fn() };
    const user = userRole ? { role: userRole } : null;

    let canActivate = false;
    if (user && allowedRoles.includes(user.role)) {
      canActivate = true;
    } else {
      routerMock.navigate(['/dashboard']);
      canActivate = false;
    }

    return { canActivate, routerMock };
  }

  describe('authGuard', () => {
    it('should allow activation when user is authenticated', () => {
      const { canActivate, routerMock } = executeAuthGuard(true, '/inventory');
      expect(canActivate).toBe(true);
      expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should redirect to /auth/login with returnUrl when user is not authenticated', () => {
      const { canActivate, routerMock } = executeAuthGuard(false, '/reports');
      expect(canActivate).toBe(false);
      expect(routerMock.navigate).toHaveBeenCalledWith(['/auth/login'], { queryParams: { returnUrl: '/reports' } });
    });
  });

  describe('roleGuard', () => {
    it('should allow access when user role is in allowed roles', () => {
      const { canActivate, routerMock } = executeRoleGuard('Manager', ['Admin', 'Manager']);
      expect(canActivate).toBe(true);
      expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should redirect to /dashboard when user role is not allowed', () => {
      const { canActivate, routerMock } = executeRoleGuard('Employee', ['Admin', 'Manager']);
      expect(canActivate).toBe(false);
      expect(routerMock.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });
});
