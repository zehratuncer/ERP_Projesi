import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  if (state.url && state.url !== '/' && state.url !== '/dashboard') {
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  } else {
    router.navigate(['/auth/login']);
  }
  return false;
};

export const roleGuard: (allowedRoles: string[]) => CanActivateFn = (allowedRoles) => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const user = authService.currentUser();

    if (user && allowedRoles.includes(user.role)) {
      return true;
    }

    router.navigate(['/dashboard']);
    return false;
  };
};
