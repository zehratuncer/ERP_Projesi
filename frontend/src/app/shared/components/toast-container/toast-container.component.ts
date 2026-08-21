import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-wrapper">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast-item toast-{{ toast.type }}" (click)="toastService.remove(toast.id)">
          <div class="toast-icon">
            @switch (toast.type) {
              @case ('success') { <span>✅</span> }
              @case ('error') { <span>❌</span> }
              @case ('warning') { <span>⚠️</span> }
              @default { <span>ℹ️</span> }
            }
          </div>
          <div class="toast-content">
            <p>{{ toast.message }}</p>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-item {
      cursor: pointer;
      user-select: none;
      display: flex;
      align-items: center;
      gap: 0.75rem;

      .toast-content p {
        margin: 0;
        font-size: 0.875rem;
        font-weight: 500;
        color: var(--text-primary);
      }
    }
  `]
})
export class ToastContainerComponent {
  toastService = inject(ToastService);
}
