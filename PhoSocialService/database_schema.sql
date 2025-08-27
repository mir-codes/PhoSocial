-- PhoSocial Database Schema

-- Users table with enhanced profile information
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(100),
    Bio NVARCHAR(500),
    ProfileImageUrl NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastActive DATETIME2,
    IsPrivate BIT NOT NULL DEFAULT 0
);

-- Posts table with image support
CREATE TABLE Posts (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Caption NVARCHAR(2000),
    ImagePath NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Comments with threading support
CREATE TABLE Comments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PostId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ParentCommentId UNIQUEIDENTIFIER,
    Content NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ParentCommentId) REFERENCES Comments(Id)
);

-- Likes tracking
CREATE TABLE Likes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PostId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_Likes_Post_User UNIQUE (PostId, UserId)
);

-- User relationships (followers/following)
CREATE TABLE UserRelationships (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    FollowerId UNIQUEIDENTIFIER NOT NULL,
    FollowingId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (FollowerId) REFERENCES Users(Id),
    FOREIGN KEY (FollowingId) REFERENCES Users(Id),
    CONSTRAINT UQ_UserRelationships UNIQUE (FollowerId, FollowingId)
);

-- Chat conversations
CREATE TABLE Conversations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastMessageAt DATETIME2
);

-- Conversation participants
CREATE TABLE ConversationParticipants (
    ConversationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    LastReadAt DATETIME2,
    FOREIGN KEY (ConversationId) REFERENCES Conversations(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    PRIMARY KEY (ConversationId, UserId)
);

-- Messages in conversations
CREATE TABLE Messages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ConversationId UNIQUEIDENTIFIER NOT NULL,
    SenderId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ConversationId) REFERENCES Conversations(Id),
    FOREIGN KEY (SenderId) REFERENCES Users(Id)
);

-- Notifications
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- 'like', 'comment', 'follow', 'message'
    ReferenceId UNIQUEIDENTIFIER NOT NULL, -- ID of the related entity (post, comment, etc)
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Create indexes for performance
CREATE INDEX IX_Posts_UserId ON Posts(UserId);
CREATE INDEX IX_Posts_CreatedAt ON Posts(CreatedAt);
CREATE INDEX IX_Comments_PostId ON Comments(PostId);
CREATE INDEX IX_Likes_PostId ON Likes(PostId);
CREATE INDEX IX_Messages_ConversationId ON Messages(ConversationId);
CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);

-- Add sample data for testing
INSERT INTO Users (Id, Username, Email, PasswordHash, DisplayName)
VALUES 
    (NEWID(), 'testuser1', 'test1@example.com', 'hashedpassword123', 'Test User 1'),
    (NEWID(), 'testuser2', 'test2@example.com', 'hashedpassword123', 'Test User 2');
