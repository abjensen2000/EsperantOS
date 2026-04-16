using EsperantOS.Data;
using EsperantOS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EsperantOS.Controllers
{
    public class HomeController : Controller
    {
        private EsperantOSContext _context;

        public HomeController(EsperantOSContext context)
        {
            this._context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Vagtplan()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FrigivedeVagter()
        {
            return View();
        }




    }
}
