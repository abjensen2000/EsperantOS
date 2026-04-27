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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Vagtplan(int? selectedVagtId)
        {
            var vagter = _uow.GetAllVagt();
            ViewBag.vagter = vagter;

            ViewBag.medarbejdere = new List<MedarbejderDTO>();

            if (selectedVagtId.HasValue)
            {
                var vagt = _uow.GetVagt(selectedVagtId.Value);
                if( vagt != null )
                {
                    ViewBag.medarbejdere = vagt.Medarbejdere;
                }
            }

            ViewBag.alleMedarbejdere = _uow.GetAllMedarbejder();

            return View();
        }
    }
}
