using System;

namespace PhoSocial.API.Models.V2
{
    public class PostV2
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? Caption { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
