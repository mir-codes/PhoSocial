using System;

namespace PhoSocial.API.Models
{
    public class Post
    {
        public long Id {get; set;}
        public long UserId {get; set;}
        public string Caption {get; set;}
        public string ImagePath {get; set;}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
