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
    public class WarningsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;

        public WarningsController(Ksans_SportsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PersonnelWarnings()
        {
            return View();
        }



        // GET: Warnings
        public async Task<IActionResult> Index()
        {
            var ksans_SportsDbContext = _context.Warnings.Include(w => w.CreatedBy).Include(w => w.ModifiedBy);
            return View(await ksans_SportsDbContext.ToListAsync());
        }

        // GET: Warnings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Warnings == null)
            {
                return NotFound();
            }

            var warning = await _context.Warnings
                .Include(w => w.CreatedBy)
                .Include(w => w.ModifiedBy)
                .FirstOrDefaultAsync(m => m.WarningId == id);
            if (warning == null)
            {
                return NotFound();
            }

            return View(warning);
        }

        // GET: Warnings/Create
        public IActionResult Create()
        {
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id");
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id");
            return View();
        }

        // POST: Warnings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WarningId,UserId,Description,CreatedDateTime,CreatedById,ModifiedById,ModifiedDateTime,ExpiryDate,NumberOfOffences,Status")] Warning warning)
        {
            if (ModelState.IsValid)
            {
                _context.Add(warning);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", warning.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id", warning.ModifiedById);
            return View(warning);
        }

        // GET: Warnings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Warnings == null)
            {
                return NotFound();
            }

            var warning = await _context.Warnings.FindAsync(id);
            if (warning == null)
            {
                return NotFound();
            }
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", warning.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id", warning.ModifiedById);
            return View(warning);
        }

        // POST: Warnings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WarningId,UserId,Description,CreatedDateTime,CreatedById,ModifiedById,ModifiedDateTime,ExpiryDate,NumberOfOffences,Status")] Warning warning)
        {
            if (id != warning.WarningId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warning);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarningExists(warning.WarningId))
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
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", warning.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.SystemUsers, "Id", "Id", warning.ModifiedById);
            return View(warning);
        }

        // GET: Warnings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Warnings == null)
            {
                return NotFound();
            }

            var warning = await _context.Warnings
                .Include(w => w.CreatedBy)
                .Include(w => w.ModifiedBy)
                .FirstOrDefaultAsync(m => m.WarningId == id);
            if (warning == null)
            {
                return NotFound();
            }

            return View(warning);
        }

        // POST: Warnings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Warnings == null)
            {
                return Problem("Entity set 'Ksans_SportsDbContext.Warnings'  is null.");
            }
            var warning = await _context.Warnings.FindAsync(id);
            if (warning != null)
            {
                _context.Warnings.Remove(warning);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WarningExists(int id)
        {
          return (_context.Warnings?.Any(e => e.WarningId == id)).GetValueOrDefault();
        }
    }
}
