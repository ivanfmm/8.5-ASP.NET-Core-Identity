using Blog.Data;
using Blog.Models;
using Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private IArticleRepository _articleRepository;
        private IUserService _userService;

        public ArticlesController(IArticleRepository articleRepository, IUserService userService) //constructor del controlador, inyeccion de dependencia
        {
            _articleRepository = articleRepository;
            _userService = userService;
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
            int sessionUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = await _articleRepository.GetUserById(sessionUserId);
            article.AuthorName = user.Username;
            article.AuthorEmail = user.Email;
            article.PublishedDate = DateTime.UtcNow;
            Article created = await _articleRepository.Create(article);

            return RedirectToAction(nameof(Details), new { id = created.Id });
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
            int sessionUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _articleRepository.GetUserById(sessionUserId);
            if (user == null)
            {
                return RedirectToAction("Method");
            }
            return View(user);
        }

    }
}

