using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using PhoSocial.API.Repositories;
using PhoSocial.API.DTOs;
using PhoSocial.API.Models;
using PhoSocial.API.Utilities;
using System;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    [Authorize]
    public class FeedController : ControllerBase
    {
        private readonly IFeedRepository _feed;
        public FeedController(IFeedRepository feed) { _feed = feed; }

        [HttpPost("posts")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateDto dto)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();

            string? savedPath = null;
            if (dto.Image != null)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                var full = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(full))
                {
                    await dto.Image.CopyToAsync(stream);
                }
                savedPath = Path.Combine("uploads", fileName)
                .Replace("\\", "/");

            }

            var post = new Post
            {
                UserId = userId.Value,
                Caption = dto.Caption,
                ImagePath = savedPath ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _feed.CreatePostAsync(post);
            return Ok(post);
        }

        [AllowAnonymous]
        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts([FromQuery] int offset = 0, [FromQuery] int pageSize = 20)
        {
            var posts = await _feed.GetRecentPostsAsync(offset, pageSize);
            return Ok(posts);
        }

        [HttpPost("like/{postId}")]
        public async Task<IActionResult> Like(long postId)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            await _feed.CreateLikeAsync(new Like { PostId = postId, UserId = userId.Value });
            return Ok();
        }

        [HttpPost("unlike/{postId}")]
        public async Task<IActionResult> Unlike(long postId)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            await _feed.RemoveLikeAsync(postId, userId.Value);
            return Ok();
        }

        [HttpPost("comment/{postId}")]
        public async Task<IActionResult> Comment(long postId, [FromBody] string content)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            var comment = new Comment { PostId = postId, UserId = userId.Value, Content = content, CreatedAt = DateTime.UtcNow };
            await _feed.CreateCommentAsync(comment);
            return Ok(comment);
        }
    }
}
