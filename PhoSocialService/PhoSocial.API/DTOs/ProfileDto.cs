using Microsoft.AspNetCore.Http;

namespace PhoSocial.API.DTOs
{
    public class ProfileUpdateDto
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class ProfileDto
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActive { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
        public bool IsFollowing { get; set; }
    }
}
