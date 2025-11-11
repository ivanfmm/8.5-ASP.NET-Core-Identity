using Blog.Models;
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

        public ArticleRepository(ArticleContext context)
        {
            _context = context;
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

        public async Task<IEnumerable<Article>> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate, int pageNumber, int pageSize)
        {
            return await _context.Articles
                .Where(a => a.PublishedDate >= startDate && a.PublishedDate <= endDate)
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
        public async Task<User> CreateUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); 
            return user;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        

        public async Task<User?> GetUserById(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
