using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EsperantOS.BusinessLogic;
using EsperantOS.Models;
using EsperantOS.Extensions;
using EsperantOS.Helpers;
using EsperantOS.DTO.Model;

namespace EsperantOS.Controllers
{
    // Håndterer al vagtrelateret funktionalitet: visning, oprettelse, redigering, sletning,
    // frigivelse og overtagelse af vagter.
    // [Authorize] kræver at brugeren er logget ind for at tilgå nogen af disse sider.
    [Authorize]
    public class VagtsController : Controller
    {
        private readonly VagtBLL _vagtBLL;
        private readonly MedarbejderBLL _medarbejderBLL;

        // Konstruktør – begge BLL-klasser injiceres automatisk af ASP.NET Core
        public VagtsController(VagtBLL vagtBLL, MedarbejderBLL medarbejderBLL)
        {
            _vagtBLL = vagtBLL;
            _medarbejderBLL = medarbejderBLL;
        }

        // GET: /Vagts/Index
        // Viser vagtplanen med alle fredagsvagter.
        // Kalder EnsureAedruVagterAsync for at sikre at der altid findes en ædru-vagt pr. fredag.
        public async Task<IActionResult> Index()
        {
            // Sørg for at alle fredage har en ædru-vagt – opretter automatisk hvis den mangler
            await _vagtBLL.EnsureAedruVagterAsync();

            // Hent alle fredagsvagter og sorter kronologisk
            var vagterDto = await _vagtBLL.GetFridayVagterAsync();
            var vagter = vagterDto.ToModelList().OrderBy(v => v.Dato).ToList();

            return View(vagter);
        }

        // GET: /Vagts/Details/5
        // Viser detaljer for én bestemt vagt
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id.Value);
            if (vagtDto == null) return NotFound();

            return View(vagtDto.ToModel());
        }

        // GET: /Vagts/Create
        // Viser formularen til oprettelse af en ny vagt.
        // Fylder dropdown-lister med de næste 10 fredage og de tre faste tidspunkter.
        public IActionResult Create()
        {
            ViewBag.UpcomingFridays = VagtHelpers.GetUpcomingFridaysSelectList();
            ViewBag.Times = VagtHelpers.GetShiftTimesSelectList();
            return View();
        }

        // POST: /Vagts/Create
        // Behandler formularen og opretter en ny vagt i databasen.
        // Dato og tid sendes som separate felter og kombineres her.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DateTime SelectedDate, string SelectedTime, bool Ædru, bool Frigivet)
        {
            // Validér at den valgte dato er en fredag (vagter må kun ligge på fredage)
            if (SelectedDate.DayOfWeek != DayOfWeek.Friday)
            {
                ModelState.AddModelError("Dato", "Vagter kan kun oprettes på fredage.");
            }

            if (ModelState.IsValid)
            {
                // Kombiner dato og tidspunkt til én DateTime-værdi
                // GetShiftDateTime håndterer specialtilfældet med 00:00 (lørdag)
                var date = VagtHelpers.GetShiftDateTime(SelectedDate, SelectedTime);

                var vagtDto = new VagtDTO
                {
                    Dato = date,
                    Ædru = Ædru,
                    Frigivet = Frigivet,
                    Medarbejdere = new List<MedarbejderDTO>() // Ingen medarbejdere ved oprettelse
                };

                await _vagtBLL.CreateVagtAsync(vagtDto);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Create));
        }

        // GET: /Vagts/Edit/5
        // Viser redigeringsformularen for en eksisterende vagt.
        // Forudfylder dropdown-listerne med vagtens nuværende dato og tidspunkt.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id.Value);
            if (vagtDto == null) return NotFound();

            // Send dropdowns til viewet med den nuværende vagt forudvalgt
            ViewBag.UpcomingFridays = VagtHelpers.GetUpcomingFridaysSelectList(vagtDto.Dato);
            ViewBag.Times = VagtHelpers.GetShiftTimesSelectList(vagtDto.Dato);

            return View(vagtDto.ToModel());
        }

        // POST: /Vagts/Edit/5
        // Gemmer ændringerne til en eksisterende vagt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DateTime SelectedDate, string SelectedTime, bool Ædru, bool Frigivet)
        {
            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id);
            if (vagtDto == null) return NotFound();

            // Validér at datoen stadig er en fredag
            if (SelectedDate.DayOfWeek != DayOfWeek.Friday)
            {
                ModelState.AddModelError("Dato", "Vagter kan kun placeres på fredage.");
            }

            if (ModelState.IsValid)
            {
                // Opdater DTO med de nye værdier fra formularen
                vagtDto.Dato = VagtHelpers.GetShiftDateTime(SelectedDate, SelectedTime);
                vagtDto.Ædru = Ædru;
                vagtDto.Frigivet = Frigivet;

                await _vagtBLL.UpdateVagtAsync(vagtDto);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Edit), new { id = id });
        }

        // POST: /Vagts/Frigiv/5
        // Markerer en vagt som frigivet – den er nu ledig for andre at tage.
        // Kaldes via JavaScript fetch (ingen sidenavigation) og returnerer HTTP 200 OK.
        [HttpPost]
        public async Task<IActionResult> Frigiv(int id)
        {
            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id);
            if (vagtDto == null) return NotFound();

            // Sæt frigivet-flaget og gem
            vagtDto.Frigivet = true;
            await _vagtBLL.UpdateVagtAsync(vagtDto);

            return Ok(); // JavaScript-koden genindlæser siden ved Ok-svar
        }

        // POST: /Vagts/TagVagt/5
        // Den indloggede bruger overtager en frigivet vagt.
        // Fjerner den tidligere ejer, markerer vagten som ikke-frigivet,
        // og tilknytter den nuværende bruger.
        // Hvis brugeren ikke eksisterer i databasen, oprettes de automatisk.
        [HttpPost]
        public async Task<IActionResult> TagVagt(int id)
        {
            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id);
            if (vagtDto == null) return NotFound();

            // Nulstil vagten: ikke frigivet længere, og fjern tidligere medarbejder
            vagtDto.Frigivet = false;
            vagtDto.Medarbejdere.Clear();

            // Hent den nuværende brugers navn fra login-cookien
            var currentUserName = User.Identity?.Name ?? "Ukendt Frivillig";

            // Slå brugeren op i databasen
            var medarbejderDto = await _medarbejderBLL.GetMedarbejderByNameAsync(currentUserName);

            if (medarbejderDto == null)
            {
                // Brugeren eksisterer ikke i databasen – opret dem automatisk som almindelig medarbejder
                await _medarbejderBLL.CreateMedarbejderAsync(new MedarbejderDTO
                {
                    Name = currentUserName,
                    Bestyrelsesmedlem = false
                });
                medarbejderDto = await _medarbejderBLL.GetMedarbejderByNameAsync(currentUserName);
            }

            // Tilknyt den nuværende bruger til vagten og gem
            vagtDto.Medarbejdere.Add(medarbejderDto!);
            await _vagtBLL.UpdateVagtAsync(vagtDto);

            return Ok(); // JavaScript-koden genindlæser siden ved Ok-svar
        }

        // GET: /Vagts/Delete/5
        // Viser bekræftelsessiden for sletning af en vagt
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id.Value);
            if (vagtDto == null) return NotFound();

            return View(vagtDto.ToModel());
        }

        // POST: /Vagts/Delete/5
        // Udfører den endelige sletning efter brugerens bekræftelse.
        // ActionName("Delete") sikrer at denne POST-metode bruger samme URL som GET-versionen.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _vagtBLL.VagtExistsAsync(id))
            {
                await _vagtBLL.DeleteVagtAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
