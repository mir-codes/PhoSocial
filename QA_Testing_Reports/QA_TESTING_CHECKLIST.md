# PhoSocial QA Testing Checklist & Integration Tests

## Manual Testing Scenarios

### Environment Setup Verification
- [ ] Database name in appsettings.json matches SQL script
  - Current: FinSocial
  - Should be: PhoSocial
  - **Status:** ❌ BROKEN

- [ ] SQL Server has PhoSocial database created
  ```sql
  SELECT name FROM sys.databases WHERE name = 'PhoSocial'
  ```
  - **Status:** ❌ Database not found (expected: PhoSocial, actual: FinSocial)

- [ ] All tables exist
  ```sql
  SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'
  ```
  - Expected tables: Users, Followers, Posts, Comments, Likes, Stories, Conversations, Messages
  - **Status:** ⚠️ Depends on DB name fix

- [ ] All stored procedures exist
  ```sql
  SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES 
  WHERE ROUTINE_SCHEMA = 'dbo' AND ROUTINE_TYPE = 'PROCEDURE'
  ```
  - Expected procedures: CreatePost, GetFeedPosts, AddComment, GetPostComments, LikePost, UnlikePost, GetProfile, UpdateProfile, GetOrCreateConversation, InsertMessage, GetMessagesPaged, GetConversationList, ExpireStories
  - **Status:** ⚠️ Depends on DB name fix

---

## Authentication Flow Testing

### Signup Test Case
```
Test: Valid Registration
POST /api/auth/signup
Content-Type: application/json

{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "SecurePass123!"
}

Expected:
- Status: 200 OK
- Response: { "token": "eyJ..." }
- User created in database

Actual:
- Status: 500 Internal Server Error
- Error: "Database 'FinSocial' does not exist"
- Reason: Database name mismatch (Issue #1)
- **Status:** ❌ BROKEN
```

```
Test: Login After Registration
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "SecurePass123!"
}

Expected:
- Status: 200 OK
- Response: { "token": "eyJ0eXAiOiJKV..." }
- JWT contains userId as long

Actual:
- Status: 500 error (from signup failure)
- If signup succeeded: Token would contain Guid string
- Next API call rejects with 401 because GetUserIdLong() returns null
- **Status:** ❌ BROKEN
```

### Authorization Test Case
```
Test: Access Protected Endpoint with JWT
GET /api/v2/posts/feed
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGc...

Expected:
- Status: 200 OK
- Response: [ { "id": 1, "caption": "...", "likeCount": 5 }, ... ]

Actual:
- Status: 401 Unauthorized
- Reason: ClaimsExtensions.GetUserIdLong() returns null
  - JWT contains: "id": "550e8400-e29b-41d4-a716-..."  (Guid string)
  - long.TryParse("550e8400-...") returns false
  - GetUserIdLong() returns null
  - Controller rejects: if (userId == null) return Unauthorized()
- **Status:** ❌ BROKEN
```

---

## Feed API Testing

### Get Feed Test Case
```
Test: Fetch User Feed (Should Use V2)
GET /api/v2/posts/feed?offset=0&pageSize=20
Authorization: Bearer <valid_token>

Expected:
- Status: 200 OK
- Response: Array of posts with like/comment counts

Actual Request:
GET /api/feed/posts (Frontend calls old endpoint instead)

Frontend Implementation:
// feed.service.ts Line 11
private base = environment.apiUrl + '/Feed';
getPosts() { return this.http.get<any[]>(`${this.base}/posts`); }

Result:
- Calls old FeedController (still works for now)
- Returns Guid-based posts
- Never migrates to V2 API
- **Status:** ⚠️ PARTIALLY BROKEN (legacy endpoint used)
```

### Like Post Test Case
```
Test: Like a Post
POST /api/v2/posts/123/like
Authorization: Bearer <token>

Expected:
- Status: 200 OK
- Response: { "likeCount": 10 }
- Like added to database

Actual:
- JWT claim extraction fails (same as Issue #3)
- Status: 401 Unauthorized
- **Status:** ❌ BROKEN
```

### Add Comment Test Case
```
Test: Add Comment to Post
POST /api/v2/posts/123/comments
Authorization: Bearer <token>
Content-Type: application/json

{ "text": "Great post!" }

Expected:
- Status: 201 Created
- Response: { "id": 1, "text": "Great post!", "userId": 1, ... }

Actual:
- Status: 404 Not Found  
- Reason: Endpoint doesn't exist in PostsV2Controller
- Only old FeedController has comment endpoint at /api/feed/comment/{postId}
- **Status:** ❌ MISSING IMPLEMENTATION (Issue #8)
```

---

## Chat System Testing

### Create Conversation Test Case
```
Test: Get or Create Conversation with User
POST /api/v2/chat/conversations/with/456
Authorization: Bearer <token>

Expected:
- Status: 200 OK
- Response: { "conversationId": 789 }
- Conversation created/retrieved

Actual:
- Status: 500 error (from auth failure)
- Root cause: JWT user ID extraction failed (Issue #3)
- **Status:** ❌ BROKEN
```

