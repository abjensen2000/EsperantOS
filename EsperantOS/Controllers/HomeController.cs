using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;



using DataAccess.Repositories;

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
            return Upvote();
        }
    }
}
