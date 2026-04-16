using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EsperantOS.Data;
using EsperantOS.Models;

namespace EsperantOS.Controllers
{
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
            return View(await _context.Vagter.ToListAsync());
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
            return View();
        }

        // POST: Vagts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Dato,Ædru,Frigivet")] Vagt vagt)
        {
            Console.WriteLine(vagt);
            if (ModelState.IsValid)
            {
                _context.Add(vagt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vagt);
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
            return View(vagt);
        }

        // POST: Vagts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Dato,Ædru,Frigivet")] Vagt vagt)
        {
            if (id != vagt.Id)
            {
                return NotFound();
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
            return View(vagt);
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
