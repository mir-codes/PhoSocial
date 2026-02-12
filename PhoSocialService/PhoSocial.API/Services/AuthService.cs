using System;
using System.Threading.Tasks;
using PhoSocial.API.DTOs;
using PhoSocial.API.Models;
using PhoSocial.API.Repositories;
using PhoSocial.API.Utilities;
using Microsoft.Extensions.Configuration;

namespace PhoSocial.API.Services
{
    public interface IAuthService
    {
        Task<string> SignupAsync(SignupDto dto);
        Task<string> LoginAsync(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;
        private readonly ISanitizationService _sanitizer;

        public AuthService(IUserRepository users, IConfiguration config, ISanitizationService sanitizer)
        {
            _users = users;
            _config = config;
            _sanitizer = sanitizer;
        }

        public async Task<string> SignupAsync(SignupDto dto)
        {
            var existing = await _users.GetByEmailAsync(dto.Email);
            if (existing != null) throw new Exception("Email already exists");
            
            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User 
            { 
                UserName = _sanitizer.SanitizeHtml(dto.UserName), 
                Email = dto.Email, 
                PasswordHash = hashed, 
                CreatedAt = DateTime.UtcNow 
            };
            await _users.CreateAsync(user);
            return JwtHelper.GenerateToken(_config, user.Id, user.Email, user.UserName);
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            string email = dto.Email.Trim();
            string password = dto.Password.Trim();

            var user = await _users.GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) 
                throw new Exception("Invalid credentials");
            
            return JwtHelper.GenerateToken(_config, user.Id, user.Email, user.UserName);
        }
    }
}
