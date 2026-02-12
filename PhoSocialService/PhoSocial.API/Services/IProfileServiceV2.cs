using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Models.V2;

namespace PhoSocial.API.Services
{
    public interface IProfileServiceV2
    {
        Task<ProfileV2?> GetProfileAsync(long userId, long? currentUserId);
        Task<IEnumerable<PostV2>> GetUserPostsAsync(long userId, int offset, int pageSize);
        Task<ProfileV2> UpdateProfileAsync(long userId, string? username, string? bio, string? profileImageUrl, bool? isPrivate);
    }
}
