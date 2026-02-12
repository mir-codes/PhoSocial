using System;
using System.Threading.Tasks;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.Services
{
    public interface IFeedService
    {
        Task LikePostAsync(long postId, long userId);
        Task CommentPostAsync(long postId, long userId, string content);
    }

    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _repo;
        public FeedService(IFeedRepository repo) { _repo = repo; }

        public async Task LikePostAsync(long postId, long userId)
        {
            await _repo.CreateLikeAsync(new Models.Like { PostId = postId, UserId = userId });
        }

        public async Task CommentPostAsync(long postId, long userId, string content)
        {
            await _repo.CreateCommentAsync(new Models.Comment { PostId = postId, UserId = userId, Content = content, CreatedAt = DateTime.UtcNow });
        }
    }
}
