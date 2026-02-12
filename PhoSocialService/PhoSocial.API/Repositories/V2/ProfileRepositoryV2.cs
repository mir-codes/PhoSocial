using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using PhoSocial.API.Models.V2;
using System.Data;

namespace PhoSocial.API.Repositories.V2
{
    public class ProfileRepositoryV2 : IProfileRepositoryV2
    {
        private readonly IDbConnectionFactory _db;
        public ProfileRepositoryV2(IDbConnectionFactory db) { _db = db; }

        public async Task<ProfileV2?> GetProfileAsync(long userId, long? currentUserId)
        {
            using var conn = _db.CreateConnection();
            // first result set is profile, second is paged posts (we will only read profile here)
            var profile = await conn.QueryFirstOrDefaultAsync<ProfileV2>("EXEC dbo.GetProfile @UserId, @CurrentUserId, @Offset, @PageSize", new { UserId = userId, CurrentUserId = currentUserId, Offset = 0, PageSize = 1 });
            return profile;
        }

        public async Task<IEnumerable<PostV2>> GetUserPostsAsync(long userId, int offset, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var posts = await conn.QueryAsync<PostV2>("EXEC dbo.GetProfile @UserId, NULL, @Offset, @PageSize", new { UserId = userId, Offset = offset, PageSize = pageSize });
            return posts;
        }

        public async Task<ProfileV2> UpdateProfileAsync(long userId, string? username, string? bio, string? profileImageUrl, bool? isPrivate)
        {
            using var conn = _db.CreateConnection();
            var updated = await conn.QueryFirstOrDefaultAsync<ProfileV2>("EXEC dbo.UpdateProfile @UserId, @Username, @Bio, @ProfileImageUrl, @IsPrivate", new { UserId = userId, Username = username, Bio = bio, ProfileImageUrl = profileImageUrl, IsPrivate = isPrivate });
            return updated!;
        }
    }
}
