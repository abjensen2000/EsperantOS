using Microsoft.AspNetCore.Authorization;
using EsperantOS.Data;
using EsperantOS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace EsperantOS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly EsperantOSContext _context;

        public HomeController(EsperantOSContext context)
        {
            this._context = context;
        }

        public IActionResult Index()
        {
            var currentUser = User.Identity?.Name ?? "Ukendt";

            // Vi simulerer "currentUser's" vagter
            var mineVagter = _context.Vagter
                .Include(v => v.Medarbejdere)
                .Where(v => v.Medarbejdere.Any(m => m.Name == currentUser))
                .OrderBy(v => v.Dato)
                .ToList();

            var viewModel = new HomeViewModel
            {
                VelkomstBesked = "Velkommen til Esperanto!",
                BestyrelsesBesked = "Husk at tjekke frigivede vagter. Vi mangler folk til fredagsbaren d. 25.!",
                MineVagter = mineVagter
            };
            
            return View(viewModel);
        }

        public IActionResult Upvote()
        {
            return View(); // Midlertidig fiks så det ikke kalder sig selv uendeligt
        }
    }
}
