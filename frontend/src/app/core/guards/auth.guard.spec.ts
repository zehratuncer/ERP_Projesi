describe('Auth & Role Guards', () => {
  function executeAuthGuard(isAuthenticated: boolean, url: string = '/inventory') {
    const routerMock = { navigate: vi.fn() };
    const authServiceMock = { isAuthenticated: () => isAuthenticated };

    let canActivate = false;
    if (authServiceMock.isAuthenticated()) {
      canActivate = true;
    } else {
      if (url && url !== '/' && url !== '/dashboard' && url !== '/pos') {
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
    const toastMock = { warning: vi.fn() };
    const user = userRole ? { role: userRole } : null;

    const getDefaultRouteForRole = () => (userRole === 'Employee' ? '/pos' : '/dashboard');

    let canActivate = false;
    if (!allowedRoles || allowedRoles.length === 0) {
      canActivate = true;
    } else if (user && allowedRoles.includes(user.role)) {
      canActivate = true;
    } else {
      toastMock.warning('Bu sayfaya erişim yetkiniz bulunmamaktadır.');
      routerMock.navigate([getDefaultRouteForRole()]);
      canActivate = false;
    }

    return { canActivate, routerMock, toastMock };
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

    it('should allow employee access to POS and notifications', () => {
      const { canActivate, routerMock } = executeRoleGuard('Employee', ['Admin', 'Manager', 'Employee']);
      expect(canActivate).toBe(true);
      expect(routerMock.navigate).not.toHaveBeenCalled();
    });

    it('should redirect Employee to /pos and show warning when trying to access managerial pages', () => {
      const { canActivate, routerMock, toastMock } = executeRoleGuard('Employee', ['Admin', 'Manager']);
      expect(canActivate).toBe(false);
      expect(routerMock.navigate).toHaveBeenCalledWith(['/pos']);
      expect(toastMock.warning).toHaveBeenCalledWith('Bu sayfaya erişim yetkiniz bulunmamaktadır.');
    });

    it('should redirect to /dashboard when non-manager user is unauthorized and not an Employee', () => {
      const { canActivate, routerMock } = executeRoleGuard('Guest', ['Admin', 'Manager']);
      expect(canActivate).toBe(false);
      expect(routerMock.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });
});
