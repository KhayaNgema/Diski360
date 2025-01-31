using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;

namespace MyField.Controllers
{
    public class UserManualsController : Controller
    {

        private readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly RequestLogService _requestLogService;
        private readonly IEncryptionService _encryptionService;

        public UserManualsController(Ksans_SportsDbContext context,
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manuals()
        {
            var userManuals = await _context.UserManuals
                .ToListAsync();

            return View(userManuals);
        }

        [Authorize(Roles = "System Administrator")]
        [HttpGet]
        public async Task<IActionResult> UploadManual()
        {
            return View();
        }

        [Authorize(Roles = "System Administrator")]
        [HttpPost]
        public async Task<IActionResult> UploadManual(UserManualsViewModel viewModel, IFormFile ManualDocuments)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var newUserManual = new UserManuals
                {
                  UserRole = viewModel.UserRole,
                };

                if (ManualDocuments != null && ManualDocuments.Length > 0)
                {
                    var uploadedPdfPath = await _fileUploadService.UploadFileAsync(ManualDocuments);
                    newUserManual.ManualDocument = uploadedPdfPath;
                }

                _context.Add(newUserManual);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Uploaded {newUserManual.UserRole} manual document", user.Id);

                await _requestLogService.LogSuceededRequest("Successfully uploaded a new user manual", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully uploaded {newUserManual.UserRole} manual.";

                return RedirectToAction(nameof(Manuals));

            }

            return View(viewModel);
        }

        [Authorize(Roles = "System Administrator")]
        [HttpGet]
        public async Task<IActionResult> UpdateManual(string manualId)
        {
            var decryptedManualId = _encryptionService.DecryptToInt(manualId);

            var userManual = await _context.UserManuals
                .Where(um => um.ManualId == decryptedManualId)
                .FirstOrDefaultAsync();

            var viewModel = new UpdateUserManualsViewModel
            {
                ManualId = decryptedManualId,
                ManualDocument = userManual.ManualDocument,
                UserRole = userManual.UserRole
            };

            return View(viewModel);
        }

        [Authorize(Roles = "System Administrator")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateManual(UpdateUserManualsViewModel viewModel, IFormFile ManualDocuments)
        {
            var user = await _userManager.GetUserAsync(User);

            var userManual = await _context.UserManuals
                .Where(um => um.ManualId == viewModel.ManualId)
                .FirstOrDefaultAsync();

            if (userManual == null)
            {
                TempData["Error"] = "User manual not found.";
                return RedirectToAction(nameof(Manuals));
            }

            if (!string.IsNullOrEmpty(viewModel.UserRole))
            {
                userManual.UserRole = viewModel.UserRole;
            }

            if (ManualDocuments != null && ManualDocuments.Length > 0)
            {
                var uploadedPdfPath = await _fileUploadService.UploadFileAsync(ManualDocuments);
                if (!string.IsNullOrEmpty(uploadedPdfPath))
                {
                    userManual.ManualDocument = uploadedPdfPath;
                }
            }


            _context.Update(userManual);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Updated {userManual.UserRole} manual document", user.Id);

            TempData["Message"] = $"You have successfully updated {userManual.UserRole} manual.";

            return RedirectToAction(nameof(Manuals));
        }


        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> DeleteManual(string manualId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedManualId = _encryptionService.DecryptToInt(manualId);

            var userManual = await _context.UserManuals
                .Where(um => um.ManualId == decryptedManualId)
                .FirstOrDefaultAsync();

           
            _context.Remove(userManual);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Deleted {userManual.UserRole} manual document", user.Id);

            TempData["Message"] = $"You have deleted {userManual.UserRole} manual.";

            return RedirectToAction(nameof(Manuals));
        }
    }
}
