using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DataAccess.Repositories;
using System.Security.Claims;

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
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }
    
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(string medarbejderId)
        {
            var medarbejder = _uow.GetMedarbejder(int.Parse(medarbejderId));
            
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
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }
    }
}