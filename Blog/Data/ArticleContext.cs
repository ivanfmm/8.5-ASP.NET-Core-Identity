using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // dotnet add package Microsoft.EntityFrameworkCore

namespace Blog.Data
{
    public class ArticleContext : IdentityDbContext
    {
        public DbSet<Models.Article> Articles { get; set; }
        public DbSet<Models.Comment> Comments { get; set; }
        public DbSet<Models.User> Users {  get; set; }
        public ArticleContext(DbContextOptions<ArticleContext> options)
            : base(options)
        {
        }
    }
}
