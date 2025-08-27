# PhoSocial API Implementation Plan

## Missing API Endpoints to Implement

### Profile Management
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    // GET api/profile/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    
    // PUT api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
    
    // GET api/profile/{id}/posts
    [HttpGet("{id}/posts")]
    public async Task<IActionResult> GetUserPosts(Guid id)
    
    // GET api/profile/{id}/followers
    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetFollowers(Guid id)
    
    // GET api/profile/{id}/following
    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetFollowing(Guid id)
    
    // POST api/profile/follow/{id}
    [HttpPost("follow/{id}")]
    public async Task<IActionResult> Follow(Guid id)
    
    // POST api/profile/unfollow/{id}
    [HttpPost("unfollow/{id}")]
    public async Task<IActionResult> Unfollow(Guid id)
}
```

### Enhanced Feed Controller Methods
```csharp
// GET api/feed/posts/{id}
[HttpGet("posts/{id}")]
public async Task<IActionResult> GetPost(Guid id)

// PUT api/feed/posts/{id}
[HttpPut("posts/{id}")]
public async Task<IActionResult> UpdatePost(Guid id, [FromBody] PostUpdateDto dto)

// DELETE api/feed/posts/{id}
[HttpDelete("posts/{id}")]
public async Task<IActionResult> DeletePost(Guid id)

// DELETE api/feed/comments/{id}
[HttpDelete("comments/{id}")]
public async Task<IActionResult> DeleteComment(Guid id)
```

### Chat Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    // GET api/chat/conversations
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    
    // GET api/chat/messages/{conversationId}
    [HttpGet("messages/{conversationId}")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    
    // GET api/chat/unread
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadCount()
}
```

### Notification Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    // GET api/notifications
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    
    // PUT api/notifications/{id}/read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    
    // GET api/notifications/unread
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadCount()
}
```

## Frontend Components to Update

### Shared Components
1. Create modern navigation bar with notifications:
```typescript
@Component({
  selector: 'app-modern-nav',
  template: `
    <nav class="modern-nav">
      <div class="nav-brand">
        <img src="assets/logo.png" alt="PhoSocial">
      </div>
      <div class="nav-actions">
        <app-notification-bell [count]="unreadCount"></app-notification-bell>
        <app-user-menu></app-user-menu>
      </div>
    </nav>
  `
})
```

2. Update post card component with animations:
```typescript
@Component({
  selector: 'app-post-card',
  template: `
    <div class="modern-card post-card" [@fadeInUp]>
      <app-post-header [user]="post.user"></app-post-header>
      <app-post-image [src]="post.imagePath"></app-post-image>
      <app-post-actions 
        [likes]="post.likes"
        [comments]="post.comments"
        (onLike)="likePost()"
        (onComment)="showComments()">
      </app-post-actions>
    </div>
  `,
  animations: [fadeInUpAnimation]
})
```

### New Features
1. Stories bar at top of feed
2. Double-tap to like
3. Infinite scroll for feed
4. Pull-to-refresh
5. Image filters and basic editing
6. Rich text comments with emoji picker
7. Typing indicators in chat
8. Read receipts in chat
9. Online status indicators
10. Share post functionality

## UI Modernization Tasks

1. **Color Scheme & Theming**
   - Implement dark mode support
   - Use CSS variables for theming
   - Add smooth color transitions

2. **Animations & Interactions**
   - Add page transition animations
   - Implement gesture controls
   - Add loading skeletons
   - Smooth image loading with blur-up

3. **Layout & Components**
   - Responsive grid system
   - Sticky headers
   - Bottom sheets for mobile
   - Modern form controls
   - Toast notifications

4. **Performance Optimizations**
   - Lazy loading for images
   - Virtual scrolling for long lists
   - Route preloading
   - Service worker for offline support

## Database Changes

See `database_schema.sql` for complete schema including:
- Enhanced user profiles
- Follow relationships
- Chat conversations
- Notifications system
- Comment threading
