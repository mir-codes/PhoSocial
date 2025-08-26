using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using PhoSocial.API.Services;
using PhoSocial.API.Models;

namespace PhoSocial.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        private string GetUserId()
        {
            var id = Context.User?.FindFirst("id")?.Value;
            return id;
        }

        public override Task OnConnectedAsync()
        {
            // Optionally, map connection Id to user in memory/store for multi-client support
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = GetUserId();
            if (string.IsNullOrEmpty(senderId)) return;
            var msg = await _chatService.CreateMessageAsync(senderId, receiverId, message);
            // Send to receiver (Clients.User expects claim NameIdentifier, but we use custom 'id' claim;
            // SignalR's default user id provider uses ClaimTypes.NameIdentifier, some setups may require custom IUserIdProvider)
            await Clients.User(receiverId).SendAsync("ReceiveMessage", msg);
            await Clients.Caller.SendAsync("MessageSent", msg);
        }

        public async Task Typing(string receiverId)
        {
            var senderId = GetUserId();
            if (string.IsNullOrEmpty(senderId)) return;
            await Clients.User(receiverId).SendAsync("UserTyping", senderId);
        }

        public async Task MarkRead(string messageId)
        {
            await _chatService.MarkMessageReadAsync(messageId);
            var msg = await _chatService.GetMessageByIdAsync(messageId);
            if (msg != null)
            {
                await Clients.User(msg.SenderId.ToString()).SendAsync("MessageRead", messageId);
            }
        }
    }
}