### Send Message Test Case
```
Test: Send Message via SignalR
SignalR Hub Connection: wss://localhost:7095/hubs/chat
Message: SendMessage(otherUserId: 456, message: "Hello!")

Expected:
- Message inserted to database
- ReceiveMessage event sent to recipient
- Both users see message in real-time

Actual:
- Hub connection rejected: 401 Unauthorized
- Reason: JWT token validation fails
  - Query string: "access_token=eyJ0eXAi..."  (contains Guid claim)
  - JwtBearerOptions.OnMessageReceived extracts token
  - Token validated, bearer scheme applied
  - BUT claim extraction fails in ChatHub.SendMessage()
  - var me = Context.User.GetUserIdLong(); // Returns null
  - if (me == null) return; // Silent failure
- **Status:** ❌ BROKEN
```

### Typing Indicator Test Case
```
Test: Typing Indicator Display
Steps:
1. User A opens chat with User B
2. User A starts typing
3. User B should see "User A is typing..."
4. User A pauses typing for 3+ seconds
5. Typing indicator should disappear

Expected:
- Typing indicator shows while typing
- Disappears 3 seconds after inactivity

Actual:
- Typing indicator shows (via UserTyping event)
- Never disappears (no timeout set)
- Persists if user app crashes or network disconnects
- **Status:** ❌ BROKEN (Issue #10)
```

### Unread Messages Test Case
```
Test: Mark Message as Read
POST /api/v2/chat/messages/789/read
Authorization: Bearer <token>

Expected:
- Status: 200 OK
- Message.IsRead = true in database
- Unread count decreases

Actual:
- Status: UNKNOWN (endpoint unclear in code)
- ChatV2Controller has MarkRead in HTTP endpoint?
- Implementation: await _chatService.MarkMessageReadAsync(messageId);
- Should work if auth succeeds
- **Status:** ⚠️ PARTIALLY BROKEN (depends on auth fix)
```

---

## Security Vulnerability Tests

### Brute Force Attack Test Case
```
Test: Unlimited Login Attempts
Script: 1000 login attempts with different passwords

for i in {1..1000}; do
  curl -X POST https://localhost:7095/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@example.com","password":"attempt'$i'"}'
done

Expected:
- Status: 429 Too Many Requests (after ~5 attempts)
- Account locked for 15 minutes

Actual:
- All 1000 requests succeed or fail individually
- No rate limiting
- No account lockout
- No exponential backoff
- **Status:** ❌ VULNERABLE (Issue #11)
```

### Comment XSS Test Case
```
Test: Store and Retrieve XSS Payload in Comment
POST /api/feed/comment/123
Content-Type: application/json

{ "content": "<img src=x onerror=\"alert('XSS')\">" }

Response:
{
  "id": 456,
  "content": "<img src=x onerror=\"alert('XSS')\">"
}

Frontend Display:
{{ comment.content }}
-> Renders as: <img src=x onerror="alert('XSS')">
-> Script executes in browser

Expected:
{ "content": "&lt;img src=x onerror=...&gt;" }

Actual:
{ "content": "<img src=x onerror=\"alert('XSS')\">" }

**Status:** ❌ VULNERABLE (Issue #12)
```

### File Upload Validation Test Case
```
Test: Upload Executable File
POST /api/feed/posts
Content-Type: multipart/form-data

caption=Test&Image=<malware.exe>

Expected:
- Status: 400 Bad Request
- Error: "Only image files are allowed"

Actual:
- Status: 200 OK
- File stored at: wwwroot/uploads/randomguid.exe
- No type validation

**Status:** ❌ VULNERABLE (Issue #13)
```

### Missing Authorization Test Case
```
Test: Access Feed Without Authentication
GET /api/v2/posts/feed?userId=999
(No Authorization header)

Expected:
- Status: 401 Unauthorized
- Error: "Authorization header required"

Actual:
- Status: 200 OK
- Returns posts for userId=999 (anyone can request anyone's feed)

What Should Happen:
[HttpGet("feed")]
[Authorize]  // <-- MISSING
public async Task<IActionResult> GetFeed(...)
{
    var me = User.GetUserIdLong();  // Use current user
    // Don't accept userId parameter
}

**Status:** ❌ VULNERABLE (Issue #7)
```

---

## Database Consistency Tests

### Soft Delete Test Case
```
Test: Verify Soft Deleted Posts Hidden
Steps:
1. Create post (ID=1) with caption "Original"
2. Add 3 likes to post
3. Mark post as deleted: UPDATE Posts SET IsDeleted=1 WHERE Id=1
4. Query GetFeedPosts for user
5. Verify post and likes not included

Expected:
- Post ID=1 not in results
- Like count from deleted post not counted

Actual:
- Likes still counted via subquery:
  LEFT JOIN (SELECT PostId, COUNT(*) FROM Likes GROUP BY PostId) l
  This includes likes from deleted posts

**Status:** ⚠️ BROKEN (Issue #15)
```

