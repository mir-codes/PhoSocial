using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoSocial.API.Services;
using PhoSocial.API.Models.V2;
using PhoSocial.API.Utilities;
using System.Threading.Tasks;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    public class PostsV2Controller : ControllerBase
    {
        private readonly IPostServiceV2 _service;
        public PostsV2Controller(IPostServiceV2 service) { _service = service; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest req)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            var post = await _service.CreatePostAsync(userId.Value, req.Caption, req.ImageUrl);
            return CreatedAtAction(nameof(GetPostComments), new { postId = post.Id }, post);
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeed([FromQuery] int offset = 0, [FromQuery] int pageSize = 20)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            var items = await _service.GetFeedAsync(userId.Value, offset, pageSize);
            return Ok(items);
        }

        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> Like(long postId)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            var count = await _service.LikePostAsync(postId, userId.Value);
            return Ok(new { likeCount = count });
        }

        [HttpPost("{postId}/unlike")]
        [Authorize]
        public async Task<IActionResult> Unlike(long postId)
        {
            var userId = User.GetUserIdLong();
            if (userId == null) return Unauthorized();
            var count = await _service.UnlikePostAsync(postId, userId.Value);
            return Ok(new { likeCount = count });
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetPostComments(long postId, [FromQuery] int offset = 0, [FromQuery] int pageSize = 20)
        {
            var comments = await _service.GetCommentsAsync(postId, offset, pageSize);
            return Ok(comments);
        }
    }

    public class CreatePostRequest
    {
        public long UserId { get; set; }
        public string? Caption { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UserActionRequest
    {
        public long UserId { get; set; }
    }
}
