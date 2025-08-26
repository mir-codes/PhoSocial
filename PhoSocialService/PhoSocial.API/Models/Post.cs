using System;

namespace PhoSocial.API.Models
{
    public class Post
    {
        public Guid Id {get; set;}
        public Guid UserId {get; set;}
        public string Caption {get; set;}
        public string ImagePath {get; set;}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
