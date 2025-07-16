using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;
using MyField.ViewModels;
using MyField.Services;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using MyField.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;

namespace MyField.Controllers
{
    public class ClubsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;
        private readonly EmailService _emailService;

        public ClubsController(Ksans_SportsDbContext context, 
            FileUploadService fileUploadService, 
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            EmailService emailService,
            IEncryptionService encryptionService,
            RequestLogService requestLogService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _emailService = emailService;
            _activityLogger = activityLogger;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        public async Task<IActionResult> LeagueClubs(string divisionId)
        {
            ViewBag.DivisionId = divisionId;

            return PartialView("LeagueClubsPartial");
        }

        [Authorize]
        public async Task<IActionResult> ClubPlayers(int clubId)
        {
            var clubPlayers = await _context.Player
                .Where(p => p.ClubId == clubId)
                .Include(s => s.Club)   
                .ToListAsync();

            return PartialView("_ClubPlayersPartial", clubPlayers);
        }


        public async Task<IActionResult> HeadToHead(int clubId)
        {

            var headToHead = await _context.HeadToHead
                .Include(s => s.HomeTeam)
                .Include(s => s.AwayTeam)
                .Where(h => h.ClubId == clubId)
                .OrderByDescending(mo => mo.HeadToHeadDate)
                .ToListAsync();

            return PartialView("_ClubStatsPartial", headToHead);
        }


        public async Task<IActionResult> Clubs()
        {
            var clubs = await _context.Club
                .Include(s => s.CreatedBy)
                .Include(s => s.ModifiedBy)
                .ToListAsync();

            foreach(var club in clubs)
            {
                var clubManager = await _context.ClubManager
              .Where(mo => mo.ClubId == club.ClubId)
              .FirstOrDefaultAsync();

                if (clubManager != null && clubManager.FirstName != null && clubManager.LastName != null)
                {
                    ViewBag.ClubManager = $"{clubManager.FirstName} {clubManager.LastName}";
                }
                else
                {
                    ViewBag.ClubManager = "N/A";
                }
            }


            return View(clubs);
        }


        [Authorize(Roles = "Sport Administrator, Sport Coordinator, Division Manager")]
        public async Task<IActionResult> ClubsBackOffice()
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

            var clubs = await _context.Club
                .Where(s => s.DivisionId == divisionId)
                .Include(s => s.CreatedBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.ClubManager)
                .Include(s => s.League)
                .OrderByDescending(s => s.CreatedDateTime)
                .ToListAsync();

            return View(clubs);
        }


        [Authorize(Roles = ("Club Administrator, Club Manager, Player"))]
        public async Task<IActionResult> MyClub()
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

            var clubs = await _context.Club
                                      .FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            ViewBag.ClubName = clubs?.ClubName;

