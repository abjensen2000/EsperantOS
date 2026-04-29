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
    [Authorize]
    public class VagtsController : Controller
    {
        private readonly VagtBLL _vagtBLL;
        private readonly MedarbejderBLL _medarbejderBLL;

        public VagtsController(VagtBLL vagtBLL, MedarbejderBLL medarbejderBLL)
        {
            _vagtBLL = vagtBLL;
            _medarbejderBLL = medarbejderBLL;
        }

        // GET: Vagts
        public async Task<IActionResult> Index()
        {
            await _vagtBLL.EnsureAedruVagterAsync();

            var vagterDto = await _vagtBLL.GetFridayVagterAsync();
            var vagter = vagterDto.ToModelList().OrderBy(v => v.Dato).ToList();

            return View(vagter);
        }

        // GET: Vagts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id.Value);
            if (vagtDto == null)
            {
                return NotFound();
            }

            var vagt = vagtDto.ToModel();

            return View(vagt);
        }

        // GET: Vagts/Create
        public IActionResult Create()
        {
            ViewBag.UpcomingFridays = VagtHelpers.GetUpcomingFridaysSelectList();
            ViewBag.Times = VagtHelpers.GetShiftTimesSelectList();

            return View();
        }

        // POST: Vagts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DateTime SelectedDate, string SelectedTime, bool Ædru, bool Frigivet)
        {
            if (SelectedDate.DayOfWeek != DayOfWeek.Friday)
            {
                ModelState.AddModelError("Dato", "Vagter kan kun oprettes på fredage.");
            }

            if (ModelState.IsValid)
            {
                var date = VagtHelpers.GetShiftDateTime(SelectedDate, SelectedTime);

                var vagtDto = new VagtDTO
                {
                    Dato = date,
                    Ædru = Ædru,
                    Frigivet = Frigivet,
                    Medarbejdere = new List<MedarbejderDTO>()
                };

                await _vagtBLL.CreateVagtAsync(vagtDto);
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

            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id.Value);
            if (vagtDto == null)
            {
                return NotFound();
            }

            ViewBag.UpcomingFridays = VagtHelpers.GetUpcomingFridaysSelectList(vagtDto.Dato);
            ViewBag.Times = VagtHelpers.GetShiftTimesSelectList(vagtDto.Dato);

            var vagt = vagtDto.ToModel();

            return View(vagt);
        }

        // POST: Vagts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DateTime SelectedDate, string SelectedTime, bool Ædru, bool Frigivet)
        {
            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id);
            if (vagtDto == null)
            {
                return NotFound();
            }

            if (SelectedDate.DayOfWeek != DayOfWeek.Friday)
            {
                ModelState.AddModelError("Dato", "Vagter kan kun placeres på fredage.");
            }

            if (ModelState.IsValid)
            {
                var date = VagtHelpers.GetShiftDateTime(SelectedDate, SelectedTime);

                vagtDto.Dato = date;
                vagtDto.Ædru = Ædru;
                vagtDto.Frigivet = Frigivet;

                await _vagtBLL.UpdateVagtAsync(vagtDto);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Edit), new { id = id });
        }

        // POST: Vagts/Frigiv/5
        [HttpPost]
        public async Task<IActionResult> Frigiv(int id)
        {
            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id);
            if (vagtDto == null)
            {
                return NotFound();
            }

            vagtDto.Frigivet = true;
            await _vagtBLL.UpdateVagtAsync(vagtDto);

            return Ok();
        }

        // POST: Vagts/TagVagt/5
        [HttpPost]
        public async Task<IActionResult> TagVagt(int id)
        {
            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id);
            if (vagtDto == null)
            {
                return NotFound();
            }

            vagtDto.Frigivet = false;
            vagtDto.Medarbejdere.Clear();

            var currentUserName = User.Identity?.Name ?? "Ukendt Frivillig";
            var medarbejderDto = await _medarbejderBLL.GetMedarbejderByNameAsync(currentUserName);

            if (medarbejderDto == null)
            {
                await _medarbejderBLL.CreateMedarbejderAsync(new MedarbejderDTO
                {
                    Name = currentUserName,
                    Bestyrelsesmedlem = false
                });
                medarbejderDto = await _medarbejderBLL.GetMedarbejderByNameAsync(currentUserName);
            }

            vagtDto.Medarbejdere.Add(medarbejderDto!);
            await _vagtBLL.UpdateVagtAsync(vagtDto);

            return Ok();
        }

        // GET: Vagts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vagtDto = await _vagtBLL.GetVagtByIdAsync(id.Value);
            if (vagtDto == null)
            {
                return NotFound();
            }

            var vagt = vagtDto.ToModel();

            return View(vagt);
        }

        // POST: Vagts/Delete/5
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
