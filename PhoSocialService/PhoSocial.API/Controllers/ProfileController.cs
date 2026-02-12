using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoSocial.API.DTOs;
using PhoSocial.API.Models;
using PhoSocial.API.Repositories;
using System;
using System.Threading.Tasks;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IFeedRepository _feedRepository;

        public ProfileController(IUserRepository userRepository, IFeedRepository feedRepository)
        {
            _userRepository = userRepository;
            _feedRepository = feedRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(long id)
        {
            var profile = await _userRepository.GetProfileAsync(id);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateDto dto)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();

            string? profileImagePath = null;
            if (dto.ProfileImage != null)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ProfileImage.FileName);
                var fullPath = Path.Combine(uploads, fileName);
                
                using (var stream = System.IO.File.Create(fullPath))
                {
                    await dto.ProfileImage.CopyToAsync(stream);
                }
                profileImagePath = Path.Combine("uploads", "profiles", fileName).Replace("\\", "/");
            }

            var success = await _userRepository.UpdateProfileAsync(long.Parse(userId), dto, profileImagePath);
            if (!success) return BadRequest();
            return Ok();
        }

        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetUserPosts(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var posts = await _feedRepository.GetUserPostsAsync(id, page, pageSize);
            return Ok(posts);
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetFollowing([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var following = await _userRepository.GetFollowingAsync(long.Parse(userId), page, pageSize);
            return Ok(following);
        }

        [HttpGet("followers")]
        public async Task<IActionResult> GetFollowers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var followers = await _userRepository.GetFollowersAsync(long.Parse(userId), page, pageSize);
            return Ok(followers);
        }

        [HttpPost("follow/{id}")]
        public async Task<IActionResult> Follow(long id)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var success = await _userRepository.FollowUserAsync(long.Parse(userId), id);
            if (!success) return BadRequest();
            return Ok();
        }

        [HttpPost("unfollow/{id}")]
        public async Task<IActionResult> Unfollow(long id)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            
            var success = await _userRepository.UnfollowUserAsync(long.Parse(userId), id);
            if (!success) return BadRequest();
            return Ok();
        }
    }
}
