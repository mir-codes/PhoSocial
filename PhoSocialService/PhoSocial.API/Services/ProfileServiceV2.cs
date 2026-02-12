using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Models.V2;
using PhoSocial.API.Repositories.V2;

namespace PhoSocial.API.Services
{
    public class ProfileServiceV2 : IProfileServiceV2
    {
        private readonly IProfileRepositoryV2 _repo;
        public ProfileServiceV2(IProfileRepositoryV2 repo) { _repo = repo; }

        public Task<ProfileV2?> GetProfileAsync(long userId, long? currentUserId)
            => _repo.GetProfileAsync(userId, currentUserId);

        public Task<IEnumerable<PostV2>> GetUserPostsAsync(long userId, int offset, int pageSize)
            => _repo.GetUserPostsAsync(userId, offset, pageSize);

        public Task<ProfileV2> UpdateProfileAsync(long userId, string? username, string? bio, string? profileImageUrl, bool? isPrivate)
            => _repo.UpdateProfileAsync(userId, username, bio, profileImageUrl, isPrivate);
    }
}
