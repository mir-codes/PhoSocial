using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoSocial.API.Services;
using PhoSocial.API.Utilities;
using System.Threading.Tasks;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    public class ProfileV2Controller : ControllerBase
    {
        private readonly IProfileServiceV2 _profile;
        public ProfileV2Controller(IProfileServiceV2 profile) { _profile = profile; }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(long userId, [FromQuery] int offset = 0, [FromQuery] int pageSize = 20)
        {
            var current = User.GetUserIdLong();
            var profile = await _profile.GetProfileAsync(userId, current);
            if (profile == null) return NotFound();
            var posts = await _profile.GetUserPostsAsync(userId, offset, pageSize);
            return Ok(new { profile, posts });
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
        {
            var me = User.GetUserIdLong();
            if (me == null) return Unauthorized();
            var updated = await _profile.UpdateProfileAsync(me.Value, req.Username, req.Bio, req.ProfileImageUrl, req.IsPrivate);
            return Ok(updated);
        }
    }

    public class UpdateProfileRequest { public string? Username { get; set; } public string? Bio { get; set; } public string? ProfileImageUrl { get; set; } public bool? IsPrivate { get; set; } }
}
