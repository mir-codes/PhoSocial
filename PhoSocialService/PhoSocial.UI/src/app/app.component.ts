import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'PhoSocial.UI';
  currentView: string = 'home';
  unreadCount: number = 3;

  ngOnInit() {
    this.setupLikeButtonAnimations();
    this.setupFollowButtons();
    this.setupActionButtons();
    this.setupSearchInput();
  }

  switchView(view: string) {
    this.currentView = view;
    // Update navbar active button
    const buttons = document.querySelectorAll('.nav-btn');
    buttons.forEach(btn => btn.classList.remove('active'));
    const activeBtn = document.querySelector(`[data-view="${view}"]`);
    if (activeBtn) activeBtn.classList.add('active');
  }

  private setupLikeButtonAnimations() {
    // This will be called when the feed is loaded
    const likeBtns = document.querySelectorAll('.like-btn');
    likeBtns.forEach(btn => {
      btn.addEventListener('click', (e: any) => {
        e.currentTarget.classList.toggle('active');
        if (e.currentTarget.classList.contains('active')) {
          e.currentTarget.classList.add('glow-pink');
          this.createHeartParticles(e.currentTarget);
          setTimeout(() => e.currentTarget.classList.remove('glow-pink'), 800);
        }
      });
    });
  }

  private setupFollowButtons() {
    const followBtns = document.querySelectorAll('.follow-btn');
    followBtns.forEach(btn => {
      btn.addEventListener('click', (e: any) => {
        if (e.currentTarget.textContent.trim() === 'Follow') {
          e.currentTarget.textContent = 'Following';
          e.currentTarget.classList.add('glow-green');
          setTimeout(() => e.currentTarget.classList.remove('glow-green'), 500);
        } else {
          e.currentTarget.textContent = 'Follow';
        }
      });
    });
  }

  private setupActionButtons() {
    const actionBtns = document.querySelectorAll('.action-btn:not(.like-btn)');
    actionBtns.forEach(btn => {
      btn.addEventListener('click', (e: any) => {
        const label = btn.querySelector('.action-label');
        if (label) {
          const colorClass = label.className.split(' ').find((c: string) => c.startsWith('color-'));
          if (colorClass) {
            const glowClass = colorClass.replace('color-', 'glow-');
            e.currentTarget.classList.add(glowClass);
            setTimeout(() => e.currentTarget.classList.remove(glowClass), 500);
          }
        }
      });
    });
  }

  private setupSearchInput() {
    const searchInputs = document.querySelectorAll('.search-input');
    searchInputs.forEach(input => {
      const parent = input.closest('.search-box');
      input.addEventListener('focus', () => {
        if (parent) {
          (parent as HTMLElement).style.boxShadow = 'inset 6px 6px 12px var(--neu-shadow-dark), inset -6px -6px 12px var(--neu-shadow-light), 0 0 20px var(--glow-blue)';
        }
      });
      input.addEventListener('blur', () => {
        if (parent) {
          (parent as HTMLElement).style.boxShadow = '';
        }
      });
    });
  }

  private createHeartParticles(button: any) {
    const rect = button.getBoundingClientRect();
    const centerX = rect.left + rect.width / 2;
    const centerY = rect.top + rect.height / 2;
    
    for (let i = 0; i < 8; i++) {
      const heart = document.createElement('div');
      heart.textContent = '❤️';
      heart.style.position = 'fixed';
      heart.style.left = centerX + 'px';
      heart.style.top = centerY + 'px';
      heart.style.fontSize = '1.5rem';
      heart.style.pointerEvents = 'none';
      heart.style.zIndex = '9999';
      heart.style.transition = 'all 1s ease-out';
      
      document.body.appendChild(heart);
      
      setTimeout(() => {
        const angle = (i / 8) * Math.PI * 2;
        const distance = 100;
        const x = Math.cos(angle) * distance;
        const y = Math.sin(angle) * distance;
        
        heart.style.transform = `translate(${x}px, ${y}px) scale(0)`;
        heart.style.opacity = '0';
      }, 10);
      
      setTimeout(() => heart.remove(), 1000);
    }
  }
}
