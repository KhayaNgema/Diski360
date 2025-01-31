using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;

namespace MyField.Controllers
{
    public class ClubWarningsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;

        public ClubWarningsController(Ksans_SportsDbContext context)
        {
            _context = context;
        }

        // GET: ClubWarnings
        public async Task<IActionResult> Index()
        {
            var ksans_SportsDbContext = _context.ClubWarnings.Include(c => c.Club).Include(c => c.CreatedBy).Include(c => c.ModifiedBy);
            return View(await ksans_SportsDbContext.ToListAsync());
        }

        // GET: ClubWarnings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ClubWarnings == null)
            {
                return NotFound();
            }

            var clubWarning = await _context.ClubWarnings
                .Include(c => c.Club)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .FirstOrDefaultAsync(m => m.WarningId == id);
            if (clubWarning == null)
            {
                return NotFound();
            }

            return View(clubWarning);
        }

        // GET: ClubWarnings/Create
        public IActionResult Create()
        {
            ViewData["CLubId"] = new SelectList(_context.Club, "ClubId", "ClubLocation");
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id");
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id");
            return View();
        }

        // POST: ClubWarnings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WarningId,CLubId,Description,CreatedDateTime,CreatedById,ModifiedById,ModifiedDateTime,ExpiryDate,NumberOfOffences,Status")] ClubWarning clubWarning)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clubWarning);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CLubId"] = new SelectList(_context.Club, "ClubId", "ClubLocation", clubWarning.CLubId);
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", clubWarning.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id", clubWarning.ModifiedById);
            return View(clubWarning);
        }

        // GET: ClubWarnings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ClubWarnings == null)
            {
                return NotFound();
            }

            var clubWarning = await _context.ClubWarnings.FindAsync(id);
            if (clubWarning == null)
            {
                return NotFound();
            }
            ViewData["CLubId"] = new SelectList(_context.Club, "ClubId", "ClubLocation", clubWarning.CLubId);
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", clubWarning.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id", clubWarning.ModifiedById);
            return View(clubWarning);
        }

        // POST: ClubWarnings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WarningId,CLubId,Description,CreatedDateTime,CreatedById,ModifiedById,ModifiedDateTime,ExpiryDate,NumberOfOffences,Status")] ClubWarning clubWarning)
        {
            if (id != clubWarning.WarningId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clubWarning);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubWarningExists(clubWarning.WarningId))
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
            ViewData["CLubId"] = new SelectList(_context.Club, "ClubId", "ClubLocation", clubWarning.CLubId);
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", clubWarning.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id", clubWarning.ModifiedById);
            return View(clubWarning);
        }

        // GET: ClubWarnings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ClubWarnings == null)
            {
                return NotFound();
            }

            var clubWarning = await _context.ClubWarnings
                .Include(c => c.Club)
                .Include(c => c.CreatedBy)
                .Include(c => c.ModifiedBy)
                .FirstOrDefaultAsync(m => m.WarningId == id);
            if (clubWarning == null)
            {
                return NotFound();
            }

            return View(clubWarning);
        }

        // POST: ClubWarnings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ClubWarnings == null)
            {
                return Problem("Entity set 'Ksans_SportsDbContext.ClubWarnings'  is null.");
            }
            var clubWarning = await _context.ClubWarnings.FindAsync(id);
            if (clubWarning != null)
            {
                _context.ClubWarnings.Remove(clubWarning);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClubWarningExists(int id)
        {
          return (_context.ClubWarnings?.Any(e => e.WarningId == id)).GetValueOrDefault();
        }
    }
}
