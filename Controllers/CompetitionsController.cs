using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;

namespace MyField.Controllers
{
    public class CompetitionsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;


        public CompetitionsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Leaderboard()
        {
            var leaderboard = await _context.CompetitionParticipants
                .Where(lb => lb.Competition.CompetitionStatus == CompetitionStatus.Current)
                .Include(lb => lb.Participant)
                .OrderByDescending(lb => lb.Points)
                .ToListAsync(); 

            var currentCompetiton = await _context.Competition
                .Where(cc => cc.CompetitionStatus == CompetitionStatus.Current)
                .FirstOrDefaultAsync();

            ViewBag.CompetitionRound = currentCompetiton.Month.ToString("MMMM");

            return View(leaderboard);
        }

        public async Task<IActionResult> Competitions()
        {
            var competitions = await _context.Competition
                .Include(lb => lb.Winner)
                .OrderByDescending(lb => lb.Month)
                .ToListAsync();

            return View(competitions);
        }
    }
}
