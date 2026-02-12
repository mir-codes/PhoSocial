using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using PhoSocial.API.Models.V2;
using System.Data;

namespace PhoSocial.API.Repositories.V2
{
    public class PostFeedItem
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? Caption { get; set; }
        public string? ImageUrl { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool LikedByCurrentUser { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }

    public class CommentDto
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CommentText { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }

    public class PostRepositoryV2 : IPostRepositoryV2
    {
        private readonly IDbConnectionFactory _db;
        public PostRepositoryV2(IDbConnectionFactory db) { _db = db; }

        public async Task<PostV2> CreatePostAsync(long userId, string? caption, string? imageUrl)
        {
            using var conn = _db.CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<PostV2>("EXEC dbo.CreatePost @UserId, @Caption, @ImageUrl", new { UserId = userId, Caption = caption, ImageUrl = imageUrl });
            return result!;
        }

        public async Task<IEnumerable<PostFeedItem>> GetFeedPostsAsync(long currentUserId, int offset, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var items = await conn.QueryAsync<PostFeedItem>("EXEC dbo.GetFeedPosts @CurrentUserId, @Offset, @PageSize", new { CurrentUserId = currentUserId, Offset = offset, PageSize = pageSize });
            return items;
        }

        public async Task<int> LikePostAsync(long postId, long userId)
        {
            using var conn = _db.CreateConnection();
            var res = await conn.QueryFirstOrDefaultAsync<int>("EXEC dbo.LikePost @PostId, @UserId", new { PostId = postId, UserId = userId });
            return res;
        }

        public async Task<int> UnlikePostAsync(long postId, long userId)
        {
            using var conn = _db.CreateConnection();
            var res = await conn.QueryFirstOrDefaultAsync<int>("EXEC dbo.UnlikePost @PostId, @UserId", new { PostId = postId, UserId = userId });
            return res;
        }

        public async Task<IEnumerable<CommentDto>> GetPostCommentsAsync(long postId, int offset, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var items = await conn.QueryAsync<CommentDto>("EXEC dbo.GetPostComments @PostId, @Offset, @PageSize", new { PostId = postId, Offset = offset, PageSize = pageSize });
            return items;
        }

        public async Task<IEnumerable<PostV2>> GetUserPostsAsync(long userId, int offset, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var items = await conn.QueryAsync<PostV2>("EXEC dbo.GetProfile @UserId, NULL, @Offset, @PageSize", new { UserId = userId, Offset = offset, PageSize = pageSize });
            return items;
        }
    }
}
