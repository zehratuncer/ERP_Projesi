import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  if (state.url && state.url !== '/' && state.url !== '/dashboard' && state.url !== '/pos') {
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  } else {
    router.navigate(['/auth/login']);
  }
  return false;
};

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toastService = inject(ToastService);

  const allowedRoles = (route.data?.['roles'] as string[]) || [];
  const user = authService.currentUser();

  // Rol kısıtlaması tanımlanmamışsa geçişe izin ver
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  if (user && allowedRoles.includes(user.role)) {
    return true;
  }

  // Yetkisiz erişim denemesi
  toastService.warning('Bu sayfaya erişim yetkiniz bulunmamaktadır.');
  const targetRoute = authService.getDefaultRouteForRole();
  router.navigate([targetRoute]);
  return false;
};

export const hasRoleGuard: (allowedRoles: string[]) => CanActivateFn = (allowedRoles) => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const toastService = inject(ToastService);
    const user = authService.currentUser();

    if (user && allowedRoles.includes(user.role)) {
      return true;
    }

    toastService.warning('Bu sayfaya erişim yetkiniz bulunmamaktadır.');
    const targetRoute = authService.getDefaultRouteForRole();
    router.navigate([targetRoute]);
    return false;
  };
};