            return View(clubs);
        }

        [Authorize]
        public async Task<IActionResult> ClubsBackOfficeUsers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }


            var clubs = await _context.Club
                .Where(mo => mo.LeagueId == currentLeague.LeagueId && 
                 mo.DivisionId == divisionId)
                .ToListAsync();

            return View( clubs);
        }

        public async Task<IActionResult> ClubsMain(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == decryptedDivisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }


            var clubs = await _context.Club
                .Where(mo => mo.LeagueId == currentLeague.LeagueId && mo.DivisionId == decryptedDivisionId)
                .ToListAsync();

            return PartialView("_ClubsPartial", clubs);
        }


        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> BackOfficeClubs()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var clubs = await _context.Club
                .Where(c => c.League.IsCurrent && 
                c.DivisionId == divisionId)
                .ToListAsync();

            return PartialView("_BackOfficeClubsPartial", clubs);
        }


        [Authorize]
        public async Task<IActionResult> ClubSummary(int? clubId)
        {
            if (clubId == null)
            {
                return NotFound();
            }

            var club = await _context.Club
                .Include(c => c.ClubManager)
                .FirstOrDefaultAsync(c => c.ClubId == clubId);

            if (club != null)
            {
                var viewModel = new ClubSummaryViewModel
                {
                    Club = club,
                    TournamentClub = null
                };
                return PartialView("_ClubSummaryPartial", viewModel);
            }

            var tournamentClub = await _context.TournamentClubs
                .FirstOrDefaultAsync(tc => tc.ClubId == clubId);

            if (tournamentClub != null)
            {
                var viewModel = new ClubSummaryViewModel
                {
                    Club = null,
                    TournamentClub = tournamentClub
                };
                return PartialView("_ClubSummaryPartial", viewModel);
            }

            return NotFound();
        }

        [Authorize]
        public async Task<IActionResult> Details(string clubId)
        {
            var decryptedClubId = _encryptionService.DecryptToInt(clubId);

            if (decryptedClubId == null)
            {
                return NotFound();
            }

            var club = await _context.Club
                .FirstOrDefaultAsync(c => c.ClubId == decryptedClubId);

            if (club != null)
            {
                var viewModel = new ClubDetailsViewModel
                {
                    Club = club,
                    TournamentClub = null
                };
                return View("Details", viewModel);
            }

            var tournamentClub = await _context.TournamentClubs
                .FirstOrDefaultAsync(tc => tc.ClubId == decryptedClubId);

            if (tournamentClub != null)
            {
                var viewModel = new ClubDetailsViewModel
                {
                    Club = null,
                    TournamentClub = tournamentClub
                };
                return View("Details", viewModel);
            }

            return NotFound();
        }




        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> RejoinSeason(string clubId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var divisionId = (user as SportsMember)?.DivisionId ??
                             (user as SportsMember)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            var decryptedClubId = _encryptionService.DecryptToInt(clubId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
                return View();
            }

            var existingClub = await _context.Club.FindAsync(decryptedClubId);

            if (existingClub == null)
            {
                ModelState.AddModelError(string.Empty, "Club not found.");
                return View();
            }

            var existingStanding = await _context.Standing.FirstOrDefaultAsync(s => s.LeagueId == currentLeague.LeagueId && s.ClubId == existingClub.ClubId);

            if (existingStanding != null)
            {
                TempData["Message"] = $"{existingClub.ClubName} has already joined the current league.";
                return RedirectToAction("ClubsBackOffice");
            }

            existingClub.LeagueId = currentLeague.LeagueId;
            existingClub.IsActive = true;
            existingClub.Status = ClubStatus.Active;

             var matchResultsReport = await _context.MatchResultsReports
                .Where(m => m.Season.IsCurrent)
                .Include(m => m.Season)
                .FirstOrDefaultAsync();


            var newStanding = new Standing
            {
                LeagueId = currentLeague.LeagueId,
                ClubId = existingClub.ClubId,
                CreatedDateTime = DateTime.Now,
                ModifiedDateTime = DateTime.Now,
                CreatedById = userId,
                ModifiedById = userId,
                Position = 0,
                Draw = 0,
                Points = 0,
                MatchPlayed = 0,
                GoalDifference = 0,
                GoalsConceded = 0,
                GoalsScored = 0,
                Lose = 0,
                Wins = 0,
                DivisionId = divisionId
            };

            _context.Add(newStanding);
            await _context.SaveChangesAsync();

            var clubTransferReport = new ClubTransferReport
            {
                LeagueId = currentLeague.LeagueId,
                ClubId = existingClub.ClubId,
                OverallTransfersCount = 0,
                OutgoingTransfersCount = 0,
                IncomingTransfersCount = 0,
                SuccessfulIncomingTransfersCount = 0,
                SuccessfulOutgoingTransfersCount = 0,
                RejectedIncomingTransfersCount = 0,
                RejectedOutgoingTransfersCount = 0,
                NotActionedIncomingTransferCount = 0,
                NotActionedOutgoigTransferCount = 0,
                OutgoingTransferRate = 0,
                IncomingTransferRate = 0,
                SuccessfullIncomingTransferRate = 0,
                SuccessfullOutgoingTransferRate = 0,
                RejectedIncomingTransferRate = 0,
                RejectedOutgoingTransferRate = 0,
                NotActionedIncomingTransferRate = 0,
                NotActionedOutgoingTransferRate= 0,
                DivisionId = divisionId
            };

            var clubPerformanceReport = new ClubPerformanceReport
            {
                ClubId = existingClub.ClubId,
                LeagueId = currentLeague.LeagueId,
                GamesToPlayCount = 0,
                GamesPlayedCount = 0,
                GamesNotPlayedCount = 0,
                GamesWinCount = 0,
                GamesLoseCount = 0,
                GamesDrawCount = 0,
                GamesPlayedRate = 0,
                GamesNotPlayedRate = 0,
                GamesWinRate = 0,
                GamesDrawRate = 0,
                GamesLoseRate = 0,
                DivisionId = divisionId
            };


            var existingSubscription = await _context.Subscriptions
                .Where(es => es.ClubId == existingClub.ClubId)
                .FirstAsync();

            existingSubscription.SubscriptionPlan = SubscriptionPlan.Basic;
            existingSubscription.Amount = 0;

            _context.Update(existingSubscription);

            matchResultsReport.ExpectedResultsCount++;

            _context.Add(clubTransferReport);
            _context.Add(clubPerformanceReport);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{existingClub.ClubName} has successfully rejoined the current league.";

            await _activityLogger.Log($"Rejoined season {currentLeague.LeagueYears} on behalf of {existingClub.ClubName}", user.Id);

            await _requestLogService.LogSuceededRequest("Successfully rejoined current season", StatusCodes.Status200OK);

            return RedirectToAction("ClubsBackOffice");
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            return View();
        }


        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClubViewModel viewModel, IFormFile? ClubBadges)
        {
            if (!User.Identity.IsAuthenticated)
            {
                string errorMessage = "You need to have a valid account and be logged into this system to add a club";
                return RedirectToAction("ErrorPage", "Home", new { errorMessage = errorMessage });
            }

            if (!User.IsInRole("Sport Administrator"))
            {
                string errorMessage = "Only a system administrator can add a club into this system";
                return RedirectToAction("ErrorPage", "Home", new { errorMessage = errorMessage });
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var userId = user.Id;

                var divisionId = (user as SportsMember)?.DivisionId;

                if (divisionId != null) 
                {
                    var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

                    var matchResultsReport = await _context.MatchResultsReports
                        .Where(m => m.Season.IsCurrent && m.DivisionId == divisionId)
                        .Include(m => m.Season)
                        .FirstOrDefaultAsync();

                    if (currentLeague == null)
                    {
                        ModelState.AddModelError(string.Empty, "No current league found.");
                        return View(viewModel);
                    }

                    var newClub = new Club
                    {
                        LeagueId = currentLeague.LeagueId,
                        ClubName = viewModel.ClubName,
                        ClubLocation = viewModel.ClubLocation,
                        ClubAbbr = viewModel.ClubAbbr,
                        CreatedById = userId,
                        ModifiedById = userId,
                        ClubHistory = viewModel.ClubHistory,
                        ClubSummary = viewModel.ClubSummary,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        Status = ClubStatus.Active,
                        IsActive = true,
                        ClubCode = GenerateClubCode(viewModel),
                        Email = viewModel.Email,
                        ClubBadge = "Images/placeholder_club_badge.jpg",
                        DivisionId = divisionId
                    };

                    if (ClubBadges != null && ClubBadges.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ClubBadges);
                        newClub.ClubBadge = uploadedImagePath;
                    }

                    _context.Add(newClub);
                    await _context.SaveChangesAsync();

                    await _activityLogger.Log($"Added {newClub.ClubName} as a new club", user.Id);

                    var newStanding = new Standing
                    {
                        LeagueId = currentLeague.LeagueId,
                        ClubId = newClub.ClubId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        CreatedById = userId,
                        ModifiedById = userId,
                        Position = 0,
                        Draw = 0,
                        Points = 0,
                        MatchPlayed = 0,
                        GoalDifference = 0,
                        GoalsConceded = 0,
                        GoalsScored = 0,
                        Lose = 0,
                        Wins = 0,
                        DivisionId = divisionId
                    };

                    var clubTransferReport = new ClubTransferReport
                    {
                        LeagueId = currentLeague.LeagueId,
                        ClubId = newClub.ClubId,
                        OverallTransfersCount = 0,
                        OutgoingTransfersCount = 0,
                        IncomingTransfersCount = 0,
                        SuccessfulIncomingTransfersCount = 0,
                        SuccessfulOutgoingTransfersCount = 0,
                        RejectedIncomingTransfersCount = 0,
                        RejectedOutgoingTransfersCount = 0,
                        NotActionedIncomingTransferCount = 0,
                        NotActionedOutgoigTransferCount = 0,
                        OutgoingTransferRate = 0,
                        IncomingTransferRate = 0,
                        SuccessfullIncomingTransferRate = 0,
                        SuccessfullOutgoingTransferRate = 0,
                        RejectedIncomingTransferRate = 0,
                        RejectedOutgoingTransferRate = 0,
                        NotActionedIncomingTransferRate = 0,
                        NotActionedOutgoingTransferRate = 0,
                        DivisionId = divisionId
                    };

                    var clubPerformanceReport = new ClubPerformanceReport
                    {
                        LeagueId = currentLeague.LeagueId,
                        ClubId = newClub.ClubId,
                        GamesToPlayCount = 0,
                        GamesPlayedCount = 0,
                        GamesNotPlayedCount = 0,
                        GamesWinCount = 0,
                        GamesLoseCount = 0,
                        GamesDrawCount = 0,
                        GamesPlayedRate = 0,
                        GamesNotPlayedRate = 0,
                        GamesWinRate = 0,
                        GamesDrawRate = 0,
                        GamesLoseRate = 0,
                        DivisionId = divisionId
                    };


                    var newSubscription = new Subscription
                    {
                        ClubId = newClub.ClubId,
                        Amount = 0,
                        SubscriptionPlan = SubscriptionPlan.Basic,
                        SubscriptionStatus = SubscriptionStatus.Active,
                    };

                    matchResultsReport.ExpectedResultsCount++;

                    _context.Add(newSubscription);
                    _context.Add(newStanding);
                    _context.Add(clubPerformanceReport);
                    _context.Add(clubTransferReport);
                    await _context.SaveChangesAsync();


                    TempData["Message"] = $"{viewModel.ClubName} has been added successfully.";

                    await _activityLogger.Log($"Created {viewModel.ClubName} during season {currentLeague.LeagueYears}", user.Id);

                    await _requestLogService.LogSuceededRequest("Successfully created a new club", StatusCodes.Status200OK);

                    return RedirectToAction(nameof(ClubsBackOffice));
                }
                else
                {
                    var newClub = new Club
                    {
                        ClubName = viewModel.ClubName,
                        ClubLocation = viewModel.ClubLocation,
                        ClubAbbr = viewModel.ClubAbbr,
                        CreatedById = userId,
                        ModifiedById = userId,
                        ClubHistory = viewModel.ClubHistory,
                        ClubSummary = viewModel.ClubSummary,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        Status = ClubStatus.Active,
                        IsActive = true,
                        ClubCode = GenerateClubCode(viewModel),
                        Email = viewModel.Email,
                        ClubBadge = "Images/placeholder_club_badge.jpg",
                        DivisionId = divisionId
                    };

                    if (ClubBadges != null && ClubBadges.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ClubBadges);
                        newClub.ClubBadge = uploadedImagePath;
                    }

                    _context.Add(newClub);
                    await _context.SaveChangesAsync();

                    await _activityLogger.Log($"Added {newClub.ClubName} as a new club", user.Id);

                    var newSubscription = new Subscription
                    {
                        ClubId = newClub.ClubId,
                        Amount = 0,
                        SubscriptionPlan = SubscriptionPlan.Basic,
                        SubscriptionStatus = SubscriptionStatus.Active,
                    };

                    _context.Add(newSubscription);
                    await _context.SaveChangesAsync();

                    var newClubAdministrator = await _context.UserBaseModel
                        .Where(nca => nca.Id == user.Id)
                        .FirstOrDefaultAsync();


                    var result = await _userManager.UpdateAsync(newClubAdministrator);

                    if (result.Succeeded)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(newClubAdministrator);
                        await _userManager.RemoveFromRolesAsync(newClubAdministrator, currentRoles);
                        await _userManager.AddToRoleAsync(newClubAdministrator, "Club Administrator");

                        string accountCreationEmailBody = $"Hello {user.FirstName} {user.LastName},<br><br>";
                        accountCreationEmailBody += $"You have successfully created your new club!<br><br>";
                        accountCreationEmailBody += $"You have been assigned a new role of being {newClub.ClubName} Administrator.<br><br>";
                        accountCreationEmailBody += $"!Important: You won't be able to access your previous role. If you are willing to switch to your previous role, please contact the system administrator.<br>";
                        accountCreationEmailBody += "Please note that you will still be able to log onto our system using your previous credentials.<br><br>";
                        accountCreationEmailBody += "Thank you!";

                        BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(newClubAdministrator.Email, "Welcome to Diski360", accountCreationEmailBody, "Diski 360"));


                        await _activityLogger.Log($"Updated {newClubAdministrator.FirstName} {newClubAdministrator.LastName} role to a club administrator for {newClub.ClubName}", user.Id);

                        await _context.SaveChangesAsync();

                    }

                    TempData["Message"] = $"{viewModel.ClubName} has been added successfully.";

                    await _requestLogService.LogSuceededRequest("Successfully created a new club with no division", StatusCodes.Status200OK);

                    return RedirectToAction("", "");
                }
            }

            await _requestLogService.LogFailedRequest("Failed to create a new club", StatusCodes.Status500InternalServerError);

            return View(viewModel);
        }

        private string GenerateClubCode(ClubViewModel viewModel)
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            var clubNameAbbreviation = viewModel.ClubName.Substring(0, 3).ToUpper();

            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var randomLetters = new string(Enumerable.Repeat(letters, 3)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{year}{month}{day}{clubNameAbbreviation}{randomLetters}";
        }


        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> ClubManagerDetails(string id)
        {
            if (id == null || _context.Club == null)
            {
                return NotFound();
            }

            var clubManager = await _context.ClubManager
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clubManager == null)
            {
                return NotFound();
            }

            return View(clubManager);
        }


        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> ClubPlayerDetails(string id)
        {
            if (id == null || _context.Club == null)
            {
                return NotFound();
            }

            var clubPlayer = await _context.Player
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clubPlayer  == null)
            {
                return NotFound();
            }

            return View(clubPlayer);
        }


        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> SuspendClub(string clubId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedClubId = _encryptionService.DecryptToInt(clubId);

            var club = await _context.Club
                .Where( c => c.ClubId == decryptedClubId)
                .FirstOrDefaultAsync(); 

            if (club.IsSuspended == true)
            {
                TempData["Message"] = $"This club is already suspended.";

                return RedirectToAction(nameof(ClubsBackOffice));
            }


            club.IsSuspended = true;
            club.ModifiedById = user.Id;
            club.ModifiedDateTime = DateTime.Now;

            _context.Update(club);
            await _context.SaveChangesAsync();


            TempData["Message"] = $"You have successfully suspended {club.ClubName} and now they won't be able to access all features of the system.";

            await _requestLogService.LogSuceededRequest("Successfully suspended a club", StatusCodes.Status200OK);

            await _activityLogger.Log($"Suspended {club.ClubName}", user.Id);

            return RedirectToAction(nameof(ClubsBackOffice));
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> UnsuspendClub(string clubId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedClubId = _encryptionService.DecryptToInt(clubId);

            var club = await _context.Club
                .Where(c => c.ClubId == decryptedClubId)
                .FirstOrDefaultAsync();

            if (club.IsSuspended == false)
            {
                TempData["Message"] = $"This club is already active";

                return RedirectToAction(nameof(ClubsBackOffice));
            }

            club.IsSuspended = false;
            club.ModifiedById = user.Id;
            club.ModifiedDateTime = DateTime.Now;

            _context.Update(club);
            await _context.SaveChangesAsync();

            await _requestLogService.LogSuceededRequest("Successfully unsuspended a club", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully unsuspended {club.ClubName} and now they will be able to access some system features due to te decision you have made.";
            await _activityLogger.Log($"Unsuspended {club.ClubName}", user.Id);
            return RedirectToAction(nameof(ClubsBackOffice));
        }


        [HttpGet]
        [Authorize(Roles = ("Sport Administrator, Club Administrator"))]
        public async Task<IActionResult> Update(string clubId)
        {
            var logMessages = new List<string>();

            var decryptedClubId = _encryptionService.DecryptToInt(clubId);

            if (decryptedClubId == null || _context.Club == null)
            {
                var message = $"Edit called with null id or Club context is null";
                Console.WriteLine(message);
                logMessages.Add(message);
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var club = await _context.Club.FindAsync(decryptedClubId);

            if (club == null)
            {
                var message = $"Club with id {decryptedClubId} not found";
                Console.WriteLine(message);
                logMessages.Add(message);
                TempData["LogMessages"] = logMessages;
                return NotFound();
            }

            var clubViewModel = new UpdateClubViewModel
            {
                ClubId = decryptedClubId,
                ClubName = club.ClubName,
                Email = club.Email,
                ClubLocation = club.ClubLocation,
                ClubAbbr = club.ClubAbbr,
                ClubBadges = club.ClubBadge,
                ClubHistory = club.ClubHistory,
                ClubSummary = club.ClubSummary
            };

            TempData["LogMessages"] = logMessages;
            return View(clubViewModel);
        }


        [Authorize(Roles = ("Sport Administrator, Club Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateClubViewModel viewModel, IFormFile ClubBadgeFile)
        {
            var logMessages = new List<string>();

            var user = await _userManager.GetUserAsync(User);

            if (viewModel.ClubId == null)
            {
                return NotFound();
            }

            if (ValidateUpdatedProperties(viewModel))
            {
                var club = await _context.Club.FindAsync(viewModel.ClubId);

                try
                {

                    if (club == null)
                    {
                        return NotFound();
                    }

                    club.ClubAbbr = viewModel.ClubAbbr;
                    club.ClubName = viewModel.ClubName;
                    club.Email = viewModel.Email;
                    club.ClubLocation = viewModel.ClubLocation;
                    club.ClubHistory = viewModel.ClubHistory;
                    club.ClubSummary = viewModel.ClubSummary;
                    club.ModifiedById = user.Id;
                    club.ModifiedDateTime = DateTime.Now;

                    if (ClubBadgeFile != null && ClubBadgeFile.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ClubBadgeFile);
                        club.ClubBadge = uploadedImagePath;
                    }

                    _context.Update(club);
                    await _context.SaveChangesAsync();

                    await _activityLogger.Log($"Updated {club.ClubName} information", user.Id);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ClubExists(viewModel.ClubId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }

                await _activityLogger.Log($"Updated {club.ClubName}information", user.Id);

                if (User.IsInRole("Club Administrator"))
                {
                    TempData["Message"] = $"{club.ClubName} information has been updated successfully.";
                    return RedirectToAction(nameof(MyClub));
                }
                else if (User.IsInRole("Sport Administrator"))
                {
                    TempData["Message"] = $"{club.ClubName} information has been updated successfully.";
                    return RedirectToAction(nameof(ClubsBackOffice));
                }
                else if (User.IsInRole("Sport Coordinator"))
                {
                    TempData["Message"] = $"{club.ClubName} information has been updated successfully.";
                    return RedirectToAction(nameof(ClubsBackOffice));
                }

                await _requestLogService.LogSuceededRequest("Successfully updated a club", StatusCodes.Status200OK);

            }

            await _requestLogService.LogFailedRequest("Failed to update a club", StatusCodes.Status500InternalServerError);

            var existingClub = await _context.Club.FindAsync(viewModel.ClubId);

            viewModel.ClubBadges = existingClub.ClubBadge;

            return View(viewModel);
        }

        private bool ValidateUpdatedProperties(UpdateClubViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.ClubName, new ValidationContext(viewModel, null, null) { MemberName = "ClubName" }, validationResults);
            Validator.TryValidateProperty(viewModel.Email, new ValidationContext(viewModel, null, null) { MemberName = "Email" }, validationResults);
            Validator.TryValidateProperty(viewModel.ClubLocation, new ValidationContext(viewModel, null, null) { MemberName = "ClubLocation" }, validationResults);
            Validator.TryValidateProperty(viewModel.ClubSummary, new ValidationContext(viewModel, null, null) { MemberName = "ClubSummary" }, validationResults);
            return validationResults.Count == 0;
        }

        private bool ClubExists(int id)
        {
            return _context.Club.Any(e => e.ClubId == id);
        }


        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Club == null)
            {
                return NotFound();
            }

            var club = await _context.Club
                .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null)
            {
                return NotFound();
            }

            return View(club);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Club == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Club'  is null.");
            }
            var club = await _context.Club.FindAsync(id);
            if (club != null)
            {
                _context.Club.Remove(club);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
