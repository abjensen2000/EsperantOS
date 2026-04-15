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
            _context.Medarbejdere.Add(new Medarbejder(0, "Christian", false));
            _context.SaveChanges();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
