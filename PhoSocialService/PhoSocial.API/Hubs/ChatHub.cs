using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using PhoSocial.API.Services;
using PhoSocial.API.Models;
using PhoSocial.API.Utilities;

namespace PhoSocial.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatServiceV2 _chatService;
        public ChatHub(IChatServiceV2 chatService)
        {
            _chatService = chatService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        // Send a message to a user: create/get conversation, persist message, notify both participants
        public async Task SendMessage(long otherUserId, string message)
        {
            var me = Context.User.GetUserIdLong();
            if (me == null) return;
            var convId = await _chatService.GetOrCreateConversationAsync(me.Value, otherUserId);
            var msg = await _chatService.SendMessageAsync(convId, me.Value, message);

            // Notify recipient and sender
            await Clients.User(otherUserId.ToString()).SendAsync("ReceiveMessage", msg);
            await Clients.Caller.SendAsync("MessageSent", msg);
        }

        // Typing indicator
        public async Task Typing(long otherUserId)
        {
            var me = Context.User.GetUserIdLong();
            if (me == null) return;
            await Clients.User(otherUserId.ToString()).SendAsync("UserTyping", new { UserId = me.Value });
        }

        public async Task MarkRead(long messageId)
        {
            await _chatService.MarkMessageReadAsync(messageId);
            // Optionally, notify sender that message was read
            // We don't have sender id here; client can manage acknowledgement
        }
    }
}
