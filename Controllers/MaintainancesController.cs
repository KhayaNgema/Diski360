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
    public class MaintainancesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;

        public MaintainancesController(Ksans_SportsDbContext context)
        {
            _context = context;
        }

        // GET: Maintainances
        public async Task<IActionResult> Index()
        {
            var ksans_SportsDbContext = _context.Maintainances.Include(m => m.CreatedBy).Include(m => m.ResolvedBy);
            return View(await ksans_SportsDbContext.ToListAsync());
        }

        // GET: Maintainances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Maintainances == null)
            {
                return NotFound();
            }

            var maintainance = await _context.Maintainances
                .Include(m => m.CreatedBy)
                .Include(m => m.ResolvedBy)
                .FirstOrDefaultAsync(m => m.MaintainanceId == id);
            if (maintainance == null)
            {
                return NotFound();
            }

            return View(maintainance);
        }

        // GET: Maintainances/Create
        public IActionResult Create()
        {
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id");
            ViewData["ResolvedById"] = new SelectList(_context.SystemUsers, "Id", "Id");
            return View();
        }

        // POST: Maintainances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaintainanceId,CreatedById,CreatedDateTime,ResolvedById,MaintainanceDetails,maintainanceRequestStatus")] Maintainance maintainance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(maintainance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", maintainance.CreatedById);
            ViewData["ResolvedById"] = new SelectList(_context.SystemUsers, "Id", "Id", maintainance.ResolvedById);
            return View(maintainance);
        }

        // GET: Maintainances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Maintainances == null)
            {
                return NotFound();
            }

            var maintainance = await _context.Maintainances.FindAsync(id);
            if (maintainance == null)
            {
                return NotFound();
            }
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", maintainance.CreatedById);
            ViewData["ResolvedById"] = new SelectList(_context.SystemUsers, "Id", "Id", maintainance.ResolvedById);
            return View(maintainance);
        }

        // POST: Maintainances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaintainanceId,CreatedById,CreatedDateTime,ResolvedById,MaintainanceDetails,maintainanceRequestStatus")] Maintainance maintainance)
        {
            if (id != maintainance.MaintainanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintainance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintainanceExists(maintainance.MaintainanceId))
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
            ViewData["CreatedById"] = new SelectList(_context.SystemUsers, "Id", "Id", maintainance.CreatedById);
            ViewData["ResolvedById"] = new SelectList(_context.SystemUsers, "Id", "Id", maintainance.ResolvedById);
            return View(maintainance);
        }

        // GET: Maintainances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Maintainances == null)
            {
                return NotFound();
            }

            var maintainance = await _context.Maintainances
                .Include(m => m.CreatedBy)
                .Include(m => m.ResolvedBy)
                .FirstOrDefaultAsync(m => m.MaintainanceId == id);
            if (maintainance == null)
            {
                return NotFound();
            }

            return View(maintainance);
        }

        // POST: Maintainances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Maintainances == null)
            {
                return Problem("Entity set 'Ksans_SportsDbContext.Maintainances'  is null.");
            }
            var maintainance = await _context.Maintainances.FindAsync(id);
            if (maintainance != null)
            {
                _context.Maintainances.Remove(maintainance);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaintainanceExists(int id)
        {
          return (_context.Maintainances?.Any(e => e.MaintainanceId == id)).GetValueOrDefault();
        }
    }
}
