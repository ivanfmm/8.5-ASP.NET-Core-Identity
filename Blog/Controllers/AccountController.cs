using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager,
                         SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult LoginWithGitHub()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GitHubCallback)) 
            };
            return Challenge(properties, "GitHub"); // "GitHub" es el nombre que pusiste en AddOAuth
        }

        [HttpGet]
        public async Task<IActionResult> GitHubCallback()
        {
            // Obtiene la información devuelta por GitHub
            var result = await HttpContext.AuthenticateAsync("GitHub");
            if (!result.Succeeded)
            {
                return RedirectToAction("Login", "Account"); // algo salió mal
            }

            var claims = result.Principal.Claims;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            // Busca si el usuario ya existe en Identity
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                // Si no existe, lo crea
                user = new IdentityUser
                {
                    UserName = username,
                    Email = email
                };
                await _userManager.CreateAsync(user);
            }

            // Firma la cookie de Identity
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Redirige a tu página principal
            return RedirectToAction("Index", "Articles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Articles");
        }
    }
}
