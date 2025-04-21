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



        public async Task<IActionResult> AllTournaments()
        {
            var tournaments = await _context.Tournament
                .Where(t => t.IsPublished)
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
                .OrderByDescending (t => t.StartDate)
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
        public async Task<IActionResult> Tournaments()
        {
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Fixtures(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournamentFixtures = await _context.TournamentFixtures
                .Where(tf => tf.TournamentId == decryptedTournamentId &&
                tf.FixtureStatus == FixtureStatus.Upcoming)
                .ToListAsync();

            return PartialView("_TournamentFixturesPartial", tournamentFixtures);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Knockout(string tournamentId)
        {
            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var knockout = await _context.TournamentMatchResults
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
            if(tournamentId == null)
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
                .Where(t => t.TournamentId ==  decryptedTournamentId)
                .FirstOrDefaultAsync();

            var club = await _context.Club
                .Where(c => c.ClubId == clubId)
                .Include(c => c.Division)
                .Include(c => c.ClubManager)
                .FirstOrDefaultAsync();

            if(club != null)
            {
                var viewModel = new JoinTournamentViewModel
                {
                    ClubName = club.ClubName,
                    ClubAbbr = club.ClubAbbr,
                    ClubBadge = club.ClubBadge,
                    ClubHistory = club.ClubHistory,
                    ClubSummary = club.ClubSummary,
                    DivisionId = club.DivisionId,
                    ClubLocation = club.ClubLocation,
                    TournamentId = decryptedTournamentId,
                    ClubManagerName = $"{club.ClubManager.FirstName} {club.ClubManager.LastName}",
                    ClubManagerEmail = club.ClubManager.Email,
                    ClubManagerPhone = club.ClubManager.PhoneNumber,
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
        public async Task<IActionResult> JoinTournament(JoinTournamentViewModel viewModel)
        {


            var tournamentId = _encryptionService.Encrypt(viewModel.TournamentId);

            return RedirectToAction(nameof(Details), new { tournamentId });
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
           if(ModelState.IsValid)
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

            var  decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync();

            await _activityLogger.Log($"Cancelled/Deleted {tournament.TournamentName}.", user.Id);

            TempData["Message"] = $"{tournament.TournamentName} has been cancelled/deleted.";

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
