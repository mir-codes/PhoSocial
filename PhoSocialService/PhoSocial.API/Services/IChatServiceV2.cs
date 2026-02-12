using System.Collections.Generic;
using System.Threading.Tasks;
using PhoSocial.API.Models.V2;

namespace PhoSocial.API.Services
{
    public interface IChatServiceV2
    {
        Task<long> GetOrCreateConversationAsync(long userA, long userB);
        Task<MessageV2> SendMessageAsync(long conversationId, long senderId, string messageText);
        Task<IEnumerable<MessageV2>> GetMessagesPagedAsync(long conversationId, int offset, int pageSize);
        Task<IEnumerable<ConversationListItem>> GetConversationListAsync(long userId);
        Task MarkMessageReadAsync(long messageId);
    }
}
