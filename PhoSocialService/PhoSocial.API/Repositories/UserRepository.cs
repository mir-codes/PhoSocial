using System;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using PhoSocial.API.Models;
using PhoSocial.API.DTOs;

namespace PhoSocial.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _db;
        public UserRepository(IDbConnectionFactory db) { _db = db; }

        public async Task<User> GetByEmailAsync(string email)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User> GetByIdAsync(long id)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task CreateAsync(User user)
        {
            using var conn = _db.CreateConnection();
            var sql = "INSERT INTO Users (UserName, Email, PasswordHash, CreatedAt) VALUES (@UserName,@Email,@PasswordHash,@CreatedAt)";
            await conn.ExecuteAsync(sql, user);
        }

        public async Task<IEnumerable<ProfileDto>> SearchByNameOrEmailAsync(string q)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT 
                    u.*,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowingId = u.Id) as FollowersCount,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowerId = u.Id) as FollowingCount,
                    (SELECT COUNT(*) FROM Posts WHERE UserId = u.Id) as PostsCount
                FROM Users u
                WHERE u.Username LIKE @Q OR u.Email LIKE @Q
                ORDER BY u.DisplayName
                OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY";

            return await conn.QueryAsync<ProfileDto>(sql, new { Q = "%" + q + "%" });
        }

        public async Task<ProfileDto?> GetProfileAsync(long userId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT 
                    u.*,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowingId = @UserId) as FollowersCount,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowerId = @UserId) as FollowingCount,
                    (SELECT COUNT(*) FROM Posts WHERE UserId = @UserId) as PostsCount
                FROM Users u
                WHERE u.Id = @UserId";

            return await conn.QueryFirstOrDefaultAsync<ProfileDto>(sql, new { UserId = userId });
        }

        public async Task<bool> UpdateProfileAsync(long userId, ProfileUpdateDto dto, string? profileImagePath)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                UPDATE Users 
                SET 
                    DisplayName = COALESCE(@DisplayName, DisplayName),
                    Bio = COALESCE(@Bio, Bio),
                    ProfileImageUrl = COALESCE(@ProfileImageUrl, ProfileImageUrl),
                    IsPrivate = @IsPrivate
                WHERE Id = @UserId";

            var rowsAffected = await conn.ExecuteAsync(sql, new { 
                UserId = userId,
                dto.DisplayName,
                dto.Bio,
                ProfileImageUrl = profileImagePath,
                dto.IsPrivate
            });

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<ProfileDto>> GetFollowersAsync(long userId, int page, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT 
                    u.*,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowingId = u.Id) as FollowersCount,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowerId = u.Id) as FollowingCount,
                    (SELECT COUNT(*) FROM Posts WHERE UserId = u.Id) as PostsCount
                FROM Users u
                INNER JOIN UserRelationships ur ON u.Id = ur.FollowerId
                WHERE ur.FollowingId = @UserId
                ORDER BY ur.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return await conn.QueryAsync<ProfileDto>(sql, new { 
                UserId = userId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<IEnumerable<ProfileDto>> GetFollowingAsync(long userId, int page, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                SELECT 
                    u.*,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowingId = u.Id) as FollowersCount,
                    (SELECT COUNT(*) FROM UserRelationships WHERE FollowerId = u.Id) as FollowingCount,
                    (SELECT COUNT(*) FROM Posts WHERE UserId = u.Id) as PostsCount
                FROM Users u
                INNER JOIN UserRelationships ur ON u.Id = ur.FollowingId
                WHERE ur.FollowerId = @UserId
                ORDER BY ur.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            return await conn.QueryAsync<ProfileDto>(sql, new { 
                UserId = userId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            });
        }

        public async Task<bool> FollowUserAsync(long followerId, long followingId)
        {
            using var conn = _db.CreateConnection();
            try
            {
                var sql = @"
                    INSERT INTO UserRelationships (Id, FollowerId, FollowingId, CreatedAt)
                    VALUES (NEWID(), @FollowerId, @FollowingId, GETUTCDATE())";

                await conn.ExecuteAsync(sql, new { FollowerId = followerId, FollowingId = followingId });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UnfollowUserAsync(long followerId, long followingId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                DELETE FROM UserRelationships 
                WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId";

            var rowsAffected = await conn.ExecuteAsync(sql, new { 
                FollowerId = followerId, 
                FollowingId = followingId 
            });

            return rowsAffected > 0;
        }
    }
}
