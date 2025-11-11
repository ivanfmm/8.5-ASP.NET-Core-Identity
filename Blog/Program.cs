using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            var databaseConfig = builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>();
            builder.Services.AddDbContext<ArticleContext>(options => options.UseSqlite(databaseConfig.DefaultConnectionString));

            //agregamos Identity
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
        .AddEntityFrameworkStores<ArticleContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            var app = builder.Build();

           

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Articles}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
