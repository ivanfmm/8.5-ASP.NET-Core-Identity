using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Xml.Linq;

namespace Blog.Data
{
    /// <summary>
    /// Implementation of <see cref="IArticleRepository"/> using SQLite as a persistence solution.
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly ArticleContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ArticleRepository(ArticleContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Creates the necessary tables for this application if they don't exist already.
        /// Should be called once when starting the service.
        /// </summary>

        public async Task<IEnumerable<Article>> GetAll()
        {
            return await _context.Articles.ToListAsync();
        }

        public async Task<int> GetTotalArticles()
        {
            return await _context.Articles.CountAsync();
        }

        public async Task<IEnumerable<Article>> GetByDateRange(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        {
            return await _context.Articles
                    .Where(a => a.PublishedDate >= startDate
                             && a.PublishedDate <= endDate)
                    .OrderByDescending(a => a.PublishedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }

        public async Task<Article?> GetById(int id)
        {
            return await _context.Articles.FindAsync(id);
        }

        public async Task<Article> Create(Article article)
        {
            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task AddComment(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByArticleId(int articleId)
        {
            return await _context.Comments
                .Where(c => c.ArticleId == articleId)
                .ToListAsync();
        }
        //public async Task<IdentityUser> CreateUser(User user)
        //{
        //    await _context.Users.AddAsync(user);
        //    await _context.SaveChangesAsync(); 
        //    return user;
        //}

        public async Task<IdentityUser?> CreateUser(string username, string email, string password)
        {
            var user = new IdentityUser
            {
                UserName = username,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password); // Crea usuario y hashea la contraseña
            if (result.Succeeded)
            {
                return user;
            }

            return null; // O manejar errores según result.Errors
        }

        public async Task<IdentityUser?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        

        public async Task<IdentityUser?> GetUserById(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task Update(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
        }
    }
}
