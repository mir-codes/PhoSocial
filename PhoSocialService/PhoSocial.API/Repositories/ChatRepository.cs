using System;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using PhoSocial.API.Models;

namespace PhoSocial.API.Repositories
{
    public interface IChatRepository
    {
        Task CreateMessageAsync(Message message);
        Task<Message> GetMessageByIdAsync(Guid id);
        Task<IEnumerable<Message>> GetConversationAsync(Guid userA, Guid userB, int take = 50);
        Task MarkMessageReadAsync(Guid id);
    }

    public class ChatRepository : IChatRepository
    {
        private readonly IDbConnectionFactory _db;
        public ChatRepository(IDbConnectionFactory db) { _db = db; }

        public async Task CreateMessageAsync(Message message)
        {
            using var conn = _db.CreateConnection();
            var sql = "INSERT INTO Messages (Id, SenderId, ReceiverId, Content, Status, CreatedAt, UserName) VALUES (@Id,@SenderId,@ReceiverId,@Content,@Status,@CreatedAt,@UserName)";
            await conn.ExecuteAsync(sql, message);
        }

        public async Task<Message> GetMessageByIdAsync(Guid id)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT * FROM Messages WHERE Id = @Id";
            return await conn.QueryFirstOrDefaultAsync<Message>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(Guid userA, Guid userB, int take = 50)
        {
            using var conn = _db.CreateConnection();
            var sql = @"SELECT TOP (@Take) * FROM Messages
                        WHERE (SenderId = @A AND ReceiverId = @B) OR (SenderId = @B AND ReceiverId = @A)
                        ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<Message>(sql, new { A = userA, B = userB, Take = take });
        }

        public async Task MarkMessageReadAsync(Guid id)
        {
            using var conn = _db.CreateConnection();
            var sql = "UPDATE Messages SET Status = 'Read' WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id });
        }
    }
}
