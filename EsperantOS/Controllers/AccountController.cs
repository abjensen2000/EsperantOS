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
    // Håndterer alt der har med login og logout at gøre.
    // Denne controller kræver IKKE at brugeren er logget ind (ingen [Authorize]).
    public class AccountController : Controller
    {
        // BLL til at slå medarbejdere op – bruges til at hente det korrekte navn fra databasen ved login
        private readonly MedarbejderBLL _medarbejderBLL;

        // Konstruktør – MedarbejderBLL injiceres automatisk af ASP.NET Core
        public AccountController(MedarbejderBLL medarbejderBLL)
        {
            _medarbejderBLL = medarbejderBLL;
        }

        // GET: /Account/Login
        // Viser loginformularen. Hvis brugeren allerede er logget ind, sendes de videre til forsiden.
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        // Behandler loginformularen når den indsendes.
        // Validerer brugernavn og kodeord, slår det rigtige databasenavn op, og opretter en login-cookie.
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Tjek om brugernavn og kodeord matcher (hardcoded til "simon" / "test123")
            // OrdinalIgnoreCase gør at "Simon", "SIMON" og "simon" alle accepteres
            if (model.Username.Equals("simon", StringComparison.OrdinalIgnoreCase) && model.Password == "test123")
            {
                // Slå medarbejderen op i databasen for at hente det korrekte navn (f.eks. "Simon" med stort S).
                // Det er vigtigt at claimet matcher databasenavnet præcist,
                // ellers virker vagtopslag og "frigiv vagt"-knappen ikke.
                var medarbejder = await _medarbejderBLL.GetMedarbejderByNameAsync(model.Username);
                var displayName = medarbejder?.Name ?? model.Username; // Brug DB-navn hvis fundet

                // Opret claims – de gemmes i login-cookien og kan aflæses i alle views og controllere
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, displayName),  // Bruges som User.Identity.Name
                    new Claim(ClaimTypes.Role, "Admin")        // Giver adgang til admin-funktioner
                };

                // Pak claims ind i en identitet og en principal
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Skriv login-cookien til browseren – brugeren er nu logget ind
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            // Forkert brugernavn eller kodeord – vis fejlbesked i formularen
            ModelState.AddModelError("", "Ugyldigt brugernavn eller kodeord.");
            return View(model);
        }

        // GET: /Account/Logout
        // Logger brugeren ud ved at slette login-cookien og sende dem til loginsiden.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
