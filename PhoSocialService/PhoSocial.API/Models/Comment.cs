using System;

namespace PhoSocial.API.Models
{
    public class Comment
    {
        public Guid Id {get; set;}
        public Guid PostId {get; set;}
        public Guid UserId {get; set;}
        public string Content {get; set;}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
