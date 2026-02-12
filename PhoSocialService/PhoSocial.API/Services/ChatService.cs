using System;
using System.Threading.Tasks;
using PhoSocial.API.Models;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.Services
{
    public interface IChatService
    {
        Task<Message> CreateMessageAsync(long senderId, long receiverId, string content);
        Task<Message> GetMessageByIdAsync(long messageId);
        Task MarkMessageReadAsync(long messageId);
    }

    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        private readonly IUserRepository _userRepo;
        public ChatService(IChatRepository repo, IUserRepository userRepo) { _repo = repo; _userRepo = userRepo; }

        public async Task<Message> CreateMessageAsync(long senderId, long receiverId, string content)
        {
            var sender = await _userRepo.GetByIdAsync(senderId);
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                Status = "Sent",
                CreatedAt = DateTime.UtcNow,
                UserName = sender?.UserName
            };
            await _repo.CreateMessageAsync(msg);
            return msg;
        }

        public async Task<Message> GetMessageByIdAsync(long messageId)
        {
            return await _repo.GetMessageByIdAsync(messageId);
        }

        public async Task MarkMessageReadAsync(long messageId)
        {
            await _repo.MarkMessageReadAsync(messageId);        }
    }
}