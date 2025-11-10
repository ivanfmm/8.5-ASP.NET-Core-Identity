using Blog.Data;
using Blog.Models;
using Blog.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

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

        // GET: ArticlesController/Create
        public ActionResult Create()
        {
            var session = GetValidSession();
            if (session == null)
            {
                return RedirectToAction("Method");
            }
            return View();
        }

        // POST: ArticlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }
            var session = GetValidSession();
            if (session == null)
            {
                return RedirectToAction("Method");
            }

            User user = _articleRepository.GetUserById(session.UserID);
            article.AuthorName = user.Username;
            article.AuthorEmail = user.Email;
            article.PublishedDate = DateTimeOffset.UtcNow;
            Article created = _articleRepository.Create(article);

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }

        [HttpPost]
        [Route("Articles/{articleId}/AddComment")]
        public ActionResult AddComment(int articleId, Comment comment)
        {
            var session = GetValidSession();
            if (session == null)
            {
                return RedirectToAction("Method");
            }
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

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        public ActionResult Method()
        {
            return View();
        }

        public ActionResult Profile()
        {
            var session = GetValidSession();
            if (session == null)
            {
                return RedirectToAction("Method");
            }
            var user = _articleRepository.GetUserById(session.UserID);
            if (user == null)
            {
                return RedirectToAction("Method");
            }
            return View(user);
        }
        public ActionResult Logout()
        {
            if (Request.Cookies.TryGetValue("SessionID", out var sessionId))
            {
                _articleRepository.RemoveSession(sessionId);
                Response.Cookies.Delete("SessionID");
            }
            return RedirectToAction("Index", "Articles");
        }


        // POST: ArticlesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(User user)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("No se logro autenticar");
                return View(user);
            }
            User created = _userService.RegisterUser(user);
            CreateSessionCookie(created.Id);

            Console.WriteLine("se logro autenticar");

            return RedirectToAction("Index", "Articles");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Login(LoginUser Loginuser)
        {
            if (!ModelState.IsValid)
            {
                return View(Loginuser);
            }
            User? user = _userService.VerifyPassword(Loginuser);
            if (user != null)
            {
                CreateSessionCookie(user.Id);
                return RedirectToAction("Index", "Articles"); //pagina index del controlador Articles
            }
            ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            return View(user);
        }

        private Session? GetValidSession()
        {
            if (!Request.Cookies.TryGetValue("SessionID", out var sessionId))
            {
                Console.WriteLine("no se encontro sesion");
                return null;
            }
            Console.WriteLine($"valor de sessionId {sessionId}");
            var session = _articleRepository.GetSession(sessionId);
            if (session == null)
                return null;
            Console.WriteLine("se consiguio sesion");
            if (DateTime.UtcNow - session.LastActivity > TimeSpan.FromMinutes(5))
            {
                Console.WriteLine("se cerro la sesion");
                _articleRepository.RemoveSession(sessionId);
                return null;
            }

            // Actualizar LastActivity
            session.LastActivity = DateTime.UtcNow;
            _articleRepository.UpdateSessionActivity(session);
            Console.WriteLine("se actualizo la sesion");

            return session;
        }

        private void CreateSessionCookie(int userId)
        {
            byte[] sessionBytes = RandomNumberGenerator.GetBytes(16); 
            string sessionId = Convert.ToBase64String(sessionBytes);

            var session = new Session
            {
                SessionID = sessionId,
                UserID = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };
            _articleRepository.CreateSession(session);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                         
                Secure = true,                           
                //Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            };

            Response.Cookies.Append("SessionID", sessionId, cookieOptions);
        }
    }
}


//probar comentarios, posts, 
//probar los logins, sign ups
// probar que si me saque despues de 5 minutos de inactividad

//hacer boton para cerrar sesion
//checar lo del hash
