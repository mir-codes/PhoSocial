-- PhoSocial single-run SQL Server initialization script
-- Creates database, schema, tables, indexes, triggers, and stored procedures
SET NOCOUNT ON;

-- 1) Create database
IF NOT EXISTS(SELECT 1 FROM sys.databases WHERE name = 'PhoSocial')
BEGIN
    CREATE DATABASE PhoSocial;
END
GO

USE PhoSocial;
GO

-- 2) Create common types / helper
-- None required for now

-- 3) Users table
CREATE TABLE dbo.[Users]
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    Bio NVARCHAR(1024) NULL,
    ProfileImageUrl NVARCHAR(1024) NULL,
    IsPrivate BIT NOT NULL DEFAULT(0),
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT(0)
);

CREATE UNIQUE INDEX UX_Users_Username ON dbo.[Users](Username);
CREATE UNIQUE INDEX UX_Users_Email ON dbo.[Users](Email);
CREATE INDEX IX_Users_CreatedAt ON dbo.[Users](CreatedAt DESC);
GO

-- 4) Followers (follower -> following)
CREATE TABLE dbo.Followers
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    FollowerId BIGINT NOT NULL,
    FollowingId BIGINT NOT NULL,
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

ALTER TABLE dbo.Followers
ADD CONSTRAINT FK_Followers_Follower_User FOREIGN KEY (FollowerId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Followers
ADD CONSTRAINT FK_Followers_Following_User FOREIGN KEY (FollowingId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

CREATE UNIQUE INDEX UX_Followers_Follower_Following ON dbo.Followers(FollowerId, FollowingId);
CREATE INDEX IX_Followers_FollowingId ON dbo.Followers(FollowingId);
GO

-- 5) Posts
CREATE TABLE dbo.Posts
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    UserId BIGINT NOT NULL,
    Caption NVARCHAR(2000) NULL,
    ImageUrl NVARCHAR(2048) NULL,
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT(0)
);

ALTER TABLE dbo.Posts
ADD CONSTRAINT FK_Posts_User FOREIGN KEY (UserId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

CREATE NONCLUSTERED INDEX IX_Posts_UserId_CreatedAt ON dbo.Posts(UserId ASC, CreatedAt DESC);
CREATE INDEX IX_Posts_CreatedAt ON dbo.Posts(CreatedAt DESC);
GO

-- 6) Comments
CREATE TABLE dbo.Comments
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    PostId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    CommentText NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT(0)
);

ALTER TABLE dbo.Comments
ADD CONSTRAINT FK_Comments_Post FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Comments
ADD CONSTRAINT FK_Comments_User FOREIGN KEY (UserId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

CREATE INDEX IX_Comments_PostId ON dbo.Comments(PostId, CreatedAt DESC);
GO

-- 7) Likes
CREATE TABLE dbo.Likes
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    PostId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

ALTER TABLE dbo.Likes
ADD CONSTRAINT FK_Likes_Post FOREIGN KEY (PostId) REFERENCES dbo.Posts(Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Likes
ADD CONSTRAINT FK_Likes_User FOREIGN KEY (UserId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

CREATE UNIQUE INDEX UX_Likes_Post_User ON dbo.Likes(PostId, UserId);
CREATE INDEX IX_Likes_PostId ON dbo.Likes(PostId);
GO

-- 8) Stories
CREATE TABLE dbo.Stories
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    UserId BIGINT NOT NULL,
    ImageUrl NVARCHAR(2048) NOT NULL,
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt DATETIME2(3) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT(0)
);

ALTER TABLE dbo.Stories
ADD CONSTRAINT FK_Stories_User FOREIGN KEY (UserId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

CREATE INDEX IX_Stories_UserId_CreatedAt ON dbo.Stories(UserId, CreatedAt DESC);
CREATE INDEX IX_Stories_ExpiresAt ON dbo.Stories(ExpiresAt);
GO

-- 9) Conversations (peer-to-peer only): enforce User1Id < User2Id to keep unique composite
CREATE TABLE dbo.Conversations
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    User1Id BIGINT NOT NULL,
    User2Id BIGINT NOT NULL,
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

