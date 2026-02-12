using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Models.V2;
using PhoSocial.API.Repositories.V2;

namespace PhoSocial.API.Services
{
    public class ChatServiceV2 : IChatServiceV2
    {
        private readonly IChatRepositoryV2 _repo;
        public ChatServiceV2(IChatRepositoryV2 repo) { _repo = repo; }

        public Task<long> GetOrCreateConversationAsync(long userA, long userB)
            => _repo.GetOrCreateConversationAsync(userA, userB);

        public Task<MessageV2> SendMessageAsync(long conversationId, long senderId, string messageText)
            => _repo.InsertMessageAsync(conversationId, senderId, messageText);

        public Task<IEnumerable<MessageV2>> GetMessagesPagedAsync(long conversationId, int offset, int pageSize)
            => _repo.GetMessagesPagedAsync(conversationId, offset, pageSize);

        public Task<IEnumerable<ConversationListItem>> GetConversationListAsync(long userId)
            => _repo.GetConversationListAsync(userId);

        public Task MarkMessageReadAsync(long messageId)
            => _repo.MarkMessageReadAsync(messageId);
    }
}
