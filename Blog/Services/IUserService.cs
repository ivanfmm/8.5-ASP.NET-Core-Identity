using Blog.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Blog.Services
{
    public interface IUserService
    {
        public Task<User?> RegisterUser(User user);
        public string HashPassword(string password, byte[] salt);
        public Task<User?> VerifyPassword(LoginUser enteredUser);
        public byte[] GetSalt();
    }
}
