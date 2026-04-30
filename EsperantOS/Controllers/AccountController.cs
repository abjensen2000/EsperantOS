using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using EsperantOS.BusinessLogic;
using EsperantOS.Models;

namespace EsperantOS.Controllers
{
    public class AccountController : Controller
    {
        private readonly MedarbejderBLL _medarbejderBLL;

        public AccountController(MedarbejderBLL medarbejderBLL)
        {
            _medarbejderBLL = medarbejderBLL;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model.Username.Equals("simon", StringComparison.OrdinalIgnoreCase) && model.Password == "test123")
            {
                // Look up the real DB name so the claim matches exactly
                var medarbejder = await _medarbejderBLL.GetMedarbejderByNameAsync(model.Username);
                var displayName = medarbejder?.Name ?? model.Username;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Ugyldigt brugernavn eller kodeord.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
