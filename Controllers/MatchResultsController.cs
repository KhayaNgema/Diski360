using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;

namespace MyField.Controllers
{
    public class MatchResultsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailServcie;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public MatchResultsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            EmailService emailService,
            IEncryptionService encryptionService,
            RequestLogService requestLogService)
        {
            _context = context;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _emailServcie = emailService;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }


        public async Task<IActionResult> LeagueResults(string divisionId)
        {
            ViewBag.DivisionId = divisionId;

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            ViewBag.Leagues = await _context.League
                .Where(l => l.DivisionId == decryptedDivisionId)
                .OrderByDescending(l => l.CreatedDateTime)
                .ToListAsync();

            return PartialView("LeagueResultsPartial");
        }

        [Authorize(Roles = ("Sport Administrator, Sport Coordinator, Division Manager"))]
        public async Task<IActionResult> MatchResultsBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            int? divisionId = null;
            dynamic loggedInUser = null;

            var divisionManager = await _context.DivisionManagers
                .Where(lu => lu.Id == user.Id)
                .Include(lu => lu.Division)
                .FirstOrDefaultAsync();

            if (divisionManager != null)
            {
                divisionId = divisionManager.DivisionId;
                loggedInUser = divisionManager;
            }
            else
            {
                var sportsMember = await _context.SportMember
                    .Where(lu => lu.Id == user.Id)
                    .Include(lu => lu.Division)
                    .FirstOrDefaultAsync();

                if (sportsMember != null)
                {
                    divisionId = sportsMember.DivisionId;
                    loggedInUser = sportsMember;
                }
            }

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }

            var matchResults = await _context.MatchResult
                .Where(m => m.LeagueId == currentLeague.LeagueId &&
                m.DivisionId == divisionId)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.CreatedBy)
                .Include(m => m.ModifiedBy)
                .OrderByDescending(m => m.CreatedDateTime)
                .ToListAsync();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent && c.DivisionId == divisionId && c.DivisionId == divisionId)
                .FirstOrDefaultAsync();


            ViewBag.CurrentSeason = currentSeason.LeagueYears;

            return View(matchResults);
        }


        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> MatchResults()
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

            var matchResults = await _context.MatchResult
                .Where(m => m.LeagueId == currentLeague.LeagueId && 
                m.DivisionId == divisionId)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.CreatedBy)
                .Include(m => m.ModifiedBy)
                .OrderByDescending(m => m.CreatedDateTime)
                .ToListAsync();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                .FirstOrDefaultAsync();


            ViewBag.CurrentSeason = currentSeason.LeagueYears;

            return View(matchResults);
        }

        [Authorize(Roles =("Club Administrator, Club Manager, Player"))]
        public async Task<IActionResult> MyMatchResults()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return RedirectToAction("Error", "Home");
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var club = await _context.Club.FindAsync(clubId);

            if (club == null)
            {
                return RedirectToAction("Error", "Home");
            }

            ViewBag.ClubName = club.ClubName;

            var matchResults = await _context.MatchResult
                                             .Where( m => m.League.IsCurrent && m.League.DivisionId == club.DivisionId)
                                             .Include(m => m.HomeTeam)
                                             .Include(m => m.AwayTeam)
                                             .Where(mo => mo.HomeTeam.ClubId == clubId || mo.AwayTeam.ClubId == clubId)
                                              .OrderByDescending(m => m.CreatedDateTime)
                                             .ToListAsync();

            return View(matchResults);
        }

        public async Task<IActionResult> ResultsMain(string divisionId, int? leagueId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == decryptedDivisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }


            var matchResults = await _context.MatchResult
               .Where(m =>  m.DivisionId == decryptedDivisionId && 
               m.LeagueId == leagueId)
               .Include(m => m.HomeTeam)
               .Include(m => m.AwayTeam)
               .Include(m => m.CreatedBy)
               .Include(m => m.ModifiedBy)
              .OrderByDescending(m => m.CreatedDateTime)
              .ToListAsync();


            return PartialView("_MatchResultsPartial", matchResults);
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> BackOfficeMatchResults()
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


            var matchResults = await _context.MatchResult
                .Where(m => m.LeagueId == currentLeague.LeagueId && 
                m.DivisionId == divisionId)
                .Include(m => m.CreatedBy)
                .Include(m => m.ModifiedBy)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                  .OrderByDescending(m => m.CreatedDateTime)
                .ToListAsync();

            return PartialView("_BackOfficeMatchResultsPartial", matchResults);
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpGet]
        public async Task<IActionResult> Upload (string fixtureId, string homeClubName, string awayClubName, DateTime kickOfDate, DateTime kickOffTime, string homeTeamBadge, string awayTeamBadge, string location, string homeTeamId, string awayTeamId)
        {
            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);
            var decryptedHomeTeamId = _encryptionService.DecryptToInt(homeTeamId);
            var decryptedAwayTeamId = _encryptionService.DecryptToInt(awayTeamId);

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == decryptedFixtureId &&
                f.HomeTeamId == decryptedHomeTeamId &&
                f.AwayTeamId == decryptedAwayTeamId && 
                f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            var homeTeam = fixture.HomeTeam;

            var awayTeam = fixture.AwayTeam;

            var viewModel = new MatchResultsViewModel
            {
                FixtureId = decryptedFixtureId,
                HomeTeamId = decryptedHomeTeamId,
                AwayTeamId = decryptedAwayTeamId,
                HomeTeamBadge = homeTeamBadge,
                AwayTeamBadge = awayTeamBadge,
                HomeTeamScore = 0,
                AwayTeamScore = 0,
                Location = location,
                MatchDate = kickOfDate,
                MatchTime = kickOffTime,
                HomeTeam = homeClubName,
                AwayTeam = awayClubName
            };

            return View(viewModel);
        }


        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpPost]
        public async Task<IActionResult> Upload(MatchResultsViewModel viewModel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;

                var currentLeague = await _context.League
                    .Where( c => c.IsCurrent && c.DivisionId == divisionId)
                    .FirstOrDefaultAsync(); 


                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == viewModel.FixtureId && f.DivisionId == divisionId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                var homeTeamMatches = await _context.ClubPerformanceReports
                    .Where(h => h.ClubId == fixture.HomeTeam.ClubId && h.LeagueId == currentLeague.LeagueId && h.DivisionId == divisionId)
                    .FirstOrDefaultAsync();


                var awayTeamMatches = await _context.ClubPerformanceReports
                    .Where(h => h.ClubId == fixture.AwayTeam.ClubId && h.LeagueId == currentLeague.LeagueId && h.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if(homeTeamMatches != null)
                {
                    homeTeamMatches.GamesPlayedCount++;
                }


                if (awayTeamMatches != null)
                {
                    awayTeamMatches.GamesPlayedCount++;
                }

                _context.Update(homeTeamMatches);
                _context.Update(awayTeamMatches);
                await _context.SaveChangesAsync();

                fixture.FixtureStatus = FixtureStatus.Ended;

                _context.Update(fixture);

                var userId = user?.Id;

                if (user == null || userId == null)
                {
                    throw new InvalidOperationException("User not authenticated or user ID not found.");
                }

                if (currentLeague == null)
                {
                    ModelState.AddModelError(string.Empty, "No current league found.");
                    return Json(new { success = false, message = "No current league found." });
                }

                var matchResultsReport = await _context.MatchResultsReports
                    .Where(m => m.Season.IsCurrent && m.Season.DivisionId == divisionId)
                    .Include(m => m.Season)
                    .FirstOrDefaultAsync();

                var matchReport = await _context.MatchReports
                    .Where(m => m.Season.IsCurrent && m.Season.DivisionId == divisionId)
                    .Include(m => m.Season)
                    .FirstOrDefaultAsync();

                var newMatchResults = new MatchResults
                {
                    LeagueId = currentLeague.LeagueId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    CreatedById = userId,
                    ModifiedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    MatchDate = fixture.KickOffDate,
                    MatchTime = fixture.KickOffTime,
                    FixtureId = viewModel.FixtureId,
                    HomeTeamScore = viewModel.HomeTeamScore,
                    AwayTeamScore = viewModel.AwayTeamScore,
                    Location = fixture.Location,
                    DivisionId = divisionId
                };

                _context.Add(newMatchResults);

                var homeStanding = await _context.Standing
                    .Where(s => s.LeagueId == currentLeague.LeagueId && s.ClubId == fixture.HomeTeam.ClubId && s.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (homeStanding != null)
                {
                    homeStanding.MatchPlayed++;
                    homeStanding.GoalsScored += viewModel.HomeTeamScore;
                    homeStanding.GoalsConceded += viewModel.AwayTeamScore;
                    homeStanding.GoalDifference += viewModel.HomeTeamScore - viewModel.AwayTeamScore;
                    UpdateStandingStats(homeStanding, viewModel.HomeTeamScore, viewModel.AwayTeamScore);
                    homeStanding.ModifiedById = userId;
                    homeStanding.ModifiedDateTime = DateTime.Now;

                    _context.Update(homeStanding);
                }

                var awayStanding = await _context.Standing
                    .Where(s => s.LeagueId == currentLeague.LeagueId && s.ClubId == fixture.AwayTeam.ClubId && s.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (awayStanding != null)
                {
                    awayStanding.MatchPlayed++;
                    awayStanding.GoalsScored += viewModel.AwayTeamScore;
                    awayStanding.GoalsConceded += viewModel.HomeTeamScore;
                    awayStanding.GoalDifference += viewModel.AwayTeamScore - viewModel.HomeTeamScore;
                    UpdateStandingStats(awayStanding, viewModel.AwayTeamScore, viewModel.HomeTeamScore);
                    awayStanding.ModifiedById = userId;
                    awayStanding.ModifiedDateTime = DateTime.Now;

                    _context.Update(awayStanding);
                }

                var homeHeadToHead = new HeadTohead
                {
                    HeadToHeadDate = fixture.KickOffDate,
                    ClubId = fixture.HomeTeam.ClubId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    MatchResults = viewModel.HomeTeamScore > viewModel.AwayTeamScore ? "W" :
                                   viewModel.HomeTeamScore == viewModel.AwayTeamScore ? "D" :
                                   "L",
                    AwayTeamGoals = viewModel.AwayTeamScore,
                    HomeTeamGoals = viewModel.HomeTeamScore
                };

                _context.Add(homeHeadToHead);

                var awayHeadToHead = new HeadTohead
                {
                    HeadToHeadDate = fixture.KickOffDate,
                    ClubId = fixture.AwayTeam.ClubId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    MatchResults = viewModel.AwayTeamScore > viewModel.HomeTeamScore ? "W" :
                                  viewModel.AwayTeamScore == viewModel.HomeTeamScore ? "D" :
                                   "L",
                    AwayTeamGoals = viewModel.AwayTeamScore,
                    HomeTeamGoals = viewModel.HomeTeamScore
                };


                if (viewModel.HomeTeamScore > viewModel.AwayTeamScore)
                {
                    if(homeTeamMatches != null && awayTeamMatches != null)
                    {
                        homeTeamMatches.GamesWinCount++;
                        awayTeamMatches.GamesLoseCount++;
                    }
                }
                else if (viewModel.AwayTeamScore > viewModel.HomeTeamScore)
                {
                    if (homeTeamMatches != null && awayTeamMatches != null)
                    {
                        homeTeamMatches.GamesLoseCount++;
                        awayTeamMatches.GamesWinCount++;
                    }
                }
                else
                {
                    if (homeTeamMatches != null && awayTeamMatches != null)
                    {
                        homeTeamMatches.GamesDrawCount++;
                        awayTeamMatches.GamesDrawCount++;
                    }
                }

                _context.Add(awayHeadToHead);

                matchResultsReport.ReleasedResultsCount++;

                if (viewModel.HomeTeamScore > viewModel.AwayTeamScore || viewModel.AwayTeamScore > viewModel.HomeTeamScore)
                {
                    matchResultsReport.WinsCount++;
                    matchResultsReport.LosesCount++;
                }
                else
                {
                    matchResultsReport.DrawsCount++;
                }

                matchReport.PlayedMatchesCounts++;

                var matchPlayers = await _context.Player
                    .Where(p => p.ClubId == fixture.HomeTeamId || p.ClubId == fixture.AwayTeamId && p.DivisionId == divisionId)
                    .ToListAsync();

                foreach (var player in matchPlayers)
                {
                    player.HasPlayed = false;
                    player.IsOnPitch = false;
                    _context.Update(player);
                }

                _context.Update(homeTeamMatches);
                _context.Update(awayTeamMatches);


                await _context.SaveChangesAsync();


                await transaction.CommitAsync();

                await _activityLogger.Log($"Uploaded match results for a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}. The recorded final score is {viewModel.HomeTeamScore}-{viewModel.AwayTeamScore}.", user.Id);

                await _requestLogService.LogSuceededRequest("Match results uploaded successfully.", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully uploaded match results for a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}.";

                return RedirectToAction(nameof(MatchResultsBackOffice));
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to end match: " + ex.StackTrace,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }


        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int fixtureId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId)
                    .Include(l => l.Fixture)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                     .Where(f => f.FixtureId == fixtureId)
                     .Include(f => f.HomeTeam)
                     .Include(f => f.AwayTeam)
                     .FirstOrDefaultAsync();

                var homeTeamMatches = await _context.ClubPerformanceReports
                      .Where(h => h.ClubId == fixture.HomeTeam.ClubId && h.League.IsCurrent)
                      .FirstOrDefaultAsync();


                var awayTeamMatches = await _context.ClubPerformanceReports
                    .Where(h => h.ClubId == fixture.AwayTeam.ClubId && h.League.IsCurrent)
                    .FirstOrDefaultAsync();

                if (homeTeamMatches != null)
                {
                    homeTeamMatches.GamesPlayedCount++;
                }


                if (awayTeamMatches != null)
                {
                    awayTeamMatches.GamesPlayedCount++;
                }

                _context.Update(homeTeamMatches);
                _context.Update(awayTeamMatches);
                await _context.SaveChangesAsync();


                if (liveMatch == null)
                {
                    return Json(new { success = false, message = "Live match not found." });
                }

                liveMatch.IsLive = false;
                liveMatch.IsHalfTime = false;
                liveMatch.ISEnded = true;
                liveMatch.LiveTime = 90;

                _context.Update(liveMatch);

                fixture.FixtureStatus = FixtureStatus.Ended;

                _context.Update(fixture);

                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;

                var userId = user?.Id;

                if (user == null || userId == null)
                {
                    throw new InvalidOperationException("User not authenticated or user ID not found.");
                }

                var currentLeague = await _context.League
                    .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (currentLeague == null)
                {
                    ModelState.AddModelError(string.Empty, "No current league found.");
                    return Json(new { success = false, message = "No current league found." });
                }

                var matchResultsReport = await _context.MatchResultsReports
                    .Where(m => m.LeagueId == currentLeague.LeagueId &&
                    m.DivisionId == divisionId)
                    .Include(m => m.Season)
                    .FirstOrDefaultAsync();

                var matchReport = await _context.MatchReports
                    .Where(m => m.LeagueId == currentLeague.LeagueId && 
                    m.DivisionId == divisionId)
                    .Include(m => m.Season)
                    .FirstOrDefaultAsync();

                var newMatchResults = new MatchResults
                {
                    LeagueId = currentLeague.LeagueId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    CreatedById = userId,
                    ModifiedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    MatchDate = fixture.KickOffDate,
                    MatchTime = fixture.KickOffTime,
                    FixtureId = fixtureId,
                    HomeTeamScore = liveMatch.HomeTeamScore,
                    AwayTeamScore = liveMatch.AwayTeamScore,
                    Location = fixture.Location,
                    DivisionId = fixture.DivisionId
                };

                _context.Add(newMatchResults);
                await _context.SaveChangesAsync();

                var homeStanding = await _context.Standing
                    .Where(s => s.LeagueId == currentLeague.LeagueId && s.ClubId == fixture.HomeTeam.ClubId)
                    .FirstOrDefaultAsync();

                if (homeStanding != null)
                {
                    homeStanding.MatchPlayed++;
                    homeStanding.GoalsScored += liveMatch.HomeTeamScore;
                    homeStanding.GoalsConceded += liveMatch.AwayTeamScore;
                    homeStanding.GoalDifference += liveMatch.HomeTeamScore - liveMatch.AwayTeamScore;
                    UpdateStandingStats(homeStanding, liveMatch.HomeTeamScore, liveMatch.AwayTeamScore);
                    homeStanding.ModifiedById = userId;
                    homeStanding.ModifiedDateTime = DateTime.Now;

                    _context.Update(homeStanding);
                }

                var awayStanding = await _context.Standing
                    .Where(s => s.LeagueId == currentLeague.LeagueId && s.ClubId == fixture.AwayTeam.ClubId)
                    .FirstOrDefaultAsync();

                if (awayStanding != null)
                {
                    awayStanding.MatchPlayed++;
                    awayStanding.GoalsScored += liveMatch.AwayTeamScore;
                    awayStanding.GoalsConceded += liveMatch.HomeTeamScore;
                    awayStanding.GoalDifference += liveMatch.AwayTeamScore - liveMatch.HomeTeamScore;
                    UpdateStandingStats(awayStanding, liveMatch.AwayTeamScore, liveMatch.HomeTeamScore);
                    awayStanding.ModifiedById = userId;
                    awayStanding.ModifiedDateTime = DateTime.Now;

                    _context.Update(awayStanding);
                }

                var homeHeadToHead = new HeadTohead
                {
                    HeadToHeadDate = fixture.KickOffDate,
                    ClubId = fixture.HomeTeam.ClubId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    MatchResults = liveMatch.HomeTeamScore > liveMatch.AwayTeamScore ? "W" :
                                   liveMatch.HomeTeamScore == liveMatch.AwayTeamScore ? "D" :
                                   "L",
                    AwayTeamGoals = liveMatch.AwayTeamScore,
                    HomeTeamGoals = liveMatch.HomeTeamScore
                };

                _context.Add(homeHeadToHead);

                var awayHeadToHead = new HeadTohead
                {
                    HeadToHeadDate = fixture.KickOffDate,
                    ClubId = fixture.AwayTeam.ClubId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    MatchResults = liveMatch.AwayTeamScore > liveMatch.HomeTeamScore ? "W" :
                                   liveMatch.AwayTeamScore == liveMatch.HomeTeamScore ? "D" :
                                   "L",
                    AwayTeamGoals = liveMatch.AwayTeamScore,
                    HomeTeamGoals = liveMatch.HomeTeamScore
                };

                _context.Add(awayHeadToHead);



                if (liveMatch.HomeTeamScore > liveMatch.AwayTeamScore)
                {
                    if (homeTeamMatches != null && awayTeamMatches != null)
                    {
                        homeTeamMatches.GamesWinCount++;
                        awayTeamMatches.GamesLoseCount++;
                    }
                }
                else if (liveMatch.AwayTeamScore > liveMatch.HomeTeamScore)
                {
                    if (homeTeamMatches != null && awayTeamMatches != null)
                    {
                        homeTeamMatches.GamesLoseCount++;
                        awayTeamMatches.GamesWinCount++;
                    }
                }
                else
                {
                    if (homeTeamMatches != null && awayTeamMatches != null)
                    {
                        homeTeamMatches.GamesDrawCount++;
                        awayTeamMatches.GamesDrawCount++;
                    }
                }

                _context.Update(homeTeamMatches);
                _context.Update(awayTeamMatches);

                matchResultsReport.ReleasedResultsCount++;

                if (liveMatch.HomeTeamScore > liveMatch.AwayTeamScore || liveMatch.AwayTeamScore > liveMatch.HomeTeamScore)
                {
                    matchResultsReport.WinsCount++;
                    matchResultsReport.LosesCount++;
                }
                else
                {
                    matchResultsReport.DrawsCount++;
                }

                matchReport.PlayedMatchesCounts++;

                var liveGoals = await _context.LiveGoalHolders
                    .Where(l => l.LiveId == liveMatch.LiveId)
                    .Include(l => l.AssistedBy)
                    .Include(l => l.ScoredBy)
                    .Include(l => l.Live)
                    .Include(l => l.League)
                    .ToListAsync();

                foreach (var liveGoal in liveGoals)
                {
                    var newGoal = new LiveGoal
                    {
                        LiveId = liveGoal.LiveId,
                        LeagueId = liveGoal.LeagueId,
                        ScoreById = liveGoal.ScoredById,
                        ScoredTime = liveGoal.ScoredTime,
                        RecordedTime = liveGoal.RecordedTime
                    };

                    _context.Add(newGoal);
                }

                var liveYellowCards = await _context.LiveYellowCardHolders
                    .Where(l => l.LiveId == liveMatch.LiveId && l.League.IsCurrent)
                    .Include(l => l.YellowCommitedBy)
                    .Include(l => l.Live)
                    .Include(l => l.League)
                    .ToListAsync();

                foreach (var yellowCard in liveYellowCards)
                {
                    var newYellowCard = new YellowCard
                    {
                        LiveId = yellowCard.LiveId,
                        LeagueId = yellowCard.LeagueId,
                        YellowCommitedById = yellowCard.YellowCommitedById,
                        RecordedTime = yellowCard.RecordedTime,
                        YellowCardTime = yellowCard.YellowCardTime
                    };

                    _context.Add(newYellowCard);
                }

                var liveRedCards = await _context.LiveRedCardHolders
                    .Where(l => l.LiveId == liveMatch.LiveId && l.League.IsCurrent)
                    .Include(l => l.RedCommitedBy)
                    .Include(l => l.Live)
                    .Include(l => l.League)
                    .ToListAsync();

                foreach (var redCard in liveRedCards)
                {
                    var newRedCard = new RedCard
                    {
                        LiveId = redCard.LiveId,
                        LeagueId = redCard.LeagueId,
                        RedCommitedById = redCard.RedCommitedById,
                        RecordedTime = redCard.RecordedTime,
                        RedCardTime = redCard.RedCardTime
                    };

                    _context.Add(newRedCard);
                }

                var penalties = await _context.Penalties
                     .Where(l => l.LiveId == liveMatch.LiveId && l.League.IsCurrent)
                     .Include(l => l.Live)
                     .Include(l => l.League)
                     .Include(l => l.Player)
                     .ToListAsync();

                foreach (var penaltyTaker in penalties)
                {
                    var scoredPlayerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(sp => sp.PlayerId == penaltyTaker.Player.Id && sp.League.IsCurrent)
                         .Include(t => t.Player)
                        .FirstOrDefaultAsync();


                    var playerGoals = await _context.TopScores
                        .Where(t => t.PlayerId == penaltyTaker.Player.Id && t.League.IsCurrent)
                        .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    playerGoals.NumberOfGoals++;

                    if (scoredPlayerPerformanceReport != null)
                    {
                        scoredPlayerPerformanceReport.GoalsScoredCount++;

                        _context.Update(scoredPlayerPerformanceReport);
                    }
                }


                var matchPlayers = await _context.Player
                    .Where(p => p.ClubId == fixture.HomeTeamId || p.ClubId == fixture.AwayTeamId)
                    .ToListAsync();

                foreach (var player in matchPlayers)
                {
                    player.HasPlayed = false;
                    player.IsOnPitch = false;
                    _context.Update(player);
                }

                await _context.SaveChangesAsync();


                var liveOwnGoals = await _context.LiveOwnGoalHolders
                   .Where(l => l.LiveId == liveMatch.LiveId && l.League.IsCurrent)
                   .Include(l => l.OwnGoalScoredBy)
                   .Include(l => l.Live)
                   .Include(l => l.League)
                   .ToListAsync();

                foreach (var ownGoalScorer in liveOwnGoals)
                {
                    var scoredPlayerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(sp => sp.PlayerId == ownGoalScorer.OwnGoalScoredById && sp.League.IsCurrent)
                        .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    if (scoredPlayerPerformanceReport != null)
                    {
                        scoredPlayerPerformanceReport.OwnGoalsScoredCount++;

                        _context.Update(scoredPlayerPerformanceReport);
                    }
                }

                foreach (var goalScorer in liveGoals)
                {
                    var scoredPlayerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(sp => sp.PlayerId == goalScorer.ScoredById && sp.League.IsCurrent)
                         .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    var assistedPlayerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(ap => ap.PlayerId == goalScorer.AssistedById && ap.League.IsCurrent)
                        .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    var playerGoals = await _context.TopScores
                        .Where(t => t.PlayerId == goalScorer.ScoredById && t.League.IsCurrent)
                        .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    playerGoals.NumberOfGoals++;


                    var playerAssists = await _context.TopAssists
                         .Where(p => p.PlayerId == goalScorer.AssistedById && p.League.IsCurrent)
                         .Include(t => t.Player)
                         .FirstOrDefaultAsync();

                    if (assistedPlayerPerformanceReport != null && playerAssists != null)
                    {
                        assistedPlayerPerformanceReport.AssistsCount++;
                        playerAssists.NumberOfAssists++;

                        _context.Update(assistedPlayerPerformanceReport);
                    }


                    if (scoredPlayerPerformanceReport != null)
                    {
                        scoredPlayerPerformanceReport.GoalsScoredCount++;

                        _context.Update(scoredPlayerPerformanceReport);
                    }
                }


                foreach (var yellowCard in liveYellowCards)
                {
                    var yellowCardIssuedPlayerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(sp => sp.PlayerId == yellowCard.YellowCommitedById && sp.League.IsCurrent)
                        .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    if (yellowCardIssuedPlayerPerformanceReport != null)
                    {
                        yellowCardIssuedPlayerPerformanceReport.YellowCardCount++;
                        _context.Update(yellowCardIssuedPlayerPerformanceReport);
                    }
                }

                foreach (var redCard in liveRedCards)
                {
                    var redCardIssuedPlayerPerformanceReport = await _context.PlayerPerformanceReports
                        .Where(sp => sp.PlayerId == redCard.RedCommitedById && sp.League.IsCurrent)
                        .Include(t => t.Player)
                        .FirstOrDefaultAsync();

                    if (redCardIssuedPlayerPerformanceReport != null)
                    {
                        redCardIssuedPlayerPerformanceReport.RedCardCount++;
                        _context.Update(redCardIssuedPlayerPerformanceReport);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


                await _activityLogger.Log($"Ended a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}. The recorded final score is {liveMatch.HomeTeamScore}-{liveMatch.AwayTeamScore}.", user.Id);

                await _requestLogService.LogSuceededRequest("Match ended successfully.", StatusCodes.Status200OK);


                return Json(new { success = true, message = "Match results have been updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to end match: " + ex.StackTrace,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }

        private async Task UpdateStandingsOrder()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                   (user as ClubManager)?.DivisionId ??
                   (user as Player)?.DivisionId ??
                   (user as SportsMember)?.DivisionId ??
                   (user as Officials)?.DivisionId;

            var standings = await _context.Standing
                .Where(s => s.DivisionId == divisionId)
                .ToListAsync();

            var orderedStandings = standings.OrderByDescending(s => s.Points)
                                            .ThenByDescending(s => s.GoalDifference)
                                            .ThenBy(s => s.MatchPlayed)
                                            .ThenByDescending(s => s.Last5Games)
                                            .ToList();

            for (int i = 0; i < orderedStandings.Count; i++)
            {
                var standing = orderedStandings[i];
                standing.Position = i + 1;
                _context.Update(standing);
            }

            await _context.SaveChangesAsync();
        }

        private void UpdateStandingStats(Standing standing, int teamScore, int opponentScore)
        {
            if (teamScore > opponentScore)
            {
                standing.Points += 3;
                standing.Wins++;
                standing.Last5Games = AppendOutcome(standing.Last5Games, "W");
            }
            else if (teamScore == opponentScore)
            {
                standing.Points++;
                standing.Draw++;
                standing.Last5Games = AppendOutcome(standing.Last5Games, "D");
            }
            else
            {
                standing.Lose++;
                standing.Last5Games = AppendOutcome(standing.Last5Games, "L");
            }

            if (standing.Last5Games.Length > 5)
            {
                standing.Last5Games = standing.Last5Games.Substring(1);
            }
        }

        private string AppendOutcome(string last5Games, string outcome)
        {
            if (last5Games.Length >= 5)
            {
                last5Games = last5Games.Substring(1);
            }
            return last5Games + outcome;
        }

        private bool MatchResultsExists(int id)
        {
          return (_context.MatchResult?.Any(e => e.ResultsId == id)).GetValueOrDefault();
        }
    }
}
