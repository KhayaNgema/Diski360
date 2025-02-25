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


        public async Task<IActionResult> Details(string tournamentId)
        {
            if(tournamentId == null)
            {
                return NotFound();
            }

            var decryptedTournamentId = _encryptionService.DecryptToInt(tournamentId);

            var tournament = await _context.Tournament
                .Where(t => t.TournamentId == decryptedTournamentId)
                .FirstOrDefaultAsync(); 

            return View(tournament);
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

                return RedirectToAction(nameof(TournamentsBackOffice), new { tournamentId = newTournament.TournamentId });

            }

            await _requestLogService.LogFailedRequest("Failed to create a new tournament", StatusCodes.Status500InternalServerError);

            return View(viewModel);

        }

    }
}
