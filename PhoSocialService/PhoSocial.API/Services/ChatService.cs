using System;
using System.Threading.Tasks;
using PhoSocial.API.Models;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.Services
{
    public interface IChatService
    {
        Task<Message> CreateMessageAsync(string senderId, string receiverId, string content);
        Task<Message> GetMessageByIdAsync(string messageId);
        Task MarkMessageReadAsync(string messageId);
    }

    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        public ChatService(IChatRepository repo) { _repo = repo; }

        public async Task<Message> CreateMessageAsync(string senderId, string receiverId, string content)
        {
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.Parse(senderId),
                ReceiverId = Guid.Parse(receiverId),
                Content = content,
                Status = "Sent",
                CreatedAt = DateTime.UtcNow
            };
            await _repo.CreateMessageAsync(msg);
            return msg;
        }

        public async Task<Message> GetMessageByIdAsync(string messageId)
        {
            var id = Guid.Parse(messageId);
            return await _repo.GetMessageByIdAsync(id);
        }

        public async Task MarkMessageReadAsync(string messageId)
        {
            var id = Guid.Parse(messageId);
            await _repo.MarkMessageReadAsync(id);
        }
    }
}
