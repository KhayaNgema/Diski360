using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;

namespace MyField.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        public readonly UserManager<UserBaseModel> _userManager;

        public TournamentsController(Ksans_SportsDbContext context, UserManager<UserBaseModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> FindTournaments()
        {
            var tournaments = await _context.Tournament
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
                .ToListAsync();
            return PartialView("_TournamentsPartial", tournaments);
        }


        public async Task<IActionResult> Tournaments()
        {
            return PartialView("TournamentsPartial");
        }

        public async Task<IActionResult> ManageTournaments()
        {
            var user = await _userManager.GetUserAsync(User);

            var tournaments = await _context.Tournament
                .Where(t => t.CreatedById == user.Id)
                .ToListAsync();

            return View(tournaments);
        }

        // GET: Tournaments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tournament == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournament
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
                .FirstOrDefaultAsync(m => m.TournamentId == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        // GET: Tournaments/Create
        public IActionResult Create()
        {
            ViewData["CreatedById"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ModifiedById"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Tournaments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TournamentId,TournamentName,TournamentDescription,TournamentType,StartDate,EndDate,TournamentOrgarnizer,JoiningFee,TournamentRules,TournamentStatus,TournamentLocation,CreatedDateTime,ModifiedDateTime,CreatedById,ModifiedById")] Tournament tournament)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tournament);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedById"] = new SelectList(_context.Users, "Id", "Id", tournament.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.Users, "Id", "Id", tournament.ModifiedById);
            return View(tournament);
        }

        // GET: Tournaments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tournament == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournament.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            ViewData["CreatedById"] = new SelectList(_context.Users, "Id", "Id", tournament.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.Users, "Id", "Id", tournament.ModifiedById);
            return View(tournament);
        }

        // POST: Tournaments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TournamentId,TournamentName,TournamentDescription,TournamentType,StartDate,EndDate,TournamentOrgarnizer,JoiningFee,TournamentRules,TournamentStatus,TournamentLocation,CreatedDateTime,ModifiedDateTime,CreatedById,ModifiedById")] Tournament tournament)
        {
            if (id != tournament.TournamentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.TournamentId))
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
            ViewData["CreatedById"] = new SelectList(_context.Users, "Id", "Id", tournament.CreatedById);
            ViewData["ModifiedById"] = new SelectList(_context.Users, "Id", "Id", tournament.ModifiedById);
            return View(tournament);
        }

        // GET: Tournaments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tournament == null)
            {
                return NotFound();
            }

            var tournament = await _context.Tournament
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
                .FirstOrDefaultAsync(m => m.TournamentId == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        // POST: Tournaments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tournament == null)
            {
                return Problem("Entity set 'Sports_ManagerDbContext.Tournament'  is null.");
            }
            var tournament = await _context.Tournament.FindAsync(id);
            if (tournament != null)
            {
                _context.Tournament.Remove(tournament);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TournamentExists(int id)
        {
          return (_context.Tournament?.Any(e => e.TournamentId == id)).GetValueOrDefault();
        }
    }
}
