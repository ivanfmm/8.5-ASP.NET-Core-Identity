using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private IArticleRepository _articleRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ArticlesController(IArticleRepository articleRepository, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _articleRepository = articleRepository;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Articles");
        }

        // GET: ArticlesController
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            var start = DateTime.MinValue;
            var end = DateTime.MaxValue;
            int pageSize = 50;

            int totalArticles = await _articleRepository.GetTotalArticles();
            int totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(await _articleRepository.GetByDateRange(start, end, page, pageSize));
        }

        // GET: ArticlesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var article = await _articleRepository.GetById(id);
            if (article == null)
            {
                return NotFound();
            }

            var comments = await _articleRepository.GetCommentsByArticleId(id);

            var viewModel = new ArticleDetailsViewModel(article, comments);
            return View(viewModel);
        }

        [Authorize]
        // GET: ArticlesController/Create
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        // POST: ArticlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }
            var user = await _userManager.GetUserAsync(User);
            article.AuthorName = user.UserName;
            article.AuthorEmail = user.Email;
            article.PublishedDate = DateTime.UtcNow;
            Article created = await _articleRepository.Create(article);

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Update(int id)
        {
            var article = await _articleRepository.GetById(id);
            if (article == null) return NotFound();
            return View(article);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(int id, Article updatedArticle)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedArticle);
            }
            var existingArticle = await _articleRepository.GetById(id);
            if (existingArticle == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (existingArticle.AuthorName != userId)
            {
                return Forbid();
            }

            if(DateTime.Now > existingArticle.PublishedDate.AddMinutes(5))
            {
                return RedirectToAction(nameof(Details), new { id = existingArticle.Id });
            }

            existingArticle.Title = updatedArticle.Title;
            existingArticle.Content = updatedArticle.Content;
            existingArticle.edit = true;
            await _articleRepository.Update(existingArticle);
            return RedirectToAction(nameof(Details), new { id = existingArticle.Id });
        }



        [Authorize]
        [HttpPost]
        [Route("Articles/{articleId}/AddComment")]
        public async Task<ActionResult> AddComment(int articleId, Comment comment)
        {
            Article? article = await _articleRepository.GetById(articleId);
            if (article == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(comment.Content))
            {
                return BadRequest();
            }

            comment.ArticleId = articleId;
            comment.PublishedDate = DateTime.UtcNow;
            await _articleRepository.AddComment(comment);

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        [Authorize]
        public async Task<ActionResult> Profile()
        {
            string sessionUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("Session User ID: " + sessionUserId);
            var user = await _articleRepository.GetUserById(sessionUserId);
            return View(user);
        }

    }
}

