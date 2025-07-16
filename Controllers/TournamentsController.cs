using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.ViewModels;
using MyField.Services;
using System.ComponentModel.DataAnnotations;

namespace MyField.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        public readonly UserManager<UserBaseModel> _userManager;
        private readonly FileUploadService _fileUploadService;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public TournamentsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            FileUploadService fileUploadService,
            IActivityLogger activityLogger,
            EmailService emailService,
            IEncryptionService encryptionService,
            RequestLogService requestLogService)

        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTournamentRules(int tournamentId)
        {
            var rules = await _context.TournamentRules
                                .Where(r => r.TournamentId == tournamentId)
                                .Select(r => new { r.RuleDescription })
                                .ToListAsync();

            return Json(rules);
        }


        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpGet]
        public async Task<IActionResult> TournamentParticipants(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournamentClubs = await _context.TournamentClubs
            .Where(tc => tc.TournamentId == decryptedTournamentId)
            .Include(tc => tc.CreatedBy)
            .Include(tc => tc.ModifiedBy)
            .Include(tc => tc.Tournament)
            .ToListAsync();

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            ViewBag.TournamentName = tournament.TournamentName;

            return View(tournamentClubs);
        }


        [HttpGet]
        public async Task<IActionResult> TournamentRules(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournamentRules = await _context.TournamentRules
                .Where(tr => tr.TournamentId == decryptedTournamentId)
                .OrderByDescending(tr => tr.CreatedDateTime)
                .ToListAsync();

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            ViewBag.TournamentName = tournament?.TournamentName;
            ViewBag.TournamentId = tournamentId;

            return View(tournamentRules);
        }


        public async Task<IActionResult> JoinedSuccessfully(string tournamentId, string clubName)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            ViewBag.TournamentName = tournament.TournamentName;
            ViewBag.ClubName = clubName;

            return View();
        }


        public async Task<IActionResult> AllTournaments(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var tournaments = await _context.Tournament
                .Where(t => t.DivisionId == decryptedDivisionId && t.IsPublished)
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return PartialView("_TournamentsPartial", tournaments);
        }

        [HttpGet]
        public async Task<IActionResult> TournamentsBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var tournaments = await _context.Tournament
                .Where(t => t.DivisionId == divisionId)
                .OrderByDescending(t => t.CreatedDateTime)
                .ToListAsync();

            var division = await _context.Divisions.Where(d => d.DivisionId == divisionId).FirstOrDefaultAsync();

            ViewBag.DivisionName = division.DivisionName;

            return View(tournaments);
        }

        [HttpGet]
        public async Task<IActionResult> Tournaments(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            ViewBag.Leagues = await _context.League
                .Where(l => l.DivisionId == decryptedDivisionId)
                .OrderByDescending(l => l.CreatedDateTime)
                .ToListAsync();

            ViewBag.DivisionId = divisionId;

            return PartialView("TournamentsPartial");
        }

        public async Task<IActionResult> LeagueTournaments()
        {
            return PartialView("TournamentsPartial");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ParticipatingClubs(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var participatingClubs = await _context.TournamentClubs
                .Where(pc => pc.TournamentId == decryptedTournamentId &&
                pc.HasJoined == true &&
                pc.IsEliminated == false)
                .ToListAsync();

            return PartialView("_ParticipatingClubsPartial", participatingClubs);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TournamentRulesFans(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var participatingClubs = await _context.TournamentRules
                .Where(pc => pc.TournamentId == decryptedTournamentId)
                .ToListAsync();

            return PartialView("_TournamentRulesPartial", participatingClubs);
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpGet]
        public async Task<IActionResult> Score(string fixtureId, string homeClubName, string awayClubName, DateTime kickOfDate, DateTime kickOffTime, string homeTeamBadge, string awayTeamBadge, string location, string homeTeamClubId, string awayTeamClubId)
        {
            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);
            var decryptedHomeTeamClubId = _encryptionService.DecryptToInt(homeTeamClubId);
            var decryptedAwayTeamClubId = _encryptionService.DecryptToInt(awayTeamClubId);

            var fixture = await _context.TournamentFixtures
                .Where(f => f.FixtureId == decryptedFixtureId &&
                f.HomeTeamClubId == decryptedHomeTeamClubId &&
                f.AwayTeamClubId == decryptedAwayTeamClubId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            var homeTeam = fixture.HomeTeam;

            var awayTeam = fixture.AwayTeam;

            var viewModel = new MatchResultsViewModel
            {
                FixtureId = decryptedFixtureId,
                HomeTeamId = decryptedHomeTeamClubId,
                AwayTeamId = decryptedAwayTeamClubId,
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
        [HttpGet]
        public async Task<IActionResult> PenaltiesScore(string resultsId)
        {
            return View();
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpGet]
        public async Task<IActionResult> PenaltiesScore(PenaltiesViewModel viewModel)
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        public async Task<IActionResult> Upload(string tournamentId)
        {
            var user = await _userManager.GetUserAsync(User);
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            var upcomingFixtures = await _context.TournamentFixtures
                .Where(f => (f.FixtureStatus == FixtureStatus.Upcoming ||
                f.FixtureStatus == FixtureStatus.Live ||
                f.FixtureStatus == FixtureStatus.Interrupted) &&
                            f.TournamentId == decryptedTournamentId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .OrderBy(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .ToListAsync();

            ViewBag.TournamentId = tournamentId;

            return View(upcomingFixtures);
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpPost]
        public async Task<IActionResult> Upload(MatchResultsViewModel viewModel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.GetUserAsync(User);

                var fixture = await _context.TournamentFixtures
                    .Where(f => f.FixtureId == viewModel.FixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .Include(f => f.Tournament)
                    .FirstOrDefaultAsync();


                fixture.FixtureStatus = FixtureStatus.Ended;

                _context.Update(fixture);

                var userId = user?.Id;

                if (user == null || userId == null)
                {
                    throw new InvalidOperationException("User not authenticated or user ID not found.");
                }

                var newMatchResults = new TournamentMatchResults
                {
                    TournamentId = fixture.TournamentId,
                    HomeTeamClubId = fixture.HomeTeam.ClubId,
                    AwayTeamClubId = fixture.AwayTeam.ClubId,
                    CreatedById = userId,
                    ModifiedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    MatchDate = viewModel.MatchDate,
                    MatchTime = viewModel.MatchTime,
                    FixtureId = viewModel.FixtureId,
                    HomeTeamScore = viewModel.HomeTeamScore,
                    AwayTeamScore = viewModel.AwayTeamScore,
                    Location = fixture.Location,
                    HomeTeamTournamentId = fixture.TournamentId,
                    AwayTeamTournamentId = fixture.TournamentId
                };

                _context.Add(newMatchResults);
                await _context.SaveChangesAsync();

                var newlyUpdatedMatchResults = await _context.TournamentMatchResults
                    .Where(numr => numr.Equals(newMatchResults))
                    .FirstOrDefaultAsync();

                var encryptedResultsId = _encryptionService.Encrypt(newlyUpdatedMatchResults.ResultsId);

                if (newlyUpdatedMatchResults.HomeTeamScore == newlyUpdatedMatchResults.AwayTeamScore)
                {
                    return RedirectToAction(nameof(PenaltiesScore), new { resultsId = encryptedResultsId });
                }

                var homeHeadToHead = new HeadTohead
                {
                    HeadToHeadDate = viewModel.MatchDate,
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
                    HeadToHeadDate = viewModel.MatchDate,
                    ClubId = fixture.AwayTeam.ClubId,
                    HomeTeamId = fixture.HomeTeam.ClubId,
                    AwayTeamId = fixture.AwayTeam.ClubId,
                    MatchResults = viewModel.AwayTeamScore > viewModel.HomeTeamScore ? "W" :
                                  viewModel.AwayTeamScore == viewModel.HomeTeamScore ? "D" :
                                   "L",
                    AwayTeamGoals = viewModel.AwayTeamScore,
                    HomeTeamGoals = viewModel.HomeTeamScore
                };
                _context.Add(awayHeadToHead);

                await _context.SaveChangesAsync();


                await transaction.CommitAsync();

                await _activityLogger.Log($"Uploaded match results for a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}. The recorded final score is {viewModel.HomeTeamScore}-{viewModel.AwayTeamScore}.", user.Id);

                await _requestLogService.LogSuceededRequest("Match results uploaded successfully.", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully uploaded match results for a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}.";

                var tournamentId = _encryptionService.Encrypt(fixture.TournamentId);

                return RedirectToAction(nameof(KnockoutBackOffice), new { tournamentId = tournamentId });
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


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> FixturesBackOffice(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournamentFixtures = await _context.TournamentFixtures
                .Include(k => k.HomeTeam)
                .Include(k => k.AwayTeam)
                .Include(k => k.Tournament)
                .Include(k => k.CreatedBy)
                .Include(k => k.ModifiedBy)
                .Where(tf => tf.TournamentId == decryptedTournamentId &&
                tf.FixtureStatus == FixtureStatus.Upcoming)
                .ToListAsync();

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            ViewBag.TournamentName = tournament.TournamentName;
            ViewBag.TournamentId = tournamentId;

            return View(tournamentFixtures);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> KnockoutBackOffice(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var knockout = await _context.TournamentMatchResults
                .Include(k => k.TournamentFixture)
                .ThenInclude(k => k.HomeTeam)
                .Include(k => k.TournamentFixture)
                .ThenInclude(k => k.AwayTeam)
                .Include(k => k.Tournament)
                .Where(k => k.TournamentId == decryptedTournamentId)
                .ToListAsync();

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            ViewBag.TournamentName = tournament.TournamentName;
            ViewBag.TournamentId = tournamentId;

            return View(knockout);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Fixtures(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournamentFixtures = await _context.TournamentFixtures
                .Include(k => k.HomeTeam)
                .Include(k => k.AwayTeam)
                .Include(k => k.Tournament)
                .Where(tf => tf.TournamentId == decryptedTournamentId &&
                tf.FixtureStatus == FixtureStatus.Upcoming ||
                tf.FixtureStatus == FixtureStatus.Live ||
                tf.FixtureStatus == FixtureStatus.Postponed ||
                tf.FixtureStatus == FixtureStatus.Interrupted)
                .ToListAsync();

            return PartialView("_TournamentFixturesPartial", tournamentFixtures);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Knockout(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var knockout = await _context.TournamentMatchResults
                .Include(k => k.HomeTeam)
                .Include(k => k.AwayTeam)
                .Where(k => k.TournamentId == decryptedTournamentId)
                .ToListAsync();

            return PartialView("_KnockoutPartial", knockout);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            return View(tournament);
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        [HttpGet]
        public async Task<IActionResult> TournamentDetails(string tournamentId)
        {
            if (tournamentId == null)
            {
                return NotFound();
            }

            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            var viewModel = new TournamentDetailsViewModel
            {
                TournamentId = tournament.TournamentId,
                TournamentName = tournament.TournamentName,
                TournamentDescription = tournament.TournamentDescription,
                TournamentLocation = tournament.TournamentLocation,
                TournamentImage = tournament.TournamentImage,
                NumberOfTeams = tournament.NumberOfTeams,
                SponsorName = tournament.SponsorName,
                SponsorContactDetails = tournament.SponsorContactDetails,
                JoiningDueDate = tournament.JoiningDueDate,
                JoiningFee = tournament.JoiningFee,
                Sponsorship = tournament.Sponsorship,
                StartDate = tournament.StartDate,
                TrophyImage = tournament.TrophyImage
            };

            ViewBag.TournamentName = tournament.TournamentName;

            return View(viewModel);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> NewTournament()
        {
            var tournamentTypes = Enum.GetValues(typeof(TournamentType))
                                          .Cast<TournamentType>()
                                          .Select(t => new SelectListItem
                                          {
                                              Value = t.ToString(),
                                              Text = t.ToString().Replace("_", " ")
                                          })
                                          .ToList();

            ViewBag.TournamentTypes = tournamentTypes;

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewTournament(NewTournamentViewModel viewModel, IFormFile TournamentImages, IFormFile TrophyImages)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                     (user as ClubManager)?.DivisionId ??
                     (user as Player)?.DivisionId ??
                     (user as SportsMember)?.DivisionId ??
                     (user as Officials)?.DivisionId ??
                     (user as DivisionManager)?.DivisionId;

                var newTournament = new Tournament
                {
                    TournamentName = viewModel.TournamentName,
                    TournamentDescription = viewModel.TournamentDescription,
                    StartDate = viewModel.StartDate,
                    JoiningFee = viewModel.JoiningFee,
                    TournamentStatus = TournamentStatus.Upcoming,
                    TournamentLocation = viewModel.TournamentLocation,
                    NumberOfTeams = viewModel.NumberOfTeams,
                    JoiningDueDate = viewModel.JoiningDueDate,
                    DivisionId = divisionId,
                    SponsorName = viewModel.SponsorName,
                    SponsorContactDetails = viewModel.SponsorContactDetails,
                    Sponsorship = viewModel.Sponsorship,
                    CreatedById = user.Id,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = user.Id,
                    ModifiedDateTime = DateTime.Now,
                    IsPublished = false,
                    TournamentType = viewModel.TournamentType
                };


                if (TournamentImages != null && TournamentImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(TournamentImages);
                    newTournament.TournamentImage = uploadedImagePath;
                }

                if (TrophyImages != null && TrophyImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(TrophyImages);
                    newTournament.TrophyImage = uploadedImagePath;
                }

                _context.Add(newTournament);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Created {viewModel.TournamentName} as a new tournament", user.Id);

                TempData["Message"] = $"{viewModel.TournamentName} has been created successfully.";

                await _requestLogService.LogSuceededRequest("Successfully created a new tournament", StatusCodes.Status200OK);

                var tournamentId = _encryptionService.Encrypt(newTournament.TournamentId);

                return RedirectToAction(nameof(AddTournamentRules), new { tournamentId });

            }

            await _requestLogService.LogFailedRequest("Failed to create a new tournament", StatusCodes.Status500InternalServerError);

            var tournamentTypes = Enum.GetValues(typeof(TournamentType))
                              .Cast<TournamentType>()
                              .Select(t => new SelectListItem
                              {
                                  Value = t.ToString(),
                                  Text = t.ToString().Replace("_", " ")
                              })
                              .ToList();

            ViewBag.TournamentTypes = tournamentTypes;

            return View(viewModel);

        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> JoinTournament(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var user = await _userManager.GetUserAsync(User);

            var clubId = (user as ClubAdministrator)?.ClubId;

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            var club = await _context.Club
                .Where(c => c.ClubId == clubId)
                .Include(c => c.Division)
                .Include(c => c.ClubManager)
                .FirstOrDefaultAsync();

            if (club != null)
            {
                var viewModel = new JoinTournamentViewModel
                {
                    ClubName = club.ClubName,
                    ClubId= club.ClubId,
                    ClubAbbr = club.ClubAbbr,
                    ClubBadge = club.ClubBadge,
                    ClubHistory = club.ClubHistory,
                    ClubSummary = club.ClubSummary,
                    DivisionId = club.Division.DivisionId,
                    ClubLocation = club.ClubLocation,
                    TournamentId = decryptedTournamentId,
                    ClubManagerName = $"{club.ClubManager?.FirstName} {club.ClubManager?.LastName}",
                    ClubManagerEmail = club.ClubManager?.Email,
                    ClubManagerPhone = club.ClubManager?.PhoneNumber,
                    ManagerProfilePicture = club.ClubManager?.ProfilePicture,
                    Email = club.Email
                };

                ViewBag.TournamentName = tournament.TournamentName;

                return View(viewModel);
            }
            else
            {
                var viewModel = new JoinTournamentViewModel
                {
                    TournamentId = decryptedTournamentId,
                };

                ViewBag.TournamentName = tournament.TournamentName;

                return View(viewModel);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinTournament(JoinTournamentViewModel viewModel, IFormFile ClubBadges, IFormFile ManagerProfilePictures)
        {
            var user = await _userManager.GetUserAsync(User);

            var tournament = await _context.Tournament
                .FirstOrDefaultAsync(t => t.TournamentId == viewModel.TournamentId);

            bool clubAlreadyExists = await _context.TournamentClubs
                .AnyAsync(tc => tc.TournamentId == viewModel.TournamentId &&
                                tc.ClubName.ToLower().Trim() == viewModel.ClubName.ToLower().Trim());

            if (clubAlreadyExists)
            {
                TempData["Message"] = $"Your club has already joined this tournament. Please visit your tournaments portal for more information.";
                ViewBag.TournamentName = tournament?.TournamentName;
                return View(viewModel);
            }

            _context.Club.Attach(new Club { ClubId = viewModel.ClubId });

            var tournamentClub = new TournamentClubs
            {
                ClubId = viewModel.ClubId,
                TournamentId = viewModel.TournamentId,
                ClubAbbr = viewModel.ClubAbbr,
                ClubHistory = viewModel.ClubHistory,
                ClubLocation = viewModel.ClubLocation,
                ClubManagerEmail = viewModel.ClubManagerEmail,
                ClubManagerName = viewModel.ClubManagerName,
                ClubName = viewModel.ClubName,
                ClubSummary = viewModel.ClubSummary,
                ClubManagerPhone = viewModel.ClubManagerPhone,
                DivisionId = viewModel.DivisionId,
                Email = viewModel.Email,
                CreatedById = user.Id,
                CreatedDateTime = DateTime.Now,
                ModifiedById = user.Id,
                ModifiedDateTime = DateTime.Now,
                HasJoined = false,
                IsEliminated = false,
            };

            if (ClubBadges != null && ClubBadges.Length > 0)
            {
                var uploadedImagePath = await _fileUploadService.UploadFileAsync(ClubBadges);
                tournamentClub.ClubBadge = uploadedImagePath;
            }
            else
            {
                tournamentClub.ClubBadge = viewModel.ClubBadge;
            }

            if (ManagerProfilePictures != null && ManagerProfilePictures.Length > 0)
            {
                var uploadedImagePath = await _fileUploadService.UploadFileAsync(ManagerProfilePictures);
                tournamentClub.ManagerProfilePicture = uploadedImagePath;
            }
            else
            {
                tournamentClub.ManagerProfilePicture = viewModel.ManagerProfilePicture;
            }

            _context.Add(tournamentClub);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Created a new club for the {tournament.TournamentName} tournament.", user.Id);
            await _requestLogService.LogSuceededRequest("Successfully created a new tournament club", StatusCodes.Status200OK);

            return RedirectToAction(nameof(AddTournamentRules));
        }


        [Authorize(Roles = "Sport Administrator")]
        [HttpGet]
        public async Task<IActionResult> AddTournamentRules(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            ViewBag.TournamentName = tournament.TournamentName;

            var viewModel = new TournamentRulesViewModel
            {
                TournamentId = decryptedTournamentId,
                TournamentName = tournament.TournamentName
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTournamentRules(int tournamentId, string ruleDescription)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var tournament = await _context.Tournament
                    .Where(t => t.TournamentId == tournamentId)
                    .FirstOrDefaultAsync();

                var newTournamentRule = new TournamentRules
                {
                    TournamentId = tournamentId,
                    RuleDescription = ruleDescription,
                    CreatedById = user.Id,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = user.Id,
                    ModifiedDateTime = DateTime.Now
                };

                _context.Add(newTournamentRule);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Created a new rule for tournament {tournament.TournamentName}.", user.Id);
                await _requestLogService.LogSuceededRequest("Successfully created a new tournament rule", StatusCodes.Status200OK);

                return Ok(new { success = true, ruleDescription = newTournamentRule.RuleDescription });
            }

            await _requestLogService.LogFailedRequest("Failed to create a new tournament rule", StatusCodes.Status500InternalServerError);
            return Json(new { success = false, message = "Failed to create a new tournament rule!" });
        }

        [HttpGet]
        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        public async Task<IActionResult> NewFixture(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            var viewModel = new TournamentFixtureViewModel
            {
                TournamentId = decryptedTournamentId
            };

            ViewBag.TournamentClubs = new SelectList(await _context.TournamentClubs
                .Where(tc => tc.TournamentId == decryptedTournamentId)
                .ToListAsync(), "ClubId", "ClubName");

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        public async Task<IActionResult> NewFixture(TournamentFixtureViewModel viewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var tournament = await _context.Tournament
                    .Where(t => t.TournamentId == viewModel.TournamentId)
                    .FirstOrDefaultAsync();

                var tournamentFixture = new TournamentFixture
                {
                    TournamentId = viewModel.TournamentId,
                    HomeTeamClubId = viewModel.HomeTeamClubId,
                    AwayTeamClubId = viewModel.AwayTeamClubId,
                    HomeTeamTournamentId = viewModel.TournamentId,
                    AwayTeamTournamentId = viewModel.TournamentId,
                    KickOffDate = viewModel.KickOffDate,
                    KickOffTime = viewModel.KickOffTime,
                    Location = viewModel.Location,
                    CreatedById = user.Id,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = user.Id,
                    ModifiedDateTime = DateTime.Now,
                    FixtureStatus = FixtureStatus.Upcoming,
                    FixtureRound = viewModel.FixtureRound
                };

                _context.Add(tournamentFixture);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"You have successfully generated fixtures for tournament {tournament.TournamentName} {viewModel.FixtureRound}";

                await _activityLogger.Log($"Created a fixture for tournament {tournament.TournamentName}.", user.Id);
                await _requestLogService.LogSuceededRequest("Successfully created a new tournament fixture", StatusCodes.Status200OK);

                var encryptedTournamentId = _encryptionService.Encrypt(viewModel.TournamentId);

                return RedirectToAction(nameof(FixturesBackOffice), new { tournamentId = encryptedTournamentId });
            }
            catch(Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to create fixture: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }

            return View(viewModel);
        }


        [HttpGet]
        [Authorize(Roles = "Sport Administrator")]
        public async Task<IActionResult> UpdateTournament(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            var viewModel = new UpdateTournamentViewModel
            {
                TournamentId = tournament.TournamentId,
                TournamentName = tournament.TournamentName,
                TournamentDescription = tournament.TournamentDescription,
                TournamentLocation = tournament.TournamentLocation,
                TournamentImage = tournament.TournamentImage,
                NumberOfTeams = tournament.NumberOfTeams,
                SponsorName = tournament.SponsorName,
                SponsorContactDetails = tournament.SponsorContactDetails,
                JoiningDueDate = tournament.JoiningDueDate,
                JoiningFee = tournament.JoiningFee,
                Sponsorship = tournament.Sponsorship,
                StartDate = tournament.StartDate,
                TrophyImage = tournament.TrophyImage,
                TournamentType = tournament.TournamentType
            };

            var tournamentTypes = Enum.GetValues(typeof(TournamentType))
                              .Cast<TournamentType>()
                              .Select(t => new SelectListItem
                              {
                                  Value = t.ToString(),
                                  Text = t.ToString().Replace("_", " ")
                              })
                              .ToList();

            ViewBag.TournamentTypes = tournamentTypes;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTournament(UpdateTournamentViewModel viewModel, IFormFile TournamentImages, IFormFile TrophyImages)
        {
            var user = await _userManager.GetUserAsync(User);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == viewModel.TournamentId)
                .FirstOrDefaultAsync();

            tournament.TournamentName = viewModel.TournamentName;
            tournament.TournamentDescription = viewModel.TournamentDescription;
            tournament.TournamentLocation = viewModel.TournamentLocation;
            tournament.SponsorName = viewModel.SponsorName;
            tournament.Sponsorship = viewModel.Sponsorship;
            tournament.SponsorContactDetails = viewModel.SponsorContactDetails;
            tournament.ModifiedById = user.Id;
            tournament.ModifiedDateTime = DateTime.Now;
            tournament.JoiningFee = viewModel.JoiningFee;
            tournament.JoiningDueDate = viewModel.JoiningDueDate;
            tournament.StartDate = viewModel.StartDate;
            tournament.NumberOfTeams = viewModel.NumberOfTeams;
            tournament.TournamentType = viewModel.TournamentType;


            if (TournamentImages != null && TournamentImages.Length > 0)
            {
                var uploadedImagePath = await _fileUploadService.UploadFileAsync(TournamentImages);
                tournament.TournamentImage = uploadedImagePath;
            }

            if (TrophyImages != null && TrophyImages.Length > 0)
            {
                var uploadedImagePath = await _fileUploadService.UploadFileAsync(TrophyImages);
                tournament.TrophyImage = uploadedImagePath;
            }

            _context.Update(tournament);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Updated {tournament.TournamentName} information", user.Id);


            TempData["Message"] = $"{tournament.TournamentName} information has been updated successfully.";

            var tournamentTypes = Enum.GetValues(typeof(TournamentType))
                              .Cast<TournamentType>()
                              .Select(t => new SelectListItem
                              {
                                  Value = t.ToString(),
                                  Text = t.ToString().Replace("_", " ")
                              })
                              .ToList();

            ViewBag.TournamentTypes = tournamentTypes;

            return RedirectToAction(nameof(TournamentsBackOffice));

        }


        [HttpGet]
        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> UpdateClub(string clubId)
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

            var club = await _context.TournamentClubs.
               Where(c => c.ClubId == decryptedClubId)
               .Include(c => c.Tournament)
               .FirstOrDefaultAsync();

            var clubViewModel = new UpdateTournamentClubViewModel
            {
                ClubId = decryptedClubId,
                ClubName = club.ClubName,
                Email = club.Email,
                ClubLocation = club.ClubLocation,
                ClubAbbr = club.ClubAbbr,
                ClubBadge = club.ClubBadge,
                ClubManagerEmail = club.ClubManagerEmail,
                ClubManagerName = club.ClubManagerName,
                ManagerProfilePicture  = club.ManagerProfilePicture,
                ClubManagerPhone = club.ClubManagerPhone,
                TournamentId = club.TournamentId,
                DivisionId = club.DivisionId,
                ClubHistory = club.ClubHistory,
                ClubSummary = club.ClubSummary
            };

            TempData["LogMessages"] = logMessages;
            return View(clubViewModel);
        }


        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClub(UpdateTournamentClubViewModel viewModel, IFormFile ClubBadges,IFormFile ManagerProfilePictures)
        {
            var logMessages = new List<string>();

            var user = await _userManager.GetUserAsync(User);

            if (viewModel.ClubId == null)
            {
                return NotFound();
            }

            if (ValidateUpdatedProperties(viewModel))
            {
                var club = await _context.TournamentClubs
                    .Where(c => c.ClubId == viewModel.ClubId)
                    .Include(c => c.Tournament)
                    .FirstOrDefaultAsync();

                var tournamentId = club.Tournament.TournamentId;

                try
                {

                    if (club == null)
                    {
                        return NotFound();
                    }

                    club.ClubAbbr = viewModel.ClubAbbr;
                    club.ClubName = viewModel.ClubName;
                    club.ClubManagerName = viewModel.ClubManagerName;
                    club.ClubManagerPhone = viewModel.ClubManagerPhone;
                    club.ClubManagerEmail = viewModel.ClubManagerEmail;
                    club.Email = viewModel.Email;
                    club.ClubLocation = viewModel.ClubLocation;
                    club.ClubHistory = viewModel.ClubHistory;
                    club.ClubSummary = viewModel.ClubSummary;
                    club.ModifiedById = user.Id;
                    club.ModifiedDateTime = DateTime.Now;

                    if (ClubBadges != null && ClubBadges.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ClubBadges);
                        club.ClubBadge = uploadedImagePath;
                    }
                    else
                    {
                        club.ClubBadge = viewModel.ClubBadge;
                    }

                    if (ManagerProfilePictures != null && ManagerProfilePictures.Length > 0)
                    {
                        var uploadedImagePath = await _fileUploadService.UploadFileAsync(ManagerProfilePictures);
                        club.ManagerProfilePicture = uploadedImagePath;
                    }
                    else
                    {
                        club.ManagerProfilePicture = viewModel.ManagerProfilePicture;
                    }

                    _context.Update(club);
                    await _context.SaveChangesAsync();

                    await _activityLogger.Log($"Updated {club.ClubName} information", user.Id);
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to update club: " + ex.Message,
                        errorDetails = new
                        {
                            InnerException = ex.InnerException?.Message,
                            StackTrace = ex.StackTrace
                        }
                    });
                }

                await _activityLogger.Log($"Updated {club.ClubName}information", user.Id);
                await _requestLogService.LogSuceededRequest("Successfully updated a club", StatusCodes.Status200OK);

                var encryptedTournamentId = _encryptionService.Encrypt(tournamentId);

                TempData["Message"] = $"{club.ClubName} information has been updated successfully.";
                return RedirectToAction(nameof(TournamentParticipants), new { tournamentId = encryptedTournamentId });

            }

            await _requestLogService.LogFailedRequest("Failed to update a club", StatusCodes.Status500InternalServerError);

            var existingClub = await _context.TournamentClubs.FindAsync(viewModel.ClubId);

            viewModel.ClubBadge = existingClub.ClubBadge;

            return View(viewModel);
        }

        private bool ValidateUpdatedProperties(UpdateTournamentClubViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.ClubName, new ValidationContext(viewModel, null, null) { MemberName = "ClubName" }, validationResults);
            Validator.TryValidateProperty(viewModel.Email, new ValidationContext(viewModel, null, null) { MemberName = "Email" }, validationResults);
            Validator.TryValidateProperty(viewModel.ClubLocation, new ValidationContext(viewModel, null, null) { MemberName = "ClubLocation" }, validationResults);
            Validator.TryValidateProperty(viewModel.ClubSummary, new ValidationContext(viewModel, null, null) { MemberName = "ClubSummary" }, validationResults);
            return validationResults.Count == 0;
        }

        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTournamentRule(string ruleId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedRuleId = _encryptionService.DecryptToInt(ruleId);

            var rule = await _context.TournamentRules
                .Where(r => r.RuleId == decryptedRuleId)
                .Include(r => r.Tournament)
                .FirstOrDefaultAsync();

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == rule.TournamentId)
                .FirstOrDefaultAsync();

            await _activityLogger.Log($"Deleted {rule.RuleDescription} rule for {tournament.TournamentName}", user.Id);

            _context.Remove(rule);
            await _context.SaveChangesAsync();

            var tournamentId = _encryptionService.Encrypt(tournament.TournamentId);

            TempData["Message"] = $"{tournament.TournamentName} rule has been deleted.";
            return RedirectToAction(nameof(TournamentRules), new { tournamentId });
        }


        [HttpGet]
        [Authorize(Roles = "Sport Administrator")]
        public async Task<IActionResult> UpdateTournamentRule(string ruleId)
        {
            var decryptedRuleId = _encryptionService.DecryptToInt(ruleId);

            var rule = await _context.TournamentRules
                .Where(r => r.RuleId == decryptedRuleId)
                .Include(r => r.Tournament)
                .FirstOrDefaultAsync();

            var viewModel = new UpdateTournamentRuleViewModel
            {
                RuleId = rule.RuleId,
                RuleDescription = rule.RuleDescription,
                TournamentId = rule.Tournament.TournamentId
            };

            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTournamentRule(UpdateTournamentRuleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var rule = await _context.TournamentRules
                    .Where(r => r.RuleId == viewModel.RuleId)
                    .FirstOrDefaultAsync();

                var tournament = await _context.Tournament
                    .Where(t => t.TournamentId == viewModel.TournamentId)
                    .FirstOrDefaultAsync();

                rule.RuleDescription = viewModel.RuleDescription;

                _context.Update(rule);
                await _context.SaveChangesAsync();

                var tournamentId = _encryptionService.Encrypt(tournament.TournamentId);

                await _activityLogger.Log($"Updated {tournament.TournamentName} rule.", user.Id);

                TempData["Message"] = $"{tournament.TournamentName} rule has been updated successfully.";
                return RedirectToAction(nameof(TournamentRules), new { tournamentId });
            }

            return View(viewModel);
        }



        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTournament(string tournamentId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            var tournamentClubs = await _context.TournamentClubs
                .Where(tc => tc.TournamentId == tournament.TournamentId)
                .ToListAsync();

            await _activityLogger.Log($"Cancelled/Deleted {tournament.TournamentName}.", user.Id);

            TempData["Message"] = $"{tournament.TournamentName} has been cancelled/deleted.";

            _context.RemoveRange(tournamentClubs);
            _context.Remove(tournament);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TournamentsBackOffice));
        }


        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTournament(string tournamentId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            tournament.IsPublished = true;

            _context.Update(tournament);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Approved {tournament.TournamentName}.", user.Id);

            TempData["Message"] = $"{tournament.TournamentName} has been approved successfully.";

            return RedirectToAction(nameof(TournamentsBackOffice));
        }
    }
}
