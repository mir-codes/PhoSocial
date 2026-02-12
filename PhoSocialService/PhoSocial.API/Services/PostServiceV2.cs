using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Repositories.V2;
using PhoSocial.API.Models.V2;

namespace PhoSocial.API.Services
{
    public class PostServiceV2 : IPostServiceV2
    {
        private readonly IPostRepositoryV2 _repo;
        public PostServiceV2(IPostRepositoryV2 repo) { _repo = repo; }

        public Task<PostV2> CreatePostAsync(long userId, string? caption, string? imageUrl)
            => _repo.CreatePostAsync(userId, caption, imageUrl);

        public Task<IEnumerable<PostFeedItem>> GetFeedAsync(long currentUserId, int offset, int pageSize)
            => _repo.GetFeedPostsAsync(currentUserId, offset, pageSize);

        public Task<int> LikePostAsync(long postId, long userId)
            => _repo.LikePostAsync(postId, userId);

        public Task<int> UnlikePostAsync(long postId, long userId)
            => _repo.UnlikePostAsync(postId, userId);

        public Task<IEnumerable<CommentDto>> GetCommentsAsync(long postId, int offset, int pageSize)
            => _repo.GetPostCommentsAsync(postId, offset, pageSize);
    }
}
