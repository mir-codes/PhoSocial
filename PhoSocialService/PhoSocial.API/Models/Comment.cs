using System;

namespace PhoSocial.API.Models
{
    public class Comment
    {
        public long Id {get; set;}
        public long PostId {get; set;}
        public long UserId {get; set;}
        public string Content {get; set;}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
