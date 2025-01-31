using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
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
    public class LineUpsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly RequestLogService _requestLogService;

        public LineUpsController(Ksans_SportsDbContext context,
              UserManager<UserBaseModel> userManager,
              IActivityLogger activityLogger,
              RequestLogService requestLogService)
        {
            _context = context;
            _userManager = userManager; 
            _activityLogger = activityLogger;
            _requestLogService = requestLogService;
        }

        [Authorize]
        public async Task<IActionResult> HeadToHead(int homeClubId, int awayClubId)
        {
            var headtoheadStats = await _context.HeadToHead
                .Where(mo => (mo.HomeTeamId == homeClubId && mo.AwayTeamId == awayClubId && mo.ClubId == homeClubId) ||
                         (mo.HomeTeamId == awayClubId && mo.AwayTeamId == homeClubId && mo.ClubId == homeClubId))
                .Include(mo => mo.HomeTeam)
                .Include(mo => mo.AwayTeam)
                .OrderByDescending(mo => mo.HeadToHeadDate)
                .ToListAsync();

            ViewBag.homeClubId = homeClubId;

            return PartialView("_FixtureHeadToHeadPartial", headtoheadStats);
        }


        [Authorize]
        public IActionResult MatchLineUpsFans(int fixtureId)
        {
            var matchLineUps = _context.LineUpXI
                .Where(mo => mo.FixtureId == fixtureId)
                .Include(s => s.Fixture)
                .FirstOrDefault();

            return PartialView("_MatchLineUpsFansPartial", matchLineUps);
        }

        [Authorize]
        public async Task<IActionResult> HomeTeamLineUp(int fixtureId, int clubId)
        {
            var homeLineUp = await _context.LineUpXI
                .Where(l => l.FixtureId == fixtureId
                            && l.ClubId == clubId)
                .Include(l => l.Fixture)
                .Include(l => l.Club)
                .Include(l => l.ClubPlayer)
                .ToListAsync();


            var homeClubName = _context.Fixture
                .Where(mo => mo.HomeTeam.ClubId==clubId)
                .Include(s => s.HomeTeam)
                .FirstOrDefault();

            var clubFormation = await _context.MatchFormation
                .Where(mo => mo.ClubId == clubId && mo.FixtureId == fixtureId)
                .Include(s => s.Club)
                .Include(s => s.Formation)
                .FirstOrDefaultAsync();

            ViewBag.HomeTeamName = homeClubName?.HomeTeam?.ClubName;


            if (clubFormation != null && clubFormation.Formation != null && clubFormation.Formation.FormationName != null)
            {
                ViewBag.ClubFormation = clubFormation.Formation.FormationName;
            }
            else
            {
                ViewBag.ClubFormation = "Formation not found";
            }

            return PartialView("_HomeTeamLineUp", homeLineUp);
        }

        [Authorize]
        public async Task<IActionResult> AwayTeamLineUp(int fixtureId, int clubId)
        {
            var awayLineUp = await _context.LineUpXI
                .Where(l => l.FixtureId == fixtureId
                            && l.ClubId == clubId)
                .Include(l => l.Fixture)
                .Include(l => l.Club)
                .Include(l => l.ClubPlayer)
                .ToListAsync();

            var awayClubName = _context.Fixture
                    .Where(mo => mo.AwayTeam.ClubId == clubId)
                    .Include(s => s.AwayTeam)
                    .FirstOrDefault();

            var clubFormation = await _context.MatchFormation
                .Where(mo => mo.ClubId == clubId && mo.FixtureId == fixtureId)
                .Include(s => s.Club)
                .Include(s => s.Formation)
                .FirstOrDefaultAsync();

            ViewBag.AwayTeamName = awayClubName?.AwayTeam?.ClubName;

            if (clubFormation != null && clubFormation.Formation != null && clubFormation.Formation.FormationName != null)
            {
                ViewBag.ClubFormation = clubFormation.Formation.FormationName;
            }
            else
            {
                ViewBag.ClubFormation = "Formation not found";
            }

            return PartialView("_AwayTeamLineUp",awayLineUp );
        }

        [Authorize]
        public async Task<IActionResult> HomeTeamSubstitutes(int fixtureId, int clubId)
        {
            var homeSubs = await _context.LineUpSubstitutes
                .Where(mo => mo.FixtureId == fixtureId
                             && mo.ClubId == clubId
                             && !mo.ClubPlayer.IsOnPitch)
                .Include(s => s.Club)
                .Include(s => s.Fixture)
                .Include(s => s.ClubPlayer)
                .ToListAsync();


            var homeClubName = _context.Fixture
                .Where(mo => mo.HomeTeam.ClubId == clubId)
                .Include(s => s.HomeTeam)
                .FirstOrDefault();

            ViewBag.HomeTeamName = homeClubName?.HomeTeam?.ClubName;


            return PartialView("_HomeTeamSubstitutes", homeSubs);
        }

        [Authorize]
        public async Task<IActionResult> AwayTeamSubstitutes(int fixtureId, int clubId)
        {
            var awaySubs = await _context.LineUpSubstitutes
                .Where(mo => mo.FixtureId == fixtureId
                             && mo.ClubId == clubId
                             && !mo.ClubPlayer.IsOnPitch)
                .Include(s => s.Club)
                .Include(s => s.Fixture)
                .Include(s => s.ClubPlayer)
                .ToListAsync();


            return PartialView("_AwayTeamSubstitutes", awaySubs);
        }

        [Authorize]
        public async Task<IActionResult> LineUpXIFinal(int fixtureId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            if (loggedInUser == null || !(loggedInUser is ClubManager clubManager))
            {
                return RedirectToAction("Error", "Home");
            }

            var matchXI = await _context.LineUpXI
                .Where(mo => mo.FixtureId == fixtureId && mo.ClubId == clubManager.ClubId)
                .Include(s => s.Club)
                .Include(s => s.Fixture)
                .Include(s => s.ClubPlayer)
                .ToListAsync();

            return PartialView("_MatchLineUpFinalPartial", matchXI);
        }

        [Authorize]
        public async Task<IActionResult> LineUpSubstitutesFinal(int fixtureId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            if (loggedInUser == null || !(loggedInUser is ClubManager clubManager))
            {
                return RedirectToAction("Error", "Home");
            }

            var matchSubsXI = await _context.LineUpSubstitutes
                .Where(mo => mo.FixtureId == fixtureId && mo.ClubId == clubManager.ClubId)
                .Include(s => s.Club)
                .Include(s => s.Fixture)
                .Include(s => s.ClubPlayer)
                .ToListAsync();

            return PartialView("_MatchLineUpSubstitutesFinalPartial", matchSubsXI);
        }

        [Authorize(Roles =("Club Manager"))]
        public async Task<IActionResult> CreateMatchLineUp(int fixtureId)
        {
            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            var viewModel = new MatchLineUpFinalViewModel
            {
                FixtureId = fixtureId,
                FixtureDate = fixture.KickOffDate,
                FixtureTime = fixture.KickOffTime
            };

            return PartialView("_CreateMatchLineUpPartial", viewModel);
        }


        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> PlayerMatchLineUp()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            if (loggedInUser == null || !(loggedInUser is ClubManager clubManager))
            {
                return RedirectToAction("Error", "Home");
            }

            var players = await _context.Player
                .Where(mo => mo.ClubId == clubManager.ClubId)
                .Include(s => s.Club)
                .ToListAsync();

            return PartialView("_MatchLineUpClubPlayersPartial", players);
        }

        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> MatchXIHolder(int fixtureId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            if (loggedInUser == null || !(loggedInUser is ClubManager clubManager))
            {
                return RedirectToAction("Error", "Home");
            }

            var matchXI = await _context.LineUpXIHolder
                .Where(mo => mo.FixtureId == fixtureId && mo.ClubId == clubManager.ClubId)
                .Include(s => s.Club)
                .Include(s => s.Fixture)
                .Include(s => s.ClubPlayer)
                .ToListAsync();

            return PartialView("_MatchXIHolderPartial", matchXI);
        }

        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> MatchSubstitutes(int fixtureId)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            if (loggedInUser == null || !(loggedInUser is ClubManager clubManager))
            {
                return RedirectToAction("Error", "Home");
            }

            var matchSubstitutes = await _context.LineUpSubstitutesHolder
                .Where(mo => mo.FixtureId == fixtureId && mo.ClubId == clubManager.ClubId)
                .Include(s => s.Club)
                .Include(s => s.Fixture)
                .Include(s => s.ClubPlayer)
                .ToListAsync();

            return PartialView("_MatchSubstitutesHolderPartial", matchSubstitutes);
        }

        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> ClubPlayers()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            var clubManager = loggedInUser as ClubManager;

            if (clubManager != null)
            {
                var clubId = clubManager.ClubId;

                var clubPlayers = _context.Player
                    .Where(p => p.ClubId == clubId && p.ClubId == clubManager.ClubId)
                    .Include (s => s.Club)
                    .ToList();

                return View(clubPlayers);
            }
            else
            {
                string errorMessage = "A club manager needs to be logged into the system to see a list of their players for creating a lineup.";

                return RedirectToAction("ErrorPage","Home", new { errorMessage = errorMessage });
            }
        }

        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.LineUp == null)
            {
                return NotFound();
            }

            var lineUp = await _context.LineUp
                .Include(l => l.Club)
                .Include(l => l.CreatedBy)
                .Include(l => l.Fixture)
                .Include(l => l.ModifiedBy)
                .FirstOrDefaultAsync(m => m.LineUpId == id);
            if (lineUp == null)
            {
                return NotFound();
            }

            return View(lineUp);
        }


        [Authorize(Roles = ("Club Manager"))]
        public IActionResult CreateMatchLineUpXIHolder(int fixtureId, string playerId)
        {
            var newViewModel = new MatchLineUpXIHolderViewModel
            {
                FixtureId = fixtureId,
                UserId = playerId
            };

            return View(newViewModel);
        }

        [Authorize(Roles = "Club Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMatchLineUpXIHolder(MatchLineUpXIHolderViewModel viewModel)
        {
            try
            {
                TempData.Remove("XiMessage");
                TempData.Remove("SubstitutesMessage");

                var count = _context.LineUpXIHolder.Count(l => l.FixtureId == viewModel.FixtureId);
                if (count >= 11)
                {
                    TempData["XiMessage"] = "Starting XI is limited to 11 players per match";
                    return Ok();
                }

                var existingLineup = await _context.LineUpXIHolder
                    .FirstOrDefaultAsync(l => l.FixtureId == viewModel.FixtureId && l.PlayerId == viewModel.UserId);

                if (existingLineup != null)
                {
                    TempData["XiMessage"] = "This player has already been added to starting XI";
                    return Ok();
                }

                var existingSubstitutes = await _context.LineUpSubstitutesHolder
                    .FirstOrDefaultAsync(l => l.FixtureId == viewModel.FixtureId && l.PlayerId == viewModel.UserId);

                if (existingSubstitutes != null)
                {
                    TempData["SubstitutesMessage"] = "This player has already been added to substitutes";
                    return Ok();
                }

                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var userId = user.Id;

                    var loggedInUser = await _userManager.GetUserAsync(User);
                    var clubManager = loggedInUser as ClubManager;

                    var player = await _context.Player.FindAsync(viewModel.UserId);

                    if (player != null)
                    {
                        var newLineUpXI = new LineUpXIHolder
                        {
                            FixtureId = viewModel.FixtureId,
                            PlayerId = viewModel.UserId,
                            ClubId = clubManager?.ClubId ?? 0,
                            CreatedById = userId,
                            ModifiedById = userId,
                            CreatedDateTime = DateTime.Now,
                            ModifiedDateTime = DateTime.Now
                        };

                        _context.LineUpXIHolder.Add(newLineUpXI);
                        await _context.SaveChangesAsync();

                        TempData["XiMessage"] = $"{player.FirstName} {player.LastName} ({player.Position}) added to starting XI successfully.";
                        return Ok();
                    }
                    else
                    {
                        TempData["XiMessage"] = "Player not found.";
                        return Ok();
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating a new MatchLineUpXIHolder: {ex.Message}");
                return View("Index");
            }
        }


        [Authorize(Roles = ("Club Manager"))]
        public IActionResult CreateMatchLineUpSubstitutesHolder(int fixtureId, string playerId)
        {
            var newViewModel = new MatchLineUpSubstitutesViewModel
            {
                FixtureId = fixtureId,
                UserId = playerId,
            };
            return View(newViewModel);
        }

        [Authorize(Roles = "Club Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMatchLineUpSubstitutesHolder(MatchLineUpSubstitutesViewModel viewModel)
        {
            try
            {
                TempData.Remove("SubstitutesMessage");
                TempData.Remove("XiMessage");

                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var userId = user.Id;

                    var count = _context.LineUpSubstitutesHolder.Count(l => l.FixtureId == viewModel.FixtureId);
                    if (count >= 7)
                    {
                        TempData["SubstitutesMessage"] = "Substitutes are limited to only 7 players per match!";
                        return Ok();
                    }

                    var existingLineup = await _context.LineUpXIHolder
                        .FirstOrDefaultAsync(l => l.FixtureId == viewModel.FixtureId && l.PlayerId == viewModel.UserId);

                    if (existingLineup != null)
                    {
                        TempData["XiMessage"] = "This player has already been added to starting XI";
                        return Ok();
                    }

                    var existingSubstitutes = await _context.LineUpSubstitutesHolder
                        .FirstOrDefaultAsync(l => l.FixtureId == viewModel.FixtureId && l.PlayerId == viewModel.UserId);

                    if (existingSubstitutes != null)
                    {
                        TempData["SubstitutesMessage"] = "This player has already been added to substitutes";
                        return Ok();
                    }

                    var clubManager = user as ClubManager;
                    var player = await _context.Player.FindAsync(viewModel.UserId);

                    if (player != null)
                    {
                        var newLineUpSubstitute = new LineUpSubstitutesHolder
                        {
                            FixtureId = viewModel.FixtureId,
                            PlayerId = viewModel.UserId,
                            ClubId = clubManager?.ClubId ?? 0,
                            CreatedById = userId,
                            ModifiedById = userId,
                            CreatedDateTime = DateTime.Now,
                            ModifiedDateTime = DateTime.Now,
                        };

                        _context.LineUpSubstitutesHolder.Add(newLineUpSubstitute);
                        await _context.SaveChangesAsync();

                        TempData["SubstitutesMessage"] = $"{player.FirstName} {player.LastName} ({player.Position}) added to substitutes successfully.";
                        return Ok();
                    }
                    else
                    {
                        TempData["SubstitutesMessage"] = "Player not found.";
                        return Ok();
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating a new MatchLineUpSubstitutesHolder: {ex.Message}");
                return View("Error");
            }
        }




        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> CreateMatchLineUpFinal(int fixtureId)
        {

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            var viewModel = new  MatchLineUpFinalViewModel
            {
                   FixtureId= fixtureId,
                   FixtureDate = fixture.KickOffDate,
                   FixtureTime = fixture.KickOffTime
            }; 

            return View(viewModel);
        }

        [Authorize(Roles = "Club Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMatchLineUpFinal(MatchLineUpFinalViewModel viewModel)
        {
            try
            {
                TempData.Remove("LineUpMessage");

                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return Json(new { success = false, error = string.Join(", ", errorMessages) });
                }

                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;
                var clubManager = user as ClubManager;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User ID is null or empty" });
                }

                var existingLineUp = await _context.LineUp
                    .FirstOrDefaultAsync(l => l.FixtureId == viewModel.FixtureId && l.ClubId == clubManager.ClubId);

                if (existingLineUp != null)
                {
                    TempData["LineUpMessage"] = "Lineup already exists for this match.";
                    return Json(new { success = false, error = "Lineup already exists for this match." });
                }

                var lineupXIHolders = await _context.LineUpXIHolder
                    .Include(l => l.ClubPlayer)
                    .ToListAsync();

                var lineupSubstitutesHolders = await _context.LineUpSubstitutesHolder
                    .Include(l => l.ClubPlayer)
                    .ToListAsync();

                if (!lineupXIHolders.Any() || !lineupSubstitutesHolders.Any())
                {
                    return Json(new { success = false, error = "Lineup XI or Substitutes cannot be empty." });
                }

                var lineUp = new LineUp
                {
                    ClubId = clubManager?.ClubId ?? 0,
                    FixtureId = viewModel.FixtureId,
                    CreatedById = userId,
                    ModifiedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    LineUpXI = lineupXIHolders.Select(lineupXIHolder => new LineUpXI
                    {
                        FixtureId = lineupXIHolder.FixtureId,
                        PlayerId = lineupXIHolder.PlayerId,
                        ClubId = lineupXIHolder.ClubId,
                        CreatedById = userId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        ModifiedById = userId
                    }).ToList(),

                    LineUpSubstitutes = lineupSubstitutesHolders.Select(lineupSubstitutesHolder => new LineUpSubstitutes
                    {
                        FixtureId = lineupSubstitutesHolder.FixtureId,
                        PlayerId = lineupSubstitutesHolder.PlayerId,
                        ClubId = lineupSubstitutesHolder.ClubId,
                        CreatedById = userId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        ModifiedById = userId
                    }).ToList()
                };

                foreach (var player in lineupXIHolders)
                {
                    var playerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(p => p.Player.Id == player.ClubPlayer.Id && p.League.IsCurrent)
                        .Include(p => p.Player)
                        .FirstOrDefaultAsync();

                    if (playerPerformanceReport != null)
                    {
                        playerPerformanceReport.AppearancesCount++;
                        _context.PlayerPerformanceReports.Update(playerPerformanceReport);
                    }

                    player.ClubPlayer.HasPlayed = true;
                    player.ClubPlayer.IsOnPitch = true;
                    _context.Player.Update(player.ClubPlayer);
                }

                foreach (var player in lineupSubstitutesHolders)
                {
                    player.ClubPlayer.HasPlayed = false;
                    player.ClubPlayer.IsOnPitch = false;
                    _context.Player.Update(player.ClubPlayer);
                }

                await _context.SaveChangesAsync();

                _context.LineUp.Add(lineUp);
                _context.LineUpXIHolder.RemoveRange(lineupXIHolders);
                _context.LineUpSubstitutesHolder.RemoveRange(lineupSubstitutesHolders);

                await _context.SaveChangesAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == viewModel.FixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Created match lineup for match of {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}", user.Id);

                await _requestLogService.LogSuceededRequest("Lineup created successfully.", StatusCodes.Status200OK);

                TempData["LineUpMessage"] = "Lineup uploaded successfully.";

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while creating the match lineup.");
                Console.WriteLine("Error message: " + ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                    Console.WriteLine("Inner exception stack trace: " + ex.InnerException.StackTrace);

                    return Json(new { success = false, error = ex.Message, innerError = ex.InnerException.Message, innerStackTrace = ex.InnerException.StackTrace });
                }
                else
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
        }




        [Authorize(Roles = ("Club Manager"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlayerFromLineUpXIHolder(int fixtureId, string playerId)
        {
            try
            {
                TempData.Remove("SubstitutesMessage");
                TempData.Remove("XiMessage");

                var lineUpXIHolder = await _context.LineUpXIHolder
                    .Include(s => s.ClubPlayer)
                    .FirstOrDefaultAsync(m => m.FixtureId == fixtureId && m.PlayerId == playerId);

                if (lineUpXIHolder != null)
                {
                    var playerFirstName = lineUpXIHolder.ClubPlayer.FirstName;
                    var playerLastName = lineUpXIHolder.ClubPlayer.LastName;
                    var playerPosition = lineUpXIHolder.ClubPlayer.Position;

                    _context.LineUpXIHolder.Remove(lineUpXIHolder);
                    await _context.SaveChangesAsync();

                    TempData["XiMessage"] = $"You have deleted {playerFirstName} {playerLastName} ({playerPosition}) from the lineup.";
                }
                else
                {
                    TempData["XiMessage"] = "Player not found in the starting lineup.";
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the player from LineUpXIHolder: {ex.Message}");
                return View("Error");
            }
        }


        [Authorize(Roles = ("Club Manager"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlayerFromLineUpSubstitutesHolder(int fixtureId, string playerId)
        {
            try
            {
                TempData.Remove("XiMessage");
                TempData.Remove("SubstitutesMessage");

                var lineUpSubstitutesHolder = await _context.LineUpSubstitutesHolder
                    .Include(s => s.ClubPlayer)
                    .FirstOrDefaultAsync(m => m.FixtureId == fixtureId && m.PlayerId == playerId);

                if (lineUpSubstitutesHolder != null)
                {
                    var playerFirstName = lineUpSubstitutesHolder.ClubPlayer.FirstName;
                    var playerLastName = lineUpSubstitutesHolder.ClubPlayer.LastName;
                    var playerPosition = lineUpSubstitutesHolder.ClubPlayer.Position;

                    _context.LineUpSubstitutesHolder.Remove(lineUpSubstitutesHolder);
                    await _context.SaveChangesAsync();

                    TempData["SubstitutesMessage"] = $"You have deleted {playerFirstName} {playerLastName} ({playerPosition}) from the substitutes.";
                }
                else
                {
                    TempData["SubstitutesMessage"] = "Player not found in the substitutes.";
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the player from LineUpSubstitutesHolder: {ex.Message}");
                return View("Error");
            }
        }

        [Authorize(Roles = ("Club Manager"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovePlayerFromXItoSubstitutes(int fixtureId, string playerId, int clubId)
        {
            try
            {
                TempData.Remove("XiMessage");
                TempData.Remove("SubstitutesMessage");

                var count = await _context.LineUpSubstitutesHolder.CountAsync(l => l.FixtureId == fixtureId);
                if (count >= 7)
                {
                    TempData["XiMessage"] = "Please delete one or more players from substitutes to move a player.";
                    return Ok();
                }

                var player = await _context.LineUpXIHolder
                    .Include(p => p.ClubPlayer)
                    .FirstOrDefaultAsync(m => m.FixtureId == fixtureId && m.PlayerId == playerId);
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                if (player != null)
                {
                    _context.LineUpXIHolder.Remove(player);

                    _context.LineUpSubstitutesHolder.Add(new LineUpSubstitutesHolder
                    {
                        ClubId = clubId,
                        FixtureId = fixtureId,
                        PlayerId = playerId,
                        CreatedById = userId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        ModifiedById = userId
                    });

                    await _context.SaveChangesAsync();

                    TempData["XiMessage"] = $"You have successfully moved {player.ClubPlayer.FirstName} {player.ClubPlayer.LastName} ({player.ClubPlayer.Position}) to substitutes.";
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while moving the player from LineUpXIHolder to LineUpSubstitutesHolder: {ex.Message}");
                return View("Error");
            }
        }

        [Authorize(Roles = ("Club Manager"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovePlayerFromSubstitutesToXI(int fixtureId, string playerId, int clubId)
        {
            try
            {
                TempData.Remove("XiMessage");
                TempData.Remove("SubstitutesMessage");

                var count = await _context.LineUpXIHolder.CountAsync(l => l.FixtureId == fixtureId);
                if (count >= 11)
                {
                    TempData["SubstitutesMessage"] = "Please delete one or more players from starting XI to move a player.";
                    return Ok();
                }

                var player = await _context.LineUpSubstitutesHolder
                    .Include(s => s.ClubPlayer)
                    .FirstOrDefaultAsync(m => m.FixtureId == fixtureId && m.PlayerId == playerId);
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                if (player != null)
                {
                    _context.LineUpSubstitutesHolder.Remove(player);

                    _context.LineUpXIHolder.Add(new LineUpXIHolder
                    {
                        ClubId = clubId,
                        FixtureId = fixtureId,
                        PlayerId = playerId,
                        CreatedById = userId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        ModifiedById = userId
                    });

                    await _context.SaveChangesAsync();

                    TempData["SubstitutesMessage"] = $"You have successfully moved {player.ClubPlayer.FirstName} {player.ClubPlayer.LastName} ({player.ClubPlayer.Position}) to starting XI.";
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while moving the player from LineUpSubstitutesHolder to LineUpXIHolder: {ex.Message}");
                return View("Error");
            }
        }

        private bool LineUpExists(int id)
        {
          return (_context.LineUp?.Any(e => e.LineUpId == id)).GetValueOrDefault();
        }
    }
}
