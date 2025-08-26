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
        public async Task<IActionResult> GetHistory(string withUserId)
        {
            var userId = User?.FindFirst("id")?.Value;
            if (userId == null) return Unauthorized();
            var conv = await _chatRepo.GetConversationAsync(Guid.Parse(userId), Guid.Parse(withUserId));
            return Ok(conv);
        }

        [HttpPost("mark-read/{messageId}")]
        public async Task<IActionResult> MarkRead(string messageId)
        {
            await _chatRepo.MarkMessageReadAsync(Guid.Parse(messageId));
            return Ok();
        }
    }
}
