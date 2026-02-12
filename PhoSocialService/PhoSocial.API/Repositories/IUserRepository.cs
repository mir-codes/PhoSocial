using PhoSocial.API.DTOs;
using PhoSocial.API.Models;

namespace PhoSocial.API.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(long id);
        Task CreateAsync(User user);
        Task<ProfileDto?> GetProfileAsync(long userId);
        Task<bool> UpdateProfileAsync(long userId, ProfileUpdateDto dto, string? profileImagePath);
        Task<IEnumerable<ProfileDto>> GetFollowersAsync(long userId, int page, int pageSize);
        Task<IEnumerable<ProfileDto>> GetFollowingAsync(long userId, int page, int pageSize);
        Task<bool> FollowUserAsync(long followerId, long followingId);
        Task<bool> UnfollowUserAsync(long followerId, long followingId);
        Task<IEnumerable<ProfileDto>> SearchByNameOrEmailAsync(string query);
    }
}
