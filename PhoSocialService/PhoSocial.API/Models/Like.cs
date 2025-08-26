using System;

namespace PhoSocial.API.Models
{
    public class Like
    {
        public Guid Id {get; set;}
        public Guid PostId {get; set;}
        public Guid UserId {get; set;}
    }
}
