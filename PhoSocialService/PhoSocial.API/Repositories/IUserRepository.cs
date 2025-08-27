using PhoSocial.API.DTOs;
using PhoSocial.API.Models;

namespace PhoSocial.API.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid id);
        Task CreateAsync(User user);
        Task<ProfileDto?> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, ProfileUpdateDto dto, string? profileImagePath);
        Task<IEnumerable<ProfileDto>> GetFollowersAsync(Guid userId, int page, int pageSize);
        Task<IEnumerable<ProfileDto>> GetFollowingAsync(Guid userId, int page, int pageSize);
        Task<bool> FollowUserAsync(Guid followerId, Guid followingId);
        Task<bool> UnfollowUserAsync(Guid followerId, Guid followingId);
        Task<IEnumerable<ProfileDto>> SearchByNameOrEmailAsync(string query);
    }
}
