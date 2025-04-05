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
        public IActionResult GetTournamentRules(int tournamentId)
        {
            var rules = _context.TournamentRules
                                .Where(r => r.TournamentId == tournamentId)
                                .Select(r => new { r.RuleDescription })
                                .ToList();

            return Json(rules);
        }



        public async Task<IActionResult> AllTournaments()
        {
            var tournaments = await _context.Tournament
                .Include(t => t.CreatedBy)
                .Include(t => t.ModifiedBy)
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
            };

            ViewBag.TournamentName = tournament.TournamentName;

            return View(viewModel);
        }

        [Authorize(Roles = "Sport Administrator")]
        [HttpGet]
        public async Task<IActionResult> NewTournament()
        {
            return View();
        }

        [Authorize(Roles = "Sport Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewTournament(NewTournamentViewModel viewModel, IFormFile TournamentImages)
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
                    IsPublished = false
                };


                if (TournamentImages != null && TournamentImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(TournamentImages);
                    newTournament.TournamentImage = uploadedImagePath;
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

            return View(viewModel);

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

                TempData["Message"] = $"A new rule has been created successfully.";

                await _requestLogService.LogSuceededRequest("Successfully created a new tournament rule", StatusCodes.Status200OK);

                return Ok(new { success = true });
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
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Sport Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTournament(UpdateTournamentViewModel viewModel, IFormFile TournamentImages)
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


                if (TournamentImages != null && TournamentImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(TournamentImages);
                    tournament.TournamentImage = uploadedImagePath;
                }

                _context.Update(tournament);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Updated {tournament.TournamentName} information", user.Id);


                TempData["Message"] = $"{tournament.TournamentName} information has been updated successfully.";
                return RedirectToAction(nameof(TournamentsBackOffice));
        }

    }
}
