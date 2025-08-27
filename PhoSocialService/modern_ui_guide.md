# Modern UI Implementation Guide

## Core UI Principles

1. **Design System**
```css
:root {
  /* Colors */
  --primary: #4776e6;
  --primary-light: #6e8efb;
  --secondary: #ff7eb3;
  --background: #f6f8fb;
  --surface: #ffffff;
  --text-primary: #1a1a1a;
  --text-secondary: #757575;
  
  // Shadows
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.05);
  --shadow-md: 0 8px 32px rgba(0,0,0,0.08);
  --shadow-lg: 0 16px 48px rgba(0,0,0,0.12);
  
  // Spacing
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 2rem;
  
  // Border Radius
  --radius-sm: 8px;
  --radius-md: 16px;
  --radius-lg: 24px;
  
  // Transitions
  --transition-fast: 0.2s;
  --transition-normal: 0.3s;
  --transition-slow: 0.5s;
}
```

2. **Modern Components**

### Updated Feed Post Card
```html
<div class="modern-card post-card">
  <div class="post-header">
    <div class="user-avatar">
      <img [src]="post.user.avatar" alt="avatar">
      <div class="online-indicator" *ngIf="post.user.isOnline"></div>
    </div>
    <div class="user-info">
      <h4>{{post.user.displayName}}</h4>
      <span class="timestamp">{{post.createdAt | timeAgo}}</span>
    </div>
    <button class="action-menu">
      <i class="fas fa-ellipsis-v"></i>
    </button>
  </div>
  
  <div class="post-image" (dblclick)="likePost()">
    <img [src]="post.imagePath" [alt]="post.caption">
    <div class="like-overlay" *ngIf="showLikeAnimation">
      <i class="fas fa-heart"></i>
    </div>
  </div>
  
  <div class="post-actions">
    <div class="action-group">
      <button class="like-btn" [class.liked]="post.isLiked">
        <i class="fas" [class.fa-heart]="post.isLiked" [class.fa-heart-o]="!post.isLiked"></i>
      </button>
      <button class="comment-btn">
        <i class="far fa-comment"></i>
      </button>
      <button class="share-btn">
        <i class="far fa-paper-plane"></i>
      </button>
    </div>
    <button class="save-btn">
      <i class="far fa-bookmark"></i>
    </button>
  </div>
  
  <div class="post-stats">
    <strong>{{post.likes | number}} likes</strong>
  </div>
  
  <div class="post-caption">
    <span class="username">{{post.user.username}}</span>
    {{post.caption}}
  </div>
  
  <div class="post-comments">
    <div class="comment" *ngFor="let comment of post.comments">
      <span class="username">{{comment.user.username}}</span>
      {{comment.content}}
    </div>
  </div>
</div>
```

### Modern Chat Interface
```html
<div class="chat-container">
  <div class="chat-sidebar">
    <div class="search-bar">
      <input type="text" placeholder="Search conversations...">
    </div>
    
    <div class="conversation-list">
      <div class="conversation-item" 
           *ngFor="let conv of conversations"
           [class.active]="conv.id === activeConversation?.id">
        <div class="user-avatar">
          <img [src]="conv.user.avatar" alt="avatar">
          <div class="online-indicator" *ngIf="conv.user.isOnline"></div>
        </div>
        <div class="conversation-info">
          <h4>{{conv.user.displayName}}</h4>
          <p class="last-message">{{conv.lastMessage?.content}}</p>
        </div>
        <div class="conversation-meta">
          <span class="timestamp">{{conv.lastMessage?.createdAt | timeAgo}}</span>
          <div class="unread-badge" *ngIf="conv.unreadCount">
            {{conv.unreadCount}}
          </div>
        </div>
      </div>
    </div>
  </div>
  
  <div class="chat-main">
    <div class="chat-header">
      <div class="user-info">
        <img [src]="activeConversation?.user.avatar" alt="avatar">
        <div class="text-info">
          <h3>{{activeConversation?.user.displayName}}</h3>
          <span class="status">
            {{activeConversation?.user.isOnline ? "Online" : "Offline"}}
          </span>
        </div>
      </div>
      <div class="actions">
        <button><i class="fas fa-phone"></i></button>
        <button><i class="fas fa-video"></i></button>
        <button><i class="fas fa-info-circle"></i></button>
      </div>
    </div>
    
    <div class="message-list" #scrollContainer>
      <div class="message" 
           *ngFor="let msg of messages"
           [class.outgoing]="msg.senderId === currentUserId">
        <div class="message-content">
          {{msg.content}}
          <span class="timestamp">{{msg.createdAt | timeAgo}}</span>
        </div>
      </div>
      <div class="typing-indicator" *ngIf="isTyping">
        <span></span><span></span><span></span>
      </div>
    </div>
    
    <div class="message-input">
      <button class="attach-btn">
        <i class="fas fa-plus"></i>
      </button>
      <input type="text" 
             placeholder="Type a message..."
             [(ngModel)]="newMessage"
             (input)="onTyping()">
      <button class="emoji-btn">
        <i class="far fa-smile"></i>
      </button>
      <button class="send-btn" [disabled]="!newMessage">
        <i class="fas fa-paper-plane"></i>
      </button>
    </div>
  </div>
</div>
```

