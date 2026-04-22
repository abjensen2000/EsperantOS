using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;



using DataAccess.Repositories;
using DataAccess.Model;
using DTO.Model;

namespace EsperantOS.Controllers
{
    public class HomeController : Controller
    {
        private UnitOfWork _uow;

        public HomeController(UnitOfWork uow)
        {
            this._uow = uow;
        }

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
            //VagtDTO vagt = new VagtDTO(0, DateTime.Now, false, false);
            //vagt.Medarbejdere.Add(new MedarbejderDTO(0, "sammi", true));
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
