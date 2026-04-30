using Microsoft.AspNetCore.Authorization;
using EsperantOS.BusinessLogic;
using EsperantOS.Models;
using EsperantOS.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EsperantOS.Controllers
{
    // Håndterer forsiden (dashboard) i applikationen.
    // [Authorize] sikrer at man skal være logget ind for at tilgå denne side.
    [Authorize]
    public class HomeController : Controller
    {
        private readonly VagtBLL _vagtBLL;
        private readonly MedarbejderBLL _medarbejderBLL;

        // Konstruktør – begge BLL-klasser injiceres automatisk af ASP.NET Core
        public HomeController(VagtBLL vagtBLL, MedarbejderBLL medarbejderBLL)
        {
            _vagtBLL = vagtBLL;
            _medarbejderBLL = medarbejderBLL;
        }

        // GET: /Home/Index
        // Viser forsiden med den indloggede brugers kommende vagter og bestyrelsesbeskeder.
        public async Task<IActionResult> Index()
        {
            // Hent brugernavnet fra login-cookien (svarende til Medarbejder.Name i databasen)
            var currentUser = User.Identity?.Name ?? "Ukendt";

            // Hent brugerens vagter fra BLL via navn – returnerer DTO-liste
            var mineVagterDto = await _vagtBLL.GetVagterByMedarbejderNameAsync(currentUser);

            // Konverter DTO-listen til Model-objekter som viewet kan vise
            // Sorter kronologisk så de næste vagter vises øverst
            var mineVagter = mineVagterDto.ToModelList().OrderBy(v => v.Dato).ToList();

            // Byg ViewModel med al data som forsiden har brug for
            var viewModel = new HomeViewModel
            {
                VelkomstBesked = "Velkommen til Esperanto!",
                BestyrelsesBesked = "Husk at tjekke frigivede vagter. Vi mangler folk til fredagsbaren d. 25.!",
                MineVagter = mineVagter
            };

            return View(viewModel);
        }
    }
}
