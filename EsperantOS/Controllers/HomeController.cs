using Microsoft.AspNetCore.Authorization;
using EsperantOS.BusinessLogic;
using EsperantOS.Models;
using EsperantOS.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EsperantOS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly VagtBLL _vagtBLL;
        private readonly MedarbejderBLL _medarbejderBLL;

        public HomeController(VagtBLL vagtBLL, MedarbejderBLL medarbejderBLL)
        {
            _vagtBLL = vagtBLL;
            _medarbejderBLL = medarbejderBLL;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity?.Name ?? "Ukendt";

            // Hent currentUser's vagter fra BLL
            var mineVagterDto = await _vagtBLL.GetVagterByMedarbejderNameAsync(currentUser);

            // Konvertér DTOs til Models ved hjælp af extension method
            var mineVagter = mineVagterDto.ToModelList().OrderBy(v => v.Dato).ToList();

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

