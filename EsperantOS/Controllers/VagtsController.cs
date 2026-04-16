using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EsperantOS.Data;
using EsperantOS.Models;

namespace EsperantOS.Controllers
{
    [Authorize]
    public class VagtsController : Controller
    {
        private readonly EsperantOSContext _context;

        public VagtsController(EsperantOSContext context)
        {
            _context = context;
        }

        // GET: Vagts
        public async Task<IActionResult> Index()
        {
            var vagter = await _context.Vagter
                .Include(v => v.Medarbejdere)
                .ToListAsync();

            // Kun tillad fredage
            vagter = vagter.Where(v => v.Dato.DayOfWeek == DayOfWeek.Friday).ToList();

            var uniqueDates = vagter.Select(v => v.Dato.Date).Distinct().ToList();
            bool changesMade = false;

            // Sørg for at der altid er oprettet mindst én ædru vagt pr. fredag
            foreach (var d in uniqueDates)
            {
                if (!vagter.Any(v => v.Dato.Date == d && v.Ædru))
                {
                    // Opret en frigivet ædru vagt for datoen kl 20.00
                    var nyVagt = new Vagt 
                    { 
                        Dato = d.AddHours(20), 
                        Ædru = true, 
                        Frigivet = true, 
                        Medarbejdere = new List<Medarbejder>() 
                    };
                    _context.Vagter.Add(nyVagt);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _context.SaveChangesAsync();
                var allVagter = await _context.Vagter
                    .Include(v => v.Medarbejdere)
                    .ToListAsync();
                    
                vagter = allVagter.Where(v => v.Dato.DayOfWeek == DayOfWeek.Friday).ToList();
            }

            return View(vagter.OrderBy(v => v.Dato).ToList());
        }

        // GET: Vagts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vagt = await _context.Vagter
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vagt == null)
            {
                return NotFound();
            }

            return View(vagt);
        }

        // GET: Vagts/Create
        public IActionResult Create()
        {
            var upcomingFridays = new List<SelectListItem>();
            var date = DateTime.Today;
            while (date.DayOfWeek != DayOfWeek.Friday) date = date.AddDays(1);
            for (int i = 0; i < 10; i++)
            {
                upcomingFridays.Add(new SelectListItem { Text = date.ToString("dd. MMMM yyyy"), Value = date.ToString("yyyy-MM-dd") });
                date = date.AddDays(7);
            }
            ViewBag.UpcomingFridays = upcomingFridays;

            var times = new List<SelectListItem>
            {
                new SelectListItem { Text = "16:00 - 20:00", Value = "16:00:00" },
                new SelectListItem { Text = "20:00 - 00:00", Value = "20:00:00" },
                new SelectListItem { Text = "00:00 - 02:00", Value = "00:00:00" } // Nattevagt Starter typisk fredag midnat/lørdag
            };
            ViewBag.Times = times;

            return View();
        }