ALTER TABLE dbo.Conversations
ADD CONSTRAINT FK_Conversations_User1 FOREIGN KEY (User1Id) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Conversations
ADD CONSTRAINT FK_Conversations_User2 FOREIGN KEY (User2Id) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

-- Enforce smaller id in User1Id to maintain unique pair order
ALTER TABLE dbo.Conversations ADD CONSTRAINT CHK_Conversations_User1LessThanUser2 CHECK (User1Id < User2Id);
CREATE UNIQUE INDEX UX_Conversations_User1_User2 ON dbo.Conversations(User1Id, User2Id);
CREATE INDEX IX_Conversations_UpdatedAt ON dbo.Conversations(UpdatedAt DESC);
GO

-- 10) Messages
CREATE TABLE dbo.Messages
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    ConversationId BIGINT NOT NULL,
    SenderId BIGINT NOT NULL,
    MessageText NVARCHAR(4000) NOT NULL,
    IsRead BIT NOT NULL DEFAULT(0),
    CreatedAt DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

ALTER TABLE dbo.Messages
ADD CONSTRAINT FK_Messages_Conversation FOREIGN KEY (ConversationId) REFERENCES dbo.Conversations(Id) ON DELETE NO ACTION;
ALTER TABLE dbo.Messages
ADD CONSTRAINT FK_Messages_Sender FOREIGN KEY (SenderId) REFERENCES dbo.[Users](Id) ON DELETE NO ACTION;

CREATE INDEX IX_Messages_ConversationId_CreatedAt ON dbo.Messages(ConversationId, CreatedAt DESC);
GO

-- 11) Utility: Triggers to maintain UpdatedAt for tables where UpdatedAt exists
CREATE OR ALTER TRIGGER TRG_Posts_UpdateUpdatedAt
ON dbo.Posts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Posts SET UpdatedAt = SYSUTCDATETIME() WHERE Id IN (SELECT Id FROM inserted);
END;
GO

CREATE OR ALTER TRIGGER TRG_Conversations_UpdateUpdatedAt
ON dbo.Conversations
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Conversations SET UpdatedAt = SYSUTCDATETIME() WHERE Id IN (SELECT Id FROM inserted);
END;
GO

CREATE OR ALTER TRIGGER TRG_Users_UpdateUpdatedAt
ON dbo.[Users]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.[Users] SET UpdatedAt = SYSUTCDATETIME() WHERE Id IN (SELECT Id FROM inserted);
END;
GO

-- 12) Stored Procedures
-- CreatePost: inserts a post and returns it with counts (counts zero)
CREATE OR ALTER PROCEDURE dbo.CreatePost
    @UserId BIGINT,
    @Caption NVARCHAR(2000) = NULL,
    @ImageUrl NVARCHAR(2048) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Posts (UserId, Caption, ImageUrl)
    VALUES (@UserId, @Caption, @ImageUrl);
    
    DECLARE @NewId BIGINT = SCOPE_IDENTITY();
    
    SELECT p.Id, p.UserId, p.Caption, p.ImageUrl, p.CreatedAt, p.UpdatedAt,
           ISNULL(l.LikeCount,0) AS LikeCount,
           ISNULL(c.CommentCount,0) AS CommentCount
    FROM dbo.Posts p
    LEFT JOIN (SELECT PostId, COUNT(1) AS LikeCount FROM dbo.Likes WHERE 1=1 GROUP BY PostId) l ON l.PostId = p.Id
    LEFT JOIN (SELECT PostId, COUNT(1) AS CommentCount FROM dbo.Comments WHERE IsDeleted=0 GROUP BY PostId) c ON c.PostId = p.Id
    WHERE p.Id = @NewId;
END;
GO

