import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-theme-toggle',
  template: `
    <button 
      class="theme-toggle" 
      (click)="toggleTheme()" 
      [attr.aria-label]="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
    >
      <i class="fas" [class.fa-sun]="isDarkMode" [class.fa-moon]="!isDarkMode"></i>
    </button>
  `,
  styles: [`
    .theme-toggle {
      background: transparent;
      border: none;
      color: var(--text);
      padding: 8px;
      border-radius: 50%;
      cursor: pointer;
      transition: all 0.3s ease;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .theme-toggle:hover {
      background: var(--surface-hover);
    }

    .theme-toggle i {
      font-size: 1.2rem;
    }
  `]
})
export class ThemeToggleComponent implements OnInit {
  isDarkMode = false;

  ngOnInit() {
    // Check system preference
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const savedTheme = localStorage.getItem('theme');
    
    if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
      this.enableDarkMode();
    }
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    if (this.isDarkMode) {
      this.enableDarkMode();
    } else {
      this.disableDarkMode();
    }
  }

  private enableDarkMode() {
    document.documentElement.setAttribute('data-theme', 'dark');
    localStorage.setItem('theme', 'dark');
    this.isDarkMode = true;
  }

  private disableDarkMode() {
    document.documentElement.removeAttribute('data-theme');
    localStorage.setItem('theme', 'light');
    this.isDarkMode = false;
  }
}
