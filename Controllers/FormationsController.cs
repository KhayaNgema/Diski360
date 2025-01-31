using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;

namespace MyField.Controllers
{
    public class FormationsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly RequestLogService _requestLogService;
        private readonly IEncryptionService _encryptionService;

        public FormationsController(Ksans_SportsDbContext context,
               FileUploadService fileUploadService,
               UserManager<UserBaseModel> userManager,
               IActivityLogger activityLogger,
               RequestLogService requestLogService,
               IEncryptionService encryptionService)
        {
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _context = context;
            _activityLogger = activityLogger;
            _requestLogService = requestLogService;
            _encryptionService = encryptionService;
        }

        [Authorize(Roles = "System Administrator")]
        [HttpGet]
        public async Task<IActionResult> Formations()
        {
            var formations = await _context.Formations
                .ToListAsync();

            return View(formations);
        }

        [Authorize(Roles = "System Administrator")]
        [HttpGet]
        public async Task<IActionResult> CreateFormation()
        {
            return View();
        }


        [Authorize(Roles = "System Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFormation(FormationViewModel viewModel, IFormFile? FormationImages)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var newFormation = new Formation
                {
                    FormationName = viewModel.FormationName,
                    CreatedById = user.Id,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = user.Id,
                    ModifiedDateTime = DateTime.Now
                };

                if (FormationImages != null && FormationImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(FormationImages);
                    newFormation.FormationImage = uploadedImagePath;
                }

                _context.Add(newFormation);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Added {newFormation.FormationName} as a new formation", user.Id);

                await _requestLogService.LogSuceededRequest("Successfully created a new formation", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully added {viewModel.FormationName} formation.";

                return RedirectToAction(nameof(Formations));
            }

            return View(viewModel);
        }




        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> CreateMatchFormation()
        {
            var matchFormation = await _context.Formations
                    .Include(s => s.CreatedBy)
                    .Include(s => s.ModifiedBy)
                    .FirstOrDefaultAsync();

            if(matchFormation != null)
            {
                ViewBag.Formations = await _context.Formations.ToListAsync();
            }
            else
            {
                ViewBag.Formations = $"No match formations found";
            }

            return PartialView("_CreateMatchFormationPartial", matchFormation);
        }


        public async Task<IActionResult> MatchFormationFind(int fixtureId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null || !(user is ClubManager clubManager))
                {
                    return RedirectToAction("Error", "Home");
                }

                var matchFormations = await _context.MatchFormation
                    .Where(mo => mo.FixtureId == fixtureId &&
                    mo.ClubId == clubManager.ClubId)
                    .Include(s => s.Fixture)
                    .Include(s => s.Formation)
                    .ToListAsync();

                return PartialView("_ClubFormationPartial", matchFormations);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MatchFormation action: " + ex.Message);

                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> Index()
        {
              return _context.Formations != null ? 
                          View(await _context.Formations.ToListAsync()) :
                          Problem("Entity set 'Ksans_SportsDbContext.Formations'  is null.");
        }


        [Authorize(Roles = ("Club Manager"))]
        [HttpGet]
        public IActionResult CreateMatchFormationFinal(int fixtureId, int formationId)
        {
            var viewModel = new MatchFormationFinalViewModel
            {
                FixtureId = fixtureId,
                FormationId = formationId
            };

            return View(viewModel);
        }


        [Authorize(Roles = "Club Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateMatchFormationFinal(MatchFormationFinalViewModel viewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                if (user == null || !(user is ClubManager clubManager))
                {
                    return Json(new { success = false, error = "Unauthorized access." });
                }

                var fixture = await _context.Fixture.FindAsync(viewModel.FixtureId);

                if (fixture == null)
                {
                    return Json(new { success = false, error = "Fixture not found." });
                }

                if (!(fixture.HomeTeamId == clubManager.ClubId || fixture.AwayTeamId == clubManager.ClubId))
                {
                    return Json(new { success = false, error = "You can't set formation for the club you are not related to!" });
                }

                var existingFormation = await _context.MatchFormation
                    .Include(e => e.Formation)
                    .FirstOrDefaultAsync(x => x.FixtureId == viewModel.FixtureId && x.ClubId == clubManager.ClubId);

                if (existingFormation != null)
                {
                    return Json(new { success = false, error = "You can set formation once per match!" });
                }

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data submitted." });
                }

                var newMatchFormation = new MatchFormation
                {
                    FixtureId = viewModel.FixtureId,
                    ClubId = clubManager.ClubId,
                    FormationId = viewModel.FormationId,
                    CreatedById = userId,
                    CreatedDateTime = DateTime.UtcNow,
                    ModifiedById = userId,
                    ModifiedDateTime = DateTime.UtcNow,
                };

                _context.Add(newMatchFormation);
                await _context.SaveChangesAsync();

                var existingFixture = await _context.Fixture
                    .Where(f => f.FixtureId == viewModel.FixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                var newlySetMatchFormation = await _context.MatchFormation
                    .Where(nf => nf.FixtureId == existingFixture.FixtureId)
                    .FirstOrDefaultAsync();

                var setFormation = await _context.Formations
                    .Where(sf => sf.FormationId == newlySetMatchFormation.FormationId)
                    .FirstOrDefaultAsync();

                if (existingFixture != null)
                {
                    await _activityLogger.Log($"Set {setFormation.FormationName} formation between {existingFixture.HomeTeam.ClubName} and {existingFixture.AwayTeam.ClubName}", user.Id);
                }

                await _requestLogService.LogSuceededRequest("Formation set successfully.", StatusCodes.Status200OK);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating a new formation: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while processing the request." });
            }
        }


        [Authorize(Roles = "System Administrator")]
        [HttpGet]
        public async Task<IActionResult> UpdateFormation(string formationId)
        {
            var decryptedFormationId = _encryptionService.DecryptToInt(formationId);

            var formation = await _context.Formations
                .Where(f => f.FormationId == decryptedFormationId)
                .FirstOrDefaultAsync();

            var viewModel = new UpdateFormationViewModel
            {
                FormationId = decryptedFormationId,
                FormationImage = formation.FormationImage,
                FormationName = formation.FormationName
            };
                
            return View(viewModel);
        }


        [Authorize(Roles = "System Administrator")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateFormation(UpdateFormationViewModel viewModel, IFormFile FormationImages, string CurrentFormationImage)
        {
            var user = await _userManager.GetUserAsync(User);

            var formation = await _context.Formations
                .Where(f => f.FormationId == viewModel.FormationId)
                .FirstOrDefaultAsync();

            formation.FormationName = viewModel.FormationName;

            if (FormationImages != null && FormationImages.Length > 0)
            {
                var uploadedImagePath = await _fileUploadService.UploadFileAsync(FormationImages);
                if (!string.IsNullOrEmpty(uploadedImagePath))
                {
                    formation.FormationImage = uploadedImagePath;
                }
            }
            else
            {
                formation.FormationImage = CurrentFormationImage;
            }


            _context.Update(formation);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Updated {formation.FormationName} formation", user.Id);

            TempData["Message"] = $"You have successfully updated {formation.FormationName} formation.";

            return RedirectToAction(nameof(Formations));
        }


        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> DeleteFormation(string formationId)
        {
            var decryptedFormationId = _encryptionService.DecryptToInt(formationId);

            var user = await _userManager.GetUserAsync(User);

            var formation = await _context.Formations
                .Where(f => f.FormationId == decryptedFormationId)
                .FirstOrDefaultAsync();

            
            _context.Remove(formation);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Deleted {formation.FormationName} formation", user.Id);

            TempData["Message"] = $"You have deleted {formation.FormationName} formation.";

            return RedirectToAction(nameof(Formations));
        }



        private bool FormationExists(int id)
        {
          return (_context.Formations?.Any(e => e.FormationId == id)).GetValueOrDefault();
        }
    }
}
