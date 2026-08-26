import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="stat-card card card-hover"
      [class.clickable]="clickable"
      (click)="onClick()"
      [attr.title]="clickable ? 'Detayları görmek için tıklayın' : null"
    >
      <div class="stat-content">
        <div class="stat-info">
          <span class="stat-title">{{ title }}</span>
          <h2 class="stat-value">{{ value }}</h2>
          @if (subtitle) {
            <p class="stat-subtitle" [ngClass]="subtitleClass">{{ subtitle }}</p>
          }
        </div>
        <div class="stat-icon-wrapper" [style.backgroundColor]="iconBg">
          <span class="stat-icon">{{ icon }}</span>
        </div>
      </div>
      @if (clickable) {
        <div class="click-hint">
          <span>Detayı Gör ↗</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .stat-card {
      padding: 1.5rem;
      background: var(--bg-surface);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      position: relative;
      overflow: hidden;

      &::after {
        content: '';
        position: absolute;
        bottom: 0;
        left: 0;
        right: 0;
        height: 2px;
        background: linear-gradient(90deg, var(--primary), transparent);
      }
    }

    .stat-content {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .stat-title {
      font-size: 0.8125rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-secondary);
    }

    .stat-value {
      font-size: 2rem;
      font-weight: 800;
      color: var(--text-primary);
      margin: 0.25rem 0;
      font-family: var(--font-heading);
    }

    .stat-subtitle {
      font-size: 0.8125rem;
      margin: 0;
    }

    .stat-icon-wrapper {
      width: 52px;
      height: 52px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      box-shadow: var(--shadow-sm);
    }

    .stat-card.clickable {
      cursor: pointer;
      transition: all var(--transition-normal);

      &:hover {
        transform: translateY(-3px);
        border-color: var(--primary);
        box-shadow: var(--shadow-md), 0 0 15px var(--primary-light);

        .click-hint {
          opacity: 1;
          color: var(--primary);
        }
      }
    }

    .click-hint {
      position: absolute;
      bottom: 6px;
      right: 12px;
      font-size: 0.6875rem;
      font-weight: 600;
      color: var(--text-muted);
      opacity: 0.7;
      transition: all var(--transition-fast);
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }
  `]
})
export class StatCardComponent {
  @Input({ required: true }) title: string = '';
  @Input({ required: true }) value: string | number = '0';
  @Input() icon: string = '📊';
  @Input() iconBg: string = 'rgba(59, 130, 246, 0.15)';
  @Input() subtitle?: string;
  @Input() subtitleClass?: string;
  @Input() clickable: boolean = false;
  @Output() cardClick = new EventEmitter<void>();

  onClick() {
    if (this.clickable) {
      this.cardClick.emit();
    }
  }
}
