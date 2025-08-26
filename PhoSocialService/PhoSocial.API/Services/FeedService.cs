using System;
using System.Threading.Tasks;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.Services
{
    public interface IFeedService
    {
        Task LikePostAsync(Guid postId, Guid userId);
        Task CommentPostAsync(Guid postId, Guid userId, string content);
    }

    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _repo;
        public FeedService(IFeedRepository repo) { _repo = repo; }

        public async Task LikePostAsync(Guid postId, Guid userId)
        {
            await _repo.CreateLikeAsync(new Models.Like { Id = Guid.NewGuid(), PostId = postId, UserId = userId });
        }

        public async Task CommentPostAsync(Guid postId, Guid userId, string content)
        {
            await _repo.CreateCommentAsync(new Models.Comment { Id = Guid.NewGuid(), PostId = postId, UserId = userId, Content = content, CreatedAt = DateTime.UtcNow });
        }
    }
}