-- GetFeedPosts: paginated feed for a user, includes like/comment counts and whether current user liked
CREATE OR ALTER PROCEDURE dbo.GetFeedPosts
    @CurrentUserId BIGINT,
    @Offset INT = 0,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Followed AS (
        SELECT FollowingId AS UserId FROM dbo.Followers WHERE FollowerId = @CurrentUserId
    ),
    FeedUsers AS (
        SELECT UserId FROM Followed
        UNION ALL
        SELECT @CurrentUserId
    ),
    PostRows AS (
        SELECT p.Id, p.UserId, p.Caption, p.ImageUrl, p.CreatedAt, p.UpdatedAt
        FROM dbo.Posts p
        INNER JOIN FeedUsers fu ON p.UserId = fu.UserId
        WHERE p.IsDeleted = 0
    )
    SELECT pr.Id, pr.UserId, pr.Caption, pr.ImageUrl, pr.CreatedAt, pr.UpdatedAt,
           ISNULL(l.LikeCount,0) AS LikeCount,
           ISNULL(c.CommentCount,0) AS CommentCount,
           CASE WHEN ul.UserId IS NULL THEN 0 ELSE 1 END AS LikedByCurrentUser
    FROM (
        SELECT * FROM PostRows
        ORDER BY CreatedAt DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    ) pr
    LEFT JOIN (
        SELECT PostId, COUNT(1) AS LikeCount FROM dbo.Likes GROUP BY PostId
    ) l ON l.PostId = pr.Id
    LEFT JOIN (
        SELECT PostId, COUNT(1) AS CommentCount FROM dbo.Comments WHERE IsDeleted=0 GROUP BY PostId
    ) c ON c.PostId = pr.Id
    LEFT JOIN (
        SELECT PostId, UserId FROM dbo.Likes WHERE UserId = @CurrentUserId
    ) ul ON ul.PostId = pr.Id
    ORDER BY pr.CreatedAt DESC;
END;
GO

-- AddComment: insert comment and return paged comments around it (or first page)
CREATE OR ALTER PROCEDURE dbo.AddComment
    @PostId BIGINT,
    @UserId BIGINT,
    @CommentText NVARCHAR(2000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Comments (PostId, UserId, CommentText)
    VALUES (@PostId, @UserId, @CommentText);
    
    DECLARE @NewId BIGINT = SCOPE_IDENTITY();
    
    SELECT c.Id, c.PostId, c.UserId, u.Username, u.ProfileImageUrl, c.CommentText, c.CreatedAt
    FROM dbo.Comments c
    JOIN dbo.[Users] u ON u.Id = c.UserId
    WHERE c.PostId = @PostId AND c.IsDeleted = 0
    ORDER BY c.CreatedAt DESC
    OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
END;
GO

-- GetPostComments: paginated comments with user info
CREATE OR ALTER PROCEDURE dbo.GetPostComments
    @PostId BIGINT,
    @Offset INT = 0,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.Id, c.PostId, c.UserId, u.Username, u.ProfileImageUrl, c.CommentText, c.CreatedAt
    FROM dbo.Comments c
    JOIN dbo.[Users] u ON u.Id = c.UserId
    WHERE c.PostId = @PostId AND c.IsDeleted = 0
    ORDER BY c.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- LikePost: idempotent like creation and returns updated count
CREATE OR ALTER PROCEDURE dbo.LikePost
    @PostId BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Likes WHERE PostId = @PostId AND UserId = @UserId)
        BEGIN
            INSERT INTO dbo.Likes (PostId, UserId) VALUES (@PostId, @UserId);
        END
        DECLARE @LikeCount INT = (SELECT COUNT(1) FROM dbo.Likes WHERE PostId = @PostId);
        COMMIT TRANSACTION;
        SELECT @LikeCount AS LikeCount;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- UnlikePost: remove like if exists and return updated count
CREATE OR ALTER PROCEDURE dbo.UnlikePost
    @PostId BIGINT,
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DELETE FROM dbo.Likes WHERE PostId = @PostId AND UserId = @UserId;
        DECLARE @LikeCount INT = (SELECT COUNT(1) FROM dbo.Likes WHERE PostId = @PostId);
        COMMIT TRANSACTION;
        SELECT @LikeCount AS LikeCount;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- GetPostWithCounts: single post with aggregated counts and whether current user liked
