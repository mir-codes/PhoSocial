using System;
namespace PhoSocial.API.Models.V2
{
    public class ProfileV2
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PostCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
    }
}
