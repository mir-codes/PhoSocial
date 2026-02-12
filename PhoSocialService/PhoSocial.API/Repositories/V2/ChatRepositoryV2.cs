using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using PhoSocial.API.Models.V2;
using System.Data;

namespace PhoSocial.API.Repositories.V2
{
    public class ChatRepositoryV2 : IChatRepositoryV2
    {
        private readonly IDbConnectionFactory _db;
        public ChatRepositoryV2(IDbConnectionFactory db) { _db = db; }

        public async Task<long> GetOrCreateConversationAsync(long userA, long userB)
        {
            using var conn = _db.CreateConnection();
            var res = await conn.QueryFirstOrDefaultAsync<long>("EXEC dbo.GetOrCreateConversation @UserA, @UserB", new { UserA = userA, UserB = userB });
            return res;
        }

        public async Task<MessageV2> InsertMessageAsync(long conversationId, long senderId, string messageText)
        {
            using var conn = _db.CreateConnection();
            var msg = await conn.QueryFirstOrDefaultAsync<MessageV2>("EXEC dbo.InsertMessage @ConversationId, @SenderId, @MessageText", new { ConversationId = conversationId, SenderId = senderId, MessageText = messageText });
            return msg!;
        }

        public async Task<IEnumerable<MessageV2>> GetMessagesPagedAsync(long conversationId, int offset, int pageSize)
        {
            using var conn = _db.CreateConnection();
            var items = await conn.QueryAsync<MessageV2>("EXEC dbo.GetMessagesPaged @ConversationId, @Offset, @PageSize", new { ConversationId = conversationId, Offset = offset, PageSize = pageSize });
            return items;
        }

        public async Task<IEnumerable<ConversationListItem>> GetConversationListAsync(long userId)
        {
            using var conn = _db.CreateConnection();
            var items = await conn.QueryAsync<ConversationListItem>("EXEC dbo.GetConversationList @UserId", new { UserId = userId });
            return items;
        }

        public async Task MarkMessageReadAsync(long messageId)
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync("UPDATE dbo.Messages SET IsRead = 1 WHERE Id = @Id", new { Id = messageId });
        }
    }
}