CREATE OR ALTER PROCEDURE dbo.GetPostWithCounts
    @PostId BIGINT,
    @CurrentUserId BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.Id, p.UserId, p.Caption, p.ImageUrl, p.CreatedAt, p.UpdatedAt,
           ISNULL(l.LikeCount,0) AS LikeCount,
           ISNULL(c.CommentCount,0) AS CommentCount,
           CASE WHEN ul.UserId IS NULL THEN 0 ELSE 1 END AS LikedByCurrentUser
    FROM dbo.Posts p
    LEFT JOIN (SELECT PostId, COUNT(1) AS LikeCount FROM dbo.Likes GROUP BY PostId) l ON l.PostId = p.Id
    LEFT JOIN (SELECT PostId, COUNT(1) AS CommentCount FROM dbo.Comments WHERE IsDeleted=0 GROUP BY PostId) c ON c.PostId = p.Id
    LEFT JOIN (SELECT PostId, UserId FROM dbo.Likes WHERE UserId = @CurrentUserId) ul ON ul.PostId = p.Id
    WHERE p.Id = @PostId AND p.IsDeleted = 0;
END;
GO

-- UpdateProfile: update a user's profile fields
CREATE OR ALTER PROCEDURE dbo.UpdateProfile
    @UserId BIGINT,
    @Username NVARCHAR(50) = NULL,
    @Bio NVARCHAR(1024) = NULL,
    @ProfileImageUrl NVARCHAR(2048) = NULL,
    @IsPrivate BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.[Users]
    SET
        Username = COALESCE(@Username, Username),
        Bio = COALESCE(@Bio, Bio),
        ProfileImageUrl = COALESCE(@ProfileImageUrl, ProfileImageUrl),
        IsPrivate = COALESCE(@IsPrivate, IsPrivate)
    WHERE Id = @UserId AND IsDeleted = 0;

    SELECT Id, Username, Email, Bio, ProfileImageUrl, IsPrivate, CreatedAt, UpdatedAt
    FROM dbo.[Users]
    WHERE Id = @UserId;
END;
GO

-- GetProfile: returns profile and counts
CREATE OR ALTER PROCEDURE dbo.GetProfile
    @UserId BIGINT,
    @CurrentUserId BIGINT = NULL,
    @Offset INT = 0,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.Id, u.Username, u.ProfileImageUrl, u.Bio, u.IsPrivate, u.CreatedAt,
           (SELECT COUNT(1) FROM dbo.Posts p WHERE p.UserId = u.Id AND p.IsDeleted = 0) AS PostCount,
           (SELECT COUNT(1) FROM dbo.Followers f WHERE f.FollowingId = u.Id) AS FollowerCount,
           (SELECT COUNT(1) FROM dbo.Followers f2 WHERE f2.FollowerId = u.Id) AS FollowingCount,
           CASE WHEN @CurrentUserId IS NULL THEN 0
                WHEN EXISTS(SELECT 1 FROM dbo.Followers ff WHERE ff.FollowerId = @CurrentUserId AND ff.FollowingId = u.Id) THEN 1
                ELSE 0 END AS IsFollowing
    FROM dbo.[Users] u
    WHERE u.Id = @UserId AND u.IsDeleted = 0;

    -- paged posts
    SELECT p.Id, p.UserId, p.Caption, p.ImageUrl, p.CreatedAt,
           ISNULL(l.LikeCount,0) AS LikeCount,
           ISNULL(c.CommentCount,0) AS CommentCount
    FROM dbo.Posts p
    LEFT JOIN (SELECT PostId, COUNT(1) AS LikeCount FROM dbo.Likes GROUP BY PostId) l ON l.PostId = p.Id
    LEFT JOIN (SELECT PostId, COUNT(1) AS CommentCount FROM dbo.Comments WHERE IsDeleted=0 GROUP BY PostId) c ON c.PostId = p.Id
    WHERE p.UserId = @UserId AND p.IsDeleted = 0
    ORDER BY p.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- GetConversationList: list conversations for a user with last message and unread count
