import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `
    <div class="loading-spinner" [class.overlay]="overlay">
      <div class="spinner"></div>
      <div class="loading-text" *ngIf="text">{{text}}</div>
    </div>
  `,
  styles: [`
    .loading-spinner {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 1rem;
    }

    .loading-spinner.overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(var(--surface), 0.8);
      backdrop-filter: blur(8px);
      z-index: 1000;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid var(--border);
      border-top-color: var(--primary);
      border-radius: 50%;
      animation: spin 1s infinite linear;
    }

    .loading-text {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() overlay = false;
  @Input() text?: string;
}