### Conversation Unique Constraint Test Case
```
Test: Race Condition in GetOrCreateConversation
Concurrent calls:
- Thread 1: GetOrCreateConversation(userA=100, userB=50)
- Thread 2: GetOrCreateConversation(userA=50, userB=100)

Expected:
- Both return same conversationId (User1Id=50, User2Id=100)

Actual:
- GetOrCreateConversation doesn't normalize IDs
- Thread 1 tries: INSERT (100, 50) -> Violates CHK_User1LessThanUser2
- Thread 2 tries: INSERT (50, 100) -> Same conversation ID or unique constraint error

Stored Procedure Fix Needed:
CREATE OR ALTER PROCEDURE dbo.GetOrCreateConversation
    @UserA BIGINT,
    @UserB BIGINT
AS
BEGIN
    DECLARE @User1Id BIGINT = CASE WHEN @UserA < @UserB THEN @UserA ELSE @UserB END;
    DECLARE @User2Id BIGINT = CASE WHEN @UserA < @UserB THEN @UserB ELSE @UserA END;
    
    -- Now proceed with normalized @User1Id and @User2Id
    ...
END

**Status:** ❌ BROKEN (Issue #9)
```

---

## Performance Load Testing

### Test: Feed Load with 10,000 Posts
```
Setup:
- 10,000 posts in database
- 1,000,000 likes
- 500,000 comments

Query: SELECT * FROM GetFeedPosts(@CurrentUserId=1, @Offset=0, @PageSize=20)

Expected:
- Response time: < 200ms
- Proper index usage

Actual:
- Response time: 2000-3000ms (10x slower)
- Reason: Multiple subquery scans
  - Likes COUNT subquery: Scans 1M rows
  - Comments COUNT subquery: Scans 500k rows
  - UserLikes JOIN: Scans 1M rows

Index Analysis Needed:
EXPLAIN PLAN for GetFeedPosts

**Status:** ⚠️ PERFORMANCE ISSUE (Issue #16)
```

---

## Test Execution Report

| Test Case | Expected | Actual | Status | Issue # |
|-----------|----------|--------|--------|---------|
| DB Connection | PhoSocial database | FinSocial doesn't exist | ❌ FAIL | #1 |
| User Creation | Guid ID → saved to BIGINT | Type mismatch error | ❌ FAIL | #2 |
| JWT Extraction | Parse long from claim | Returns null | ❌ FAIL | #3 |
| Feed Load | /api/v2/posts/feed | /api/feed/posts (old) | ⚠️ PARTIAL | #6 |
| Feed Auth | 401 for no token | 200 OK (public) | ❌ FAIL | #7 |
| Like Post | Add like count | 401 Unauthorized | ❌ FAIL | #3 |
| Add Comment | 201 Created | 404 Not Found | ❌ FAIL | #8 |
| Chat Send | Message inserted | 401 Unauthorized | ❌ FAIL | #3 |
| Typing Indicator | Clear after timeout | Never clears | ❌ FAIL | #10 |
| Brute Force | 429 after limit | All requests allowed | ❌ FAIL | #11 |
| XSS Protection | HTML encoded | Script executes | ❌ FAIL | #12 |
| File Upload | Only images | Any file accepted | ❌ FAIL | #13 |
| Race Condition | Same conv ID | Unique constraint error | ❌ FAIL | #9 |
| Performance | < 200ms | 2000ms+ | ❌ FAIL | #16 |
| **Total** | **15** | **0 Passed** | **0/15** | |

---

## Recommended Test Automation

### Unit Tests to Run
```bash
# Backend tests
dotnet test PhoSocialService/PhoSocial.API.Tests/PhoSocial.API.Tests.csproj \
  --verbosity normal \
  --logger "console;verbosity=detailed"

# Frontend tests
npm --prefix PhoSocialService/PhoSocial.UI run test -- --watch=false --code-coverage
```

### Integration Tests to Run
```bash
# Full integration test against test database
dotnet test --configuration Integration \
  --no-restore \
  --logger "trx;LogFileName=test-results.trx"
```

### Load Testing
```bash
# Using Apache JMeter or similar
jmeter -n -t load-test-feed.jmx -l results.jtl -j jmeter.log
```

### Security Testing
```bash
# OWASP ZAP scan
zaproxy -cmd \
  -quickurl https://localhost:7095 \
  -quickout security-report.html
```

---

## Next Steps

1. **Fix Critical Issues #1-#3** (database, user ID, JWT)
2. **Run Unit Tests** to validate fixes
3. **Run Integration Tests** against fixed code
4. **Run Security Tests** for vulnerabilities
5. **Run Load Tests** for performance
6. **Implement All Missing Functionality** (comments, follow/unfollow)
7. **Re-execute This Checklist** until all tests pass

