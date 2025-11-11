using Blog.Data;
using Blog.Models;
using Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Security.Claims;

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
        public ActionResult Index(int page = 1)
        {
            var start =  DateTimeOffset.MinValue;
            var end = DateTimeOffset.MaxValue;
            int pageSize = 50;

            int totalArticles = _articleRepository.GetTotalArticles();
            int totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(_articleRepository.GetByDateRange(start, end, page, pageSize));
        }

        // GET: ArticlesController/Details/5
        public ActionResult Details(int id)
        {
            var article = _articleRepository.GetById(id);
            if (article == null)
            {
                return NotFound();
            }

            var comments = _articleRepository.GetCommentsByArticleId(id);

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
        public ActionResult Create(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }
            int sessionUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = _articleRepository.GetUserById(sessionUserId);
            article.AuthorName = user.Username;
            article.AuthorEmail = user.Email;
            article.PublishedDate = DateTimeOffset.UtcNow;
            Article created = _articleRepository.Create(article);

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        [Authorize]
        [HttpPost]
        [Route("Articles/{articleId}/AddComment")]
        public ActionResult AddComment(int articleId, Comment comment)
        {
            Article? article = _articleRepository.GetById(articleId);
            if (article == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(comment.Content))
            {
                return BadRequest();
            }

            comment.ArticleId = articleId;
            comment.PublishedDate = DateTimeOffset.UtcNow;
            _articleRepository.AddComment(comment);

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        [Authorize]
        public ActionResult Profile()
        {
            int sessionUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _articleRepository.GetUserById(sessionUserId);
            if (user == null)
            {
                return RedirectToAction("Method");
            }
            return View(user);
        }

    }
}

