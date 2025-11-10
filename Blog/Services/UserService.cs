using Blog.Data;
using Blog.Models;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;


namespace Blog.Services
{
    public class UserService : IUserService
    {
        private readonly IArticleRepository _articleRepository;

        public UserService(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public User RegisterUser(User user)
        {
            byte[] salt = GetSalt();
            string hashedPassword = HashPassword(user.Password, salt);
            user.Password = hashedPassword;
            user.Salt = salt;
            //Console.WriteLine("se creo la sal y la contraseña");
            return _articleRepository.CreateUser(user);
        }
        public byte[] GetSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            //Console.WriteLine($"Salt: {salt}");
            //Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");
            return salt;
        }

        public string HashPassword(string password, byte[] salt)
        {

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            Console.WriteLine($"Hashed password: {hashed}");

            return hashed;
        }

        public User? VerifyPassword(LoginUser enteredUser)
        {
            User? user = _articleRepository.GetUserByUsername(enteredUser.Username);
            if (user == null)
            {
                return null;
            }
            string hashedEnteredPassword = HashPassword(enteredUser.Password, user.Salt);
            if(hashedEnteredPassword == user.Password)
            {
                return user;
            }
            return null;
        }
    }
}