        // POST: Vagts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DateTime SelectedDate, string SelectedTime, bool Ædru, bool Frigivet)
        {
            var timeSpan = TimeSpan.Parse(SelectedTime);
            var date = SelectedDate.Date.Add(timeSpan);
            
            // Hvis time er 00:00 er vagten faktisk rykket til lørdag, men vi kapper den ned til fredag i systemet, eller lader den være nat
            if (timeSpan.Hours == 0)
            {
                date = date.AddDays(1);
            }

            var vagt = new Vagt
            {
                Dato = date,
                Ædru = Ædru,
                Frigivet = Frigivet,
                Medarbejdere = new List<Medarbejder>()
            };

            if (SelectedDate.DayOfWeek != DayOfWeek.Friday)
            {
                ModelState.AddModelError("Dato", "Vagter kan kun oprettes på fredage.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(vagt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Create));
        }

        // GET: Vagts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vagt = await _context.Vagter.FindAsync(id);
            if (vagt == null)
            {
                return NotFound();
            }
            
            var upcomingFridays = new List<SelectListItem>();
            var date = DateTime.Today;
            while (date.DayOfWeek != DayOfWeek.Friday) date = date.AddDays(1);
            for (int i = 0; i < 10; i++)
            {
                bool isSelected = vagt.Dato.Date == date.Date || (vagt.Dato.Hour == 0 && vagt.Dato.Date == date.AddDays(1).Date);
                upcomingFridays.Add(new SelectListItem { Text = date.ToString("dd. MMMM yyyy"), Value = date.ToString("yyyy-MM-dd"), Selected = isSelected });
                date = date.AddDays(7);
            }
            ViewBag.UpcomingFridays = upcomingFridays;

            string currentTimeVal = vagt.Dato.Hour == 16 ? "16:00:00" : (vagt.Dato.Hour == 20 ? "20:00:00" : "00:00:00");
            var times = new List<SelectListItem>
            {
                new SelectListItem { Text = "16:00 - 20:00", Value = "16:00:00", Selected = currentTimeVal == "16:00:00" },
                new SelectListItem { Text = "20:00 - 00:00", Value = "20:00:00", Selected = currentTimeVal == "20:00:00" },
                new SelectListItem { Text = "00:00 - 02:00", Value = "00:00:00", Selected = currentTimeVal == "00:00:00" }
            };
            ViewBag.Times = times;

            return View(vagt);
        }

        // POST: Vagts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DateTime SelectedDate, string SelectedTime, bool Ædru, bool Frigivet)
        {
            var vagt = await _context.Vagter.FindAsync(id);
            if (vagt == null)
            {
                return NotFound();
            }

            var timeSpan = TimeSpan.Parse(SelectedTime);
            var date = SelectedDate.Date.Add(timeSpan);
            
            if (timeSpan.Hours == 0)
            {
                date = date.AddDays(1);
            }

            vagt.Dato = date;
            vagt.Ædru = Ædru;
            vagt.Frigivet = Frigivet;

            if (SelectedDate.DayOfWeek != DayOfWeek.Friday)
            {
                ModelState.AddModelError("Dato", "Vagter kan kun placeres på fredage.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vagt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VagtExists(vagt.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        // POST: Vagts/Frigiv/5
        [HttpPost]
        public async Task<IActionResult> Frigiv(int id)
        {
            var vagt = await _context.Vagter.FindAsync(id);
            if (vagt == null)
            {
                return NotFound();
            }

            vagt.Frigivet = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: Vagts/TagVagt/5
        [HttpPost]
        public async Task<IActionResult> TagVagt(int id)
        {
            var vagt = await _context.Vagter
                .Include(v => v.Medarbejdere) // Inkludér medarbejdere for at kunne fjerne dem
                .FirstOrDefaultAsync(v => v.Id == id);
                
            if (vagt == null)
            {
                return NotFound();
            }

            vagt.Frigivet = false;

            // Fjern eksisterende medarbejdere fra vagten
            vagt.Medarbejdere.Clear();
            
            // Find den bruger, der er logget ind (baseret på Auth Cookie claims)
            var currentUserName = User.Identity?.Name ?? "Ukendt Frivillig";
            
            var medarbejder = await _context.Medarbejdere.FirstOrDefaultAsync(m => m.Name == currentUserName);
            
            // Hvis medarbejderen ikke eksisterer i databasen, opretter vi dem for en sikkerheds skyld (ideelt set er de der).
            if (medarbejder == null)
            {
                medarbejder = new Medarbejder { Name = currentUserName, Bestyrelsesmedlem = false };
                _context.Medarbejdere.Add(medarbejder);
            }
            
            // Tilføj den nye bruger til vagten
            vagt.Medarbejdere.Add(medarbejder);
            
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: Vagts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vagt = await _context.Vagter
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vagt == null)
            {
                return NotFound();
            }

            return View(vagt);
        }

        // POST: Vagts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vagt = await _context.Vagter.FindAsync(id);
            if (vagt != null)
            {
                _context.Vagter.Remove(vagt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VagtExists(int id)
        {
            return _context.Vagter.Any(e => e.Id == id);
        }
    }
}
