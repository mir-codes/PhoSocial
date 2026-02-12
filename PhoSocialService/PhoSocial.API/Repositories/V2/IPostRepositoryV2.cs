using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Models.V2;

namespace PhoSocial.API.Repositories.V2
{
    public interface IPostRepositoryV2
    {
        Task<PostV2> CreatePostAsync(long userId, string? caption, string? imageUrl);
        Task<IEnumerable<PostFeedItem>> GetFeedPostsAsync(long currentUserId, int offset, int pageSize);
        Task<int> LikePostAsync(long postId, long userId);
        Task<int> UnlikePostAsync(long postId, long userId);
        Task<IEnumerable<CommentDto>> GetPostCommentsAsync(long postId, int offset, int pageSize);
        Task<IEnumerable<PostV2>> GetUserPostsAsync(long userId, int offset, int pageSize);
    }
}