CREATE OR ALTER PROCEDURE dbo.GetConversationList
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH UserConvs AS (
        SELECT Id, User1Id, User2Id, CreatedAt, UpdatedAt FROM dbo.Conversations
        WHERE User1Id = @UserId OR User2Id = @UserId
    ), LastMessages AS (
        SELECT m.ConversationId, m.Id AS MessageId, m.SenderId, m.MessageText, m.IsRead, m.CreatedAt,
               ROW_NUMBER() OVER (PARTITION BY m.ConversationId ORDER BY m.CreatedAt DESC) rn
        FROM dbo.Messages m
        WHERE m.ConversationId IN (SELECT Id FROM UserConvs)
    )
    SELECT uc.Id AS ConversationId,
           CASE WHEN uc.User1Id = @UserId THEN uc.User2Id ELSE uc.User1Id END AS OtherUserId,
           ou.Username AS OtherUsername, ou.ProfileImageUrl AS OtherProfileImageUrl,
           lm.MessageId AS LastMessageId, lm.SenderId AS LastMessageSenderId, lm.MessageText AS LastMessageText, lm.CreatedAt AS LastMessageAt,
           (SELECT COUNT(1) FROM dbo.Messages mm WHERE mm.ConversationId = uc.Id AND mm.IsRead = 0 AND mm.SenderId <> @UserId) AS UnreadCount
    FROM UserConvs uc
    LEFT JOIN LastMessages lm ON lm.ConversationId = uc.Id AND lm.rn = 1
    LEFT JOIN dbo.[Users] ou ON ou.Id = CASE WHEN uc.User1Id = @UserId THEN uc.User2Id ELSE uc.User1Id END
    ORDER BY lm.CreatedAt DESC, uc.UpdatedAt DESC;
END;
GO

-- GetMessagesPaged: pagination using OFFSET/FETCH (returns newest first)
CREATE OR ALTER PROCEDURE dbo.GetMessagesPaged
    @ConversationId BIGINT,
    @Offset INT = 0,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.Id, m.ConversationId, m.SenderId, u.Username, u.ProfileImageUrl, m.MessageText, m.IsRead, m.CreatedAt
    FROM dbo.Messages m
    JOIN dbo.[Users] u ON u.Id = m.SenderId
    WHERE m.ConversationId = @ConversationId
    ORDER BY m.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- InsertMessage: creates message and returns it
CREATE OR ALTER PROCEDURE dbo.InsertMessage
    @ConversationId BIGINT,
    @SenderId BIGINT,
    @MessageText NVARCHAR(4000)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT INTO dbo.Messages (ConversationId, SenderId, MessageText)
        VALUES (@ConversationId, @SenderId, @MessageText);
        DECLARE @NewId BIGINT = SCOPE_IDENTITY();

        -- update conversation UpdatedAt
        UPDATE dbo.Conversations SET UpdatedAt = SYSUTCDATETIME() WHERE Id = @ConversationId;

        SELECT m.Id, m.ConversationId, m.SenderId, u.Username, u.ProfileImageUrl, m.MessageText, m.IsRead, m.CreatedAt
        FROM dbo.Messages m
        JOIN dbo.[Users] u ON u.Id = m.SenderId
        WHERE m.Id = @NewId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Utility procedure: GetOrCreateConversation: ensures a conversation exists and returns its Id
CREATE OR ALTER PROCEDURE dbo.GetOrCreateConversation
    @UserA BIGINT,
    @UserB BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    IF @UserA = @UserB
    BEGIN
        RAISERROR('Cannot create conversation with same user', 16, 1);
        RETURN;
    END

    DECLARE @Small BIGINT = CASE WHEN @UserA < @UserB THEN @UserA ELSE @UserB END;
    DECLARE @Large BIGINT = CASE WHEN @UserA < @UserB THEN @UserB ELSE @UserA END;

    DECLARE @ConvId BIGINT = (SELECT Id FROM dbo.Conversations WHERE User1Id = @Small AND User2Id = @Large);
    IF @ConvId IS NOT NULL
    BEGIN
        SELECT @ConvId AS ConversationId;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;
        INSERT INTO dbo.Conversations (User1Id, User2Id) VALUES (@Small, @Large);
        SET @ConvId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
        SELECT @ConvId AS ConversationId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- 13) Maintenance: sample cleanup/sp for expiring stories
CREATE OR ALTER PROCEDURE dbo.ExpireStories
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Stories SET IsDeleted = 1 WHERE ExpiresAt <= SYSUTCDATETIME() AND IsDeleted = 0;
END;
GO

-- 14) Sample data placeholders (optional) - commented out
-- INSERT INTO dbo.[Users] (Username, Email, PasswordHash) VALUES ('alice','alice@example.com','<hash>');

PRINT 'PhoSocial database created/updated successfully.';
GO
