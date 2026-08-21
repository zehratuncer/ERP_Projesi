import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = 'admin@erp.com';
  password = 'Admin123!';
  rememberMe = true;
  isLoading = false;
  errorMessage = '';

  onSubmit() {
    this.errorMessage = '';

    if (!this.email || !this.email.trim()) {
      this.errorMessage = 'Lütfen geçerli bir e-posta adresi giriniz.';
      this.toastService.warning(this.errorMessage);
      return;
    }

    if (!this.password || this.password.length < 6) {
      this.errorMessage = 'Şifreniz en az 6 karakter olmalıdır.';
      this.toastService.warning(this.errorMessage);
      return;
    }

    this.isLoading = true;

    this.authService.login({ email: this.email.trim(), password: this.password }, this.rememberMe).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.isSuccess) {
          this.toastService.success(`Hoş geldiniz, ${response.data.user.fullName}!`);
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
          this.router.navigateByUrl(returnUrl);
        } else {
          this.errorMessage = response.message || 'Giriş başarısız.';
          this.toastService.error(this.errorMessage);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Giriş yapılamadı. Bilgilerinizi kontrol ediniz.';
      }
    });
  }

  fillAdminCredentials() {
    this.email = 'admin@erp.com';
    this.password = 'Admin123!';
    this.toastService.info('Yönetici bilgileri forma dolduruldu.');
  }
}
