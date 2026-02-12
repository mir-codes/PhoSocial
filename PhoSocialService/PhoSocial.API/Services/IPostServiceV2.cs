using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Repositories.V2;
using PhoSocial.API.Models.V2;

namespace PhoSocial.API.Services
{
    public interface IPostServiceV2
    {
        Task<PostV2> CreatePostAsync(long userId, string? caption, string? imageUrl);
        Task<IEnumerable<PostFeedItem>> GetFeedAsync(long currentUserId, int offset, int pageSize);
        Task<int> LikePostAsync(long postId, long userId);
        Task<int> UnlikePostAsync(long postId, long userId);
        Task<IEnumerable<CommentDto>> GetCommentsAsync(long postId, int offset, int pageSize);
    }
}
