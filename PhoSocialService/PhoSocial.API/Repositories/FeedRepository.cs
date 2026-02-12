using System;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using PhoSocial.API.Models;

namespace PhoSocial.API.Repositories
{
    public interface IFeedRepository
    {
        Task CreatePostAsync(Post post);
        Task<IEnumerable<Post>> GetRecentPostsAsync(int take = 50);
        Task<IEnumerable<Post>> GetUserPostsAsync(long userId, int page = 0, int pageSize = 10);
        Task CreateLikeAsync(Like like);
        Task RemoveLikeAsync(long postId, long userId);
        Task CreateCommentAsync(Comment comment);
    }

    public class FeedRepository : IFeedRepository
    {
        private readonly IDbConnectionFactory _db;
        public FeedRepository(IDbConnectionFactory db) { _db = db; }

        public async Task CreatePostAsync(Post post)
        {
            using var conn = _db.CreateConnection();
            var sql = "INSERT INTO Posts (Id, UserId, Caption, ImagePath, CreatedAt) VALUES (@Id,@UserId,@Caption,@ImagePath,@CreatedAt)";
            await conn.ExecuteAsync(sql, post);
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync(int take = 50)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT TOP (@Take) * FROM Posts ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<Post>(sql, new { Take = take });
        }

        public async Task<IEnumerable<Post>> GetUserPostsAsync(long userId, int page = 0, int pageSize = 10)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT * 
                FROM Posts 
                WHERE UserId = @UserId 
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            return await conn.QueryAsync<Post>(sql, new { UserId = userId, Offset = page * pageSize, PageSize = pageSize });
        }

        public async Task CreateLikeAsync(Like like)
        {
            using var conn = _db.CreateConnection();
            var sql = "INSERT INTO Likes (Id, PostId, UserId) VALUES (@Id,@PostId,@UserId)";
            await conn.ExecuteAsync(sql, like);
        }

        public async Task RemoveLikeAsync(long postId, long userId)
        {
            using var conn = _db.CreateConnection();
            var sql = "DELETE FROM Likes WHERE PostId = @PostId AND UserId = @UserId";
            await conn.ExecuteAsync(sql, new { PostId = postId, UserId = userId });
        }

        public async Task CreateCommentAsync(Comment comment)
        {
            using var conn = _db.CreateConnection();
            var sql = "INSERT INTO Comments (Id, PostId, UserId, Content, CreatedAt) VALUES (@Id,@PostId,@UserId,@Content,@CreatedAt)";
            await conn.ExecuteAsync(sql, comment);
        }
    }
}
