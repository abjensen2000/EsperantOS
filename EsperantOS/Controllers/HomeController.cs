using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;



using DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Model;
using DTO.Model;

namespace EsperantOS.Controllers
{
    [Authorize(Policy = "Bestyrelsesmedlem")]
    public class HomeController : Controller
    {
        private UnitOfWork _uow;

        public HomeController(UnitOfWork uow)
        {
            this._uow = uow;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            //_context.Medarbejdere.Add(new Medarbejder(0, "Christian", false));
            _uow.Save();
            return View();
        }

        public IActionResult Upvote()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Vagtplan()
        {
            VagtDTO vagt = _uow.GetVagt(1);
            vagt.Medarbejdere.Add()
            //_uow.AddVagt(new VagtDTO(0, DateTime.Now, true, false));
            //_uow.Save();
            ViewBag.vagter = _uow.GetAllVagt();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Vagtplan(int vagtId)
        {
            ViewBag.medarbejdere = _uow.GetVagt(vagtId).Medarbejdere;
            return View();
        }
    }
}
