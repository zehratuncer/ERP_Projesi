import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Beklenmeyen bir hata meydana geldi.';

      if (error.error && error.error.message) {
        errorMessage = error.error.message;
      } else if (error.status === 401) {
        if (!req.url.includes('/api/auth/login')) {
          errorMessage = 'Oturum süreniz doldu veya yetkisiz erişim.';
          authService.logout();
        } else {
          errorMessage = 'E-posta veya şifre hatalı.';
        }
      } else if (error.status === 403) {
        errorMessage = 'Bu işlem için yetkiniz bulunmamaktadır.';
      } else if (error.status === 0) {
        errorMessage = 'Sunucuya ulaşılamıyor. Lütfen backend servisinin çalıştığından emin olun.';
      }

      toastService.error(errorMessage);
      return throwError(() => error);
    })
  );
};
