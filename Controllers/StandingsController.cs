using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;

namespace MyField.Controllers
{
    public class StandingsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public StandingsController(Ksans_SportsDbContext context, 
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            EmailService emailService,
            IEncryptionService encryptionService,
            RequestLogService requestLogService)
        {
            _context = context;
            _userManager = userManager; 
            _activityLogger = activityLogger;
            _emailService =  emailService;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> StandingsBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }

            var standings = _context.Standing
                .Where(s => s.LeagueId == currentLeague.LeagueId && 
                s.DivisionId == divisionId)
                .Include(s => s.Club)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenBy(s => s.MatchPlayed)
                .ToList();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.CurrentSeason = currentSeason.LeagueYears;

            return View(standings);
        }

        [Authorize(Roles =("Sport Administrator"))]
        public async Task<IActionResult> Standings()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }

            var standings= _context.Standing
                .Where(s => s.League.IsCurrent && s.DivisionId == divisionId)
                .Include(s => s.Club)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenBy(s => s.MatchPlayed)
                .ToList();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.CurrentSeason = currentSeason.LeagueYears;

            return View(standings);
        }

        public async Task<IActionResult> StandingsMain(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            ViewBag.Leagues = await _context.League
                .Where(l => l.DivisionId == decryptedDivisionId)
                .OrderByDescending(l => l.CreatedDateTime)
                .ToListAsync();

            ViewBag.DivisionId = divisionId;

            return PartialView("_StandingsPartial");
        }

        public async Task<IActionResult> StandingsTable(int? leagueId, string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var standings = await _context.Standing
                .Where(s => s.LeagueId == leagueId &&
                s.DivisionId == decryptedDivisionId && 
                s.League.DivisionId == decryptedDivisionId)
                .Include(s => s.Club)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenBy(s => s.MatchPlayed)
                .ToListAsync();

            return PartialView("_StandingsTablePartial", standings);
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> BackOfficeStandings()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }
            var standings = _context.Standing
                .Where(f => f.LeagueId == currentLeague.LeagueId && f.DivisionId == divisionId)
                                  .Include(s => s.Club)
                                  .OrderByDescending(s => s.Points)
                                  .ThenByDescending(s => s.GoalDifference)
                                  .ThenBy(s => s.MatchPlayed);

            var currentSeason = await _context.League
               .Where(c => c.IsCurrent)
               .FirstOrDefaultAsync();


            ViewBag.CurrentSeason = currentSeason.LeagueYears;

            return PartialView("_BackOfficeStandingsPartial", await standings.ToListAsync());
        }


        [Authorize]
        public async Task<IActionResult> ClubStandings(int? clubId)
        {
            var club = await _context.Club
              .Where(c => c.ClubId == clubId)
              .Include(c => c.Division)
              .FirstOrDefaultAsync();

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == club.DivisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }

            var standings = _context.Standing
                .Where(f => f.LeagueId == currentLeague.LeagueId && f.DivisionId == club.DivisionId)
                                  .Include(s => s.Club)
                                  .OrderByDescending(s => s.Points)
                                  .ThenByDescending(s => s.GoalDifference)
                                  .ThenBy(s => s.MatchPlayed);


            ViewBag.SelectedClubId = clubId;

            return PartialView("_ClubStandingPartial", await standings.ToListAsync());
        }


        [Authorize(Roles =("Sport Administrator"))]
        public async Task<IActionResult> EditPoints(string standingId)
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var logMessages = new List<string>();

            var decryptedStandingId = _encryptionService.DecryptToInt(standingId);

            if (decryptedStandingId == null)
            {
                var message = "EditPoints called with null id";
                Console.WriteLine(message);
                logMessages.Add(message);
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var standing = await _context.Standing
                                        .Where(s => s.DivisionId == divisionId)
                                        .Include(s => s.Club)
                                        .FirstOrDefaultAsync(s => s.StandingId == decryptedStandingId);

            if (standing == null)
            {
                var message = $"Standing with id {decryptedStandingId} not found";
                Console.WriteLine(message);
                logMessages.Add(message);
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var viewModel = new StandingPointsViewModel
            {
                StandingId = decryptedStandingId,
                Points = standing.Points,
                Goals = standing.GoalDifference,
                ClubName = standing.Club?.ClubName,
                ClubBadge = standing.Club?.ClubBadge,
                Reason = standing.Reason,
            };

            ViewBag.ClubName = standing?.Club?.ClubName;

            TempData["LogMessages"] = logMessages;
            return View(viewModel);
        }


        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPoints(StandingPointsViewModel viewModel)
        {
            var logMessages = new List<string>();


            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var standing = await _context.Standing
                .Where(s => s.DivisionId == divisionId)
                .Include(s => s.Club)
                .FirstOrDefaultAsync(s => s.StandingId == viewModel.StandingId);

            if (standing == null)
            {
                var message = $"Standing with id {viewModel.StandingId} not found";
                Console.WriteLine(message);
                logMessages.Add(message);
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var clubName = standing.Club.ClubName;

            if (!string.IsNullOrEmpty(viewModel.ClubCode) && standing.Club.ClubCode != viewModel.ClubCode)
            {
                TempData["Message"] = $"The club code you have provided does not match {clubName} code. Please try again!";
                ViewBag.ClubName = standing?.Club?.ClubName;
                viewModel.ClubBadge = standing?.Club.ClubBadge;
                viewModel.Points = standing.Points;
                viewModel.Goals = standing.GoalDifference;
                TempData["LogMessages"] = logMessages;

                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                var userId = user.Id;

                var originalPoints = standing.Points;
                var originalGoals = standing.GoalDifference;

                standing.Reason = viewModel.Reason;
                standing.ModifiedDateTime = DateTime.Now;
                standing.ModifiedById = userId;

                if (viewModel.PointsToBeAdded != 0)
                {
                    standing.Points = standing.Points + viewModel.PointsToBeAdded;
                }
                else if (viewModel.PointsToBeSubtracted != 0)
                {
                    standing.Points = standing.Points - viewModel.PointsToBeSubtracted;
                }

                if (viewModel.GoalsToBeSubtracted != 0)
                {
                    standing.GoalsConceded = standing.GoalsConceded + viewModel.GoalsToBeSubtracted;
                    standing.GoalsScored = standing.GoalsScored - viewModel.GoalsToBeSubtracted;
                }
                else if (viewModel.GoalsToBeAdded != 0)
                {
                    standing.GoalsScored = standing.GoalsScored + viewModel.GoalsToBeAdded;
                }

                standing.GoalDifference = standing.GoalsScored - standing.GoalsConceded;

                viewModel.Points = standing.Points;
                viewModel.Goals = standing.GoalDifference;

                TempData["Message"] = $"You have successfully updated standings for {standing.Club.ClubName} and the new points are {viewModel.Points}";

                TempData["LogMessages"] = logMessages;

                await _activityLogger.Log($"Modified {standing.Club.ClubName} points from {originalPoints} to {standing.Points} and goals from {originalGoals} to {standing.GoalDifference}", user.Id);
                await _requestLogService.LogSuceededRequest("Club pointes modified successfully.", StatusCodes.Status200OK);

                var clubEmail = standing.Club.Email; 
                var subject = "Standings Updated Notification";
                var body = $@"
            Dear {standing.Club.ClubName},<br/><br/>
            Please note that your standings have been updated and your new standings are:<br/><br/>
            Points: {standing.Points}<br/>
            Goal Difference: {standing.GoalDifference}<br/><br/>
            Reason for updating: {viewModel.Reason}.<br/><br/>
            Please check the standings tables for updated standings.<br/><br/>
            If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
            Regards,<br/>
            Diski360 Management
                ";

                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(clubEmail, subject, body, "Diski 360"));

                return RedirectToAction(nameof(Standings));
            }

            var existingStandings = await _context.Standing.Where(es => es.StandingId == viewModel.StandingId).Include(es => es.Club).FirstOrDefaultAsync();


            viewModel.ClubName = existingStandings.Club.ClubName;
            viewModel.ClubBadge = existingStandings.Club.ClubBadge;
            viewModel.Points = existingStandings.Points;
            viewModel.Goals = existingStandings.GoalDifference;

            return View(viewModel);
        }

        private bool StandingExists(int id)
        {
            return _context.Standing.Any(e => e.StandingId == id);
        }
    }
}
