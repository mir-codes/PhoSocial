using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;
        public ChatController(IChatRepository chatRepo) { _chatRepo = chatRepo; }

        [HttpGet("history/{withUserId}")]
        public async Task<IActionResult> GetHistory(long withUserId)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            var conv = await _chatRepo.GetConversationAsync(long.Parse(userId), withUserId);
            return Ok(conv);
        }

        [HttpPost("mark-read/{messageId}")]
        public async Task<IActionResult> MarkRead(long messageId)
        {
            await _chatRepo.MarkMessageReadAsync(messageId);
            return Ok();
        }
    }
}
