using Microsoft.AspNetCore.Http;

namespace PhoSocial.API.DTOs
{
    public class PostCreateDto
    {
        public long UserId { get; set; }
        public string Caption { get; set; }
        public IFormFile Image { get; set; }
    }
}
