using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DataAccess.Repositories;
using System.Security.Claims;
using DataAccess.Utilities;

namespace EsperantOS.Controllers
{
    public class AuthController : Controller
    {
        private UnitOfWork _uow;

        public AuthController(UnitOfWork uow)
        {
            _uow = uow;
        }
    
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var medarbejder = _uow.GetMedarbejdere()
                .FirstOrDefault(m => m.Name.ToLower() == username.ToLower());

            if (medarbejder == null || !PasswordHelper.VerifyPassword(password, medarbejder.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Ugyldigt brugernavn eller password");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, medarbejder.Id.ToString()),
                new Claim(ClaimTypes.Name, medarbejder.Name),
                new Claim("ErBestyrelsesmedlem", medarbejder.Bestyrelsesmedlem.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }
    }
}