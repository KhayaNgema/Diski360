using Hangfire;
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
    public class AnnouncementsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly RequestLogService _requestLogService;

        public AnnouncementsController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            EmailService emailService,
            RequestLogService requestLogService)
        {
            _context = context;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _requestLogService = requestLogService;
        }

        [Authorize]
        public async Task<IActionResult> Announcements()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId??
                 (user as DivisionManager)?.DivisionId;

            var announcements = await _context.Announcements
                .Where(a => a.DivisionId == divisionId)
                .Include(a => a.CreatedBy)
                .Include(a => a.ModifiedBy)
                .ToListAsync();

            return View(announcements);
        }


        [Authorize(Roles =("Sport Administrator"))]
        [HttpGet]
        public async Task<IActionResult> NewAnnouncement()
        {
            return View();
        }


        [Authorize(Roles = "Sport Administrator")]
        [HttpPost]
        public async Task<IActionResult> NewAnnouncement(AnnouncementViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var divisionId = (user as SportsMember)?.DivisionId;

                var newAnnouncement = new Announcement
                {
                    AnnouncementText = viewModel.AnnouncementText,
                    CreatedById = user.Id,
                    CreatedDateTime = DateTime.Now,
                    ModifiedById = user.Id,
                    ModifiedDateTime = DateTime.Now,
                    DivisionId = divisionId
                };

                _context.Announcements.Add(newAnnouncement);
                await _context.SaveChangesAsync();

                var users = _userManager.Users.ToList();

                string emailBodyTemplate = $@"
                Hi {{0}},<br/><br/>
                There is a new announcement: <br/><br/>
                {viewModel.AnnouncementText}<br/><br/>
                Kind regards,<br/>
                Diski360 Support Team
                ";

                foreach (var currentUser in users)
                {
                    var personalizedEmailBody = string.Format(emailBodyTemplate, $"{currentUser.FirstName} {currentUser.LastName}");
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(
                        currentUser.Email,
                        "New Announcement",
                        personalizedEmailBody, "Diski 360"));
                }

                TempData["Message"] = "You have successfully created a new announcement.";

                await _requestLogService.LogSuceededRequest("Successfully created an announcement", StatusCodes.Status200OK);

                await _activityLogger.Log("Created a new announcement", user.Id);

                return RedirectToAction(nameof(Announcements));
            }

            await _requestLogService.LogFailedRequest("Failed to create an announcement", StatusCodes.Status500InternalServerError);

            return View(viewModel); 
        }


        [Authorize(Roles = ("Sport Administrator"))]
        [HttpGet]
        public async Task<IActionResult> UpdateAnnouncement(int announcementId)
        {
            if (announcementId == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Where(a => a.AnnouncementId == announcementId)
                .FirstOrDefaultAsync();

            var viewModel = new UpdateAnnouncementViewModel
            {
                AnnouncementId = announcementId,
                AnnouncementText = announcement.AnnouncementText
            };

            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        [HttpPost]
        public async Task<IActionResult> UpdateAnnouncement(int announcementId, UpdateAnnouncementViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (announcementId == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);

                var announcement = await _context.Announcements
                    .Where(a => a.AnnouncementId == announcementId)
                    .FirstOrDefaultAsync();

                announcement.AnnouncementText = viewModel.AnnouncementText;
                announcement.ModifiedDateTime = DateTime.Now;
                announcement.ModifiedById = user.Id;

                _context.Update(announcement);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"You have successfully updated announcement.";

                await _activityLogger.Log($"Updated an existing announcement", user.Id);

                await _requestLogService.LogSuceededRequest("Successfully updated an announcement", StatusCodes.Status200OK);

                return RedirectToAction(nameof(Announcements));
            }

            await _requestLogService.LogFailedRequest("Failed to update an announcement", StatusCodes.Status500InternalServerError);

            TempData["Message"] = $"Failed to update announcement announcement.";

            return RedirectToAction(nameof(Announcements));

        }


        [Authorize(Roles = "Sport Administrator")]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId)
        {
            var user = await _userManager.GetUserAsync(User);

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);

            _context.Announcements.Remove(announcement);

            await _context.SaveChangesAsync();

            TempData["Message"] = "You have deleted the announcement.";

            await _activityLogger.Log($"Deleted an announcement.", user.Id);

            await _requestLogService.LogSuceededRequest("Successfully deleted an announcement.", StatusCodes.Status200OK);

            return RedirectToAction(nameof(Announcements));
        }
    }
}
