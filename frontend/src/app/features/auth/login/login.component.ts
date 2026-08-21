import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
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

  email = 'admin@erp.com';
  password = 'Admin123!';
  isLoading = false;

  onSubmit() {
    if (!this.email || !this.password) {
      this.toastService.warning('Lütfen e-posta ve şifrenizi giriniz.');
      return;
    }

    this.isLoading = true;

    // Backend endpoint hazır olduğunda gerçek API çağrısı yapacak.
    // Şimdilik demo oturum açma akışını destekler:
    setTimeout(() => {
      this.authService.setSession('demo-jwt-token-12345', {
        id: 'usr-1',
        email: this.email,
        fullName: 'Zehra Tuncer (Sistem Yöneticisi)',
        role: 'Admin'
      });

      this.isLoading = false;
      this.toastService.success('Başarıyla giriş yapıldı. Hoş geldiniz!');
      this.router.navigate(['/dashboard']);
    }, 600);
  }
}
