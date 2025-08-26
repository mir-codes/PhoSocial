using System;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using PhoSocial.API.Models;

namespace PhoSocial.API.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid id);
        Task CreateAsync(User user);
        Task<IEnumerable<User>> SearchByNameOrEmailAsync(string q);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _db;
        public UserRepository(IDbConnectionFactory db) { _db = db; }

        public async Task<User> GetByEmailAsync(string email)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task CreateAsync(User user)
        {
            using var conn = _db.CreateConnection();
            var sql = "INSERT INTO Users (Id, UserName, Email, PasswordHash, CreatedAt) VALUES (@Id,@UserName,@Email,@PasswordHash,@CreatedAt)";
            await conn.ExecuteAsync(sql, user);
        }

        public async Task<IEnumerable<User>> SearchByNameOrEmailAsync(string q)
        {
            using var conn = _db.CreateConnection();
            var sql = "SELECT TOP 50 * FROM Users WHERE UserName LIKE @Q OR Email LIKE @Q";
            return await conn.QueryAsync<User>(sql, new { Q = "%" + q + "%" });
        }
    }
}
