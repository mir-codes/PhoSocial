using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoSocial.API.Services;
using PhoSocial.API.Utilities;
using System.Threading.Tasks;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    [Authorize]
    public class ChatV2Controller : ControllerBase
    {
        private readonly IChatServiceV2 _chat;
        public ChatV2Controller(IChatServiceV2 chat) { _chat = chat; }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var me = User.GetUserIdLong();
            if (me == null) return Unauthorized();
            var items = await _chat.GetConversationListAsync(me.Value);
            return Ok(items);
        }

        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(long conversationId, [FromQuery] int offset = 0, [FromQuery] int pageSize = 20)
        {
            var me = User.GetUserIdLong();
            if (me == null) return Unauthorized();
            var msgs = await _chat.GetMessagesPagedAsync(conversationId, offset, pageSize);
            return Ok(msgs);
        }

        [HttpPost("conversations/with/{otherUserId}")]
        public async Task<IActionResult> GetOrCreateConversation(long otherUserId)
        {
            var me = User.GetUserIdLong();
            if (me == null) return Unauthorized();
            var convId = await _chat.GetOrCreateConversationAsync(me.Value, otherUserId);
            return Ok(new { conversationId = convId });
        }

        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<IActionResult> SendMessage(long conversationId, [FromBody] SendMessageRequest req)
        {
            var me = User.GetUserIdLong();
            if (me == null) return Unauthorized();
            var msg = await _chat.SendMessageAsync(conversationId, me.Value, req.MessageText);
            return Ok(msg);
        }
    }

    public class SendMessageRequest { public string MessageText { get; set; } = string.Empty; }
}