3. **Animations**
```typescript
import { trigger, transition, style, animate } from "@angular/animations";

export const fadeInUp = trigger("fadeInUp", [
  transition(":enter", [
    style({ opacity: 0, transform: "translateY(20px)" }),
    animate("0.3s ease-out")
  ])
]);

export const slideInRight = trigger("slideInRight", [
  transition(":enter", [
    style({ transform: "translateX(100%)" }),
    animate("0.3s ease-out")
  ])
]);

export const expandCollapse = trigger("expandCollapse", [
  transition(":enter", [
    style({ height: 0, opacity: 0 }),
    animate("0.2s ease-out", style({ height: "*", opacity: 1 }))
  ]),
  transition(":leave", [
    animate("0.2s ease-in", style({ height: 0, opacity: 0 }))
  ])
]);
```

4. **Responsive Design**
```css
/* Modern CSS Grid with media queries */
.post-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

/* Tablet and up */
@media (min-width: 768px) {
  .post-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Desktop and up */
@media (min-width: 992px) {
  .post-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* Modern fluid grid alternative using clamp */
.auto-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(
    auto-fit,
    minmax(min(100%, 300px), 1fr)
  );
}
```

5. **Dark Mode**
```css
/* Dark mode theme */
[data-theme="dark"] {
  --background: #121212;
  --surface: #1e1e1e;
  --text-primary: #ffffff;
  --text-secondary: #b0b0b0;
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.2);
  --shadow-md: 0 8px 32px rgba(0,0,0,0.3);
}

/* System dark mode preference */
@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) {
    --background: #121212;
    --surface: #1e1e1e;
    --text-primary: #ffffff;
    --text-secondary: #b0b0b0;
    --shadow-sm: 0 2px 4px rgba(0,0,0,0.2);
    --shadow-md: 0 8px 32px rgba(0,0,0,0.3);
  }
}
```

6. **Loading States**
```html
<!-- Skeleton Loading -->
<div class="post-card skeleton" *ngIf="loading">
  <div class="skeleton-header">
    <div class="skeleton-avatar"></div>
    <div class="skeleton-text">
      <div class="skeleton-line"></div>
      <div class="skeleton-line small"></div>
    </div>
  </div>
  <div class="skeleton-image"></div>
</div>

<style>
@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

.skeleton {
  background: var(--surface);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.skeleton-header {
  padding: var(--spacing-md);
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.skeleton-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(
    90deg,
    var(--surface) 25%,
    var(--text-secondary) 50%,
    var(--surface) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
}

.skeleton-line {
  height: 16px;
  width: 140px;
  background: linear-gradient(
    90deg,
    var(--surface) 25%,
    var(--text-secondary) 50%,
    var(--surface) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: var(--radius-sm);
  
  &.small {
    width: 100px;
  }
}
</style>
```

## Implementation Steps

1. Update global styles and design system
2. Implement new component architecture
3. Add animations and transitions
4. Implement responsive layouts
5. Add dark mode support
6. Add loading states and error boundaries
7. Optimize performance
8. Add gesture support
9. Implement accessibility features

## Key Features to Add

1. Stories carousel at top of feed
2. Image filters and basic editing
3. Rich text comments with emoji picker
4. Double-tap to like animation
5. Pull to refresh
6. Infinite scroll
7. Toast notifications
8. Bottom sheet modals for mobile
9. Online presence indicators
10. Typing indicators in chat
