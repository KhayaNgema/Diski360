using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
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
    public class SportNewsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly FileUploadService _fileUploadService;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public SportNewsController(Ksans_SportsDbContext context, 
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

        public async Task<IActionResult> SportNewsIndex()
        {
            return PartialView("NewsPartial");
        }


        public async Task<IActionResult> SportNewsIndexDivision()
        {
            return PartialView("NewsPartialDivision");
        }


        [Authorize(Roles =("News Administrator"))]
        public async Task<IActionResult> NewsReview(string newsId)
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);

            if (newsId == null || _context.SportNew == null)
            {
                return NotFound();
            }

            var overallNewsReports = await _context.OverallNewsReports
                .FirstOrDefaultAsync();

            var individualNewsReports = await _context.IndividualNewsReports
                .Where(i => i.SportNewsId == decryptedNewsId)
                .Include(i => i.SportNews)
                .FirstOrDefaultAsync();

            overallNewsReports.NewsReadersCount++;
            individualNewsReports.ReadersCount++;

            await _context.SaveChangesAsync();

            var sportNews = await _context.SportNew
                .Where(sn => sn.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .Include(s => s.RejectedBy)
                .FirstOrDefaultAsync(m => m.NewsId == decryptedNewsId);


            if (sportNews == null)
            {
                return NotFound();
            }

            return View(sportNews);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> ToBeModifiedSportNews()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var newsToBeModified = await _context.SportNew
                .Where(n => n.NewsStatus == NewsStatus.ToBeModified &&
                n.DivisionId == divisionId)
                .Include( n=> n.AuthoredBy)
                .Include(n => n.ModifiedBy)
                .OrderByDescending(n => n.ModifiedDateTime)
                .ToListAsync();

            return View(newsToBeModified);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task <IActionResult> PublishedSportNewsBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var sportsNews = await _context.SportNew
                  .Where(s => s.NewsStatus == NewsStatus.Approved && 
                  s.DivisionId == divisionId)
                  .Include(s => s.AuthoredBy)
                  .Include(s => s.ModifiedBy)
                  .Include(s => s.PublishedBy)
                  .OrderByDescending(n => n.PublishedDate)
                  .ToListAsync();

            return View(sportsNews);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> PublishedSportNews()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var userId = user.Id;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Approved && 
                 s.AuthoredById == userId &&
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();

            return PartialView("_PublishedSportNewsPartial",sportsNews);
        }

        [Authorize(Roles = ("News Updator"))]
        public async Task<IActionResult> SportNewsList()
        {
            return View();
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> ApprovedSportNewsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var userId = user.Id;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Approved && 
                 s.AuthoredById == userId && 
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.ModifiedDateTime)
                .ToListAsync();

            return PartialView("_ApprovedSportNewsPartial", sportsNews);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> ApprovedSportNews()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Approved && 
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                 .OrderByDescending(n => n.ModifiedDateTime)
                .ToListAsync();

            return View(sportsNews);
        }


        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> AwaitingApprovalSportNewsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var userId = user.Id;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Awaiting_Approval && 
                 s.AuthoredById == userId && s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.ModifiedDateTime)
                .ToListAsync();

            return PartialView("_PendingSportNewsPartial",sportsNews);
        }

        [Authorize(Roles = ("News Administrator"))]
        public async Task<IActionResult> AwaitingApprovalSportNews()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Awaiting_Approval && 
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.ModifiedDateTime)
                .ToListAsync();

            return View(sportsNews);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> RejectedSportNewsAdmin()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var userId = user.Id;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Rejected && 
                 s.AuthoredById == userId && 
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.RejectedDateTime)
                .ToListAsync();

            return PartialView("_RejectedSportNewsPartial",sportsNews);
        }

        [Authorize(Roles = ("News Administrator"))]
        public async Task<IActionResult> RejectedSportNews()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Rejected && 
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                 .OrderByDescending(n => n.RejectedDateTime)
                .ToListAsync();

            return View(sportsNews);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> SportNewsBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var sportsNews = await _context.SportNew
                 .Where(s => s.NewsStatus == NewsStatus.Approved &&
                 s.DivisionId == divisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                 .OrderByDescending(n => n.ModifiedDateTime)
                .ToListAsync();

            return View(sportsNews);
        }

        public async Task<IActionResult> NewsMain()
        {
            var sportsNews = await _context.SportNew
                .Where(s => s.NewsStatus == NewsStatus.Approved)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();

            return PartialView("_SportsNewsPartial", sportsNews);
        }

        public async Task<IActionResult> NewsMainDivision(string divisionId)
        {
            var decryptedDvisionId = _encryptionService.DecryptToInt(divisionId);

            var sportsNews = await _context.SportNew
                .Where(s => s.NewsStatus == NewsStatus.Approved &&
                s.DivisionId == decryptedDvisionId)
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();

            return PartialView("_SportsNewsPartialDivision", sportsNews);
        }

        [Authorize]
        public async Task<IActionResult> SportNews()
        {

            var sportsNews = await _context.SportNew
                .Where(s => s.NewsStatus == NewsStatus.Approved)
                .OrderByDescending(s => s.PublishedDate)
                .ToListAsync();

            return View( sportsNews);
        }

        [Authorize]
        public async Task<IActionResult> Details(string newsId)
        {

            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);

            if (newsId == null || _context.SportNew == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var overallNewsReports = await _context.OverallNewsReports
                .FirstOrDefaultAsync();

            var individualNewsReports = await _context.IndividualNewsReports
                .Where(i => i.SportNewsId == decryptedNewsId)
                .Include(i => i.SportNews)
                .FirstOrDefaultAsync();

            if(!User.IsInRole("News Administrator") || (!User.IsInRole("News Updator")))
            {
                overallNewsReports.NewsReadersCount++;
                individualNewsReports.ReadersCount++;

                var userSubscription = await _context.Subscriptions
                    .Where(us => us.UserId == user.Id)
                    .FirstOrDefaultAsync();

                if (userSubscription.SubscriptionPlan == SubscriptionPlan.Premium)
                {

                    var currentCompetition = await _context.Competition
                        .Where(cc => cc.CompetitionStatus == CompetitionStatus.Current)
                        .FirstOrDefaultAsync();

                    var userLeaderboard = await _context.CompetitionParticipants
                        .Where(ub => ub.UserId == user.Id && ub.CompetitionId == currentCompetition.CompetitionId)
                        .FirstOrDefaultAsync();

                    userLeaderboard.Points++;

                    _context.Update(userLeaderboard);
                    await _context.SaveChangesAsync();
                }
            }


            await _context.SaveChangesAsync();

            var sportNews = await _context.SportNew
                .Include(s => s.AuthoredBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.PublishedBy)
                .Include(s => s.RejectedBy)
                .FirstOrDefaultAsync(m => m.NewsId == decryptedNewsId);
            if (sportNews == null)
            {
                return NotFound();
            }

            return PartialView("_SportNewsDetailsPartial", sportNews);
        }


        [Authorize(Roles = ("News Updator"))]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "News Updator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SportNewsViewModel viewModel, IFormFile NewsImages)
        {
            try
            {
                if (!ModelState.IsValid) return View(viewModel);

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found. Please ensure you are logged in." });
                }

                var divisionId = (user as ClubAdministrator)?.DivisionId ??
                                 (user as ClubManager)?.DivisionId ??
                                 (user as Player)?.DivisionId ??
                                 (user as SportsMember)?.DivisionId ??
                                 (user as Officials)?.DivisionId ??
                                 (user as DivisionManager)?.DivisionId;

                var overallNewsReports = await _context.OverallNewsReports
                    .Where(on => on.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (overallNewsReports == null)
                {
                    return Json(new { success = false, message = "Overall news report not found for the division." });
                }

                var userId = user.Id;
                var sportNews = new SportNews
                {
                    NewsHeading = viewModel.NewsHeading,
                    NewsBody = viewModel.NewsBody,
                    AuthoredById = userId,
                    ModifiedById = userId,
                    PublishedById = userId,
                    ModifiedDateTime = DateTime.UtcNow,
                    RejectedById = userId,
                    RejectedDateTime = DateTime.UtcNow,
                    DivisionId = divisionId
                };

                if (NewsImages != null && NewsImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(NewsImages);
                    sportNews.NewsImage = uploadedImagePath;
                }

                overallNewsReports.AuthoredNewsCount++;

                sportNews.NewsStatus = NewsStatus.Awaiting_Approval;

                _context.SportNew.Add(sportNews);
                await _context.SaveChangesAsync();

                var savedNews = await _context.SportNew
                    .Where(s => s.Equals(sportNews))
                    .FirstOrDefaultAsync();

                var existingNewsReport = await _context.OverallNewsReports
                    .Where(enr => enr.DivisionId == divisionId)
                    .FirstOrDefaultAsync();

                if (existingNewsReport != null) 
                {
                    existingNewsReport.AuthoredNewsCount++;

                    _context.Update(existingNewsReport);
                    await _context.SaveChangesAsync();
                };

                var newIndividualNews = new IndividualNewsReport
                {
                    SportNewsId = sportNews.NewsId,
                    ReadersCount = 0,
                    DivisionId = divisionId
                };

                _context.IndividualNewsReports.Add(newIndividualNews);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"You have successfully authored the sport news with heading {savedNews?.NewsHeading}.";
                await _activityLogger.Log($"Authored sport news with heading {savedNews?.NewsHeading}", user.Id);
                await _requestLogService.LogSuceededRequest("Sport news authored successfully.", StatusCodes.Status200OK);

                var newsAdminRoleName = "News Administrator";
                var subject = "Pending News Approval";
                var emailBodyTemplate = $@"
            Dear {{0}},<br/><br/>
            A new sport news item with the heading <b>{savedNews?.NewsHeading}</b> is awaiting your approval.<br/><br/>
            Please review and take the necessary action as soon as possible.<br/><br/>
            If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
            Regards,<br/>
            Diski360 Support Team
        ";

                var newsAdminUsers = await _userManager.GetUsersInRoleAsync(newsAdminRoleName);

                foreach (var adminUser in newsAdminUsers)
                {
                    var personalizedEmailBody = string.Format(emailBodyTemplate, $"{adminUser.FirstName} {adminUser.LastName}");
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(adminUser.Email, subject, personalizedEmailBody, "Diski 360"));
                }

                return RedirectToAction(nameof(SportNewsList));
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to create news: " + ex.Message,
                    errorDetails = new
                    {
                        innerException = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        }


        [Authorize(Roles = "News Updator, News Administrator")]
        [HttpGet]
        public async Task<IActionResult> ReEditNews(string newsId)
        {
            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);

            if (newsId == null)
            {
                return NotFound();
            }

            var existingNews = await _context.SportNew
                .Where(e => e.NewsId == decryptedNewsId)
                .FirstOrDefaultAsync();

            if (existingNews == null)
            {
                return NotFound();
            }

            var viewModel = new ReEditNewsViewModel
            {
                NewsId = decryptedNewsId,
                NewsImage = existingNews.NewsImage,
                NewsTitle = existingNews.NewsHeading,
                NewsBody = existingNews.NewsBody,
                ReasonForReEdit = existingNews.ReasonForReEdit
            };

            return View(viewModel);
        }

        [Authorize(Roles = "News Updator, News Administrator")]
        [HttpPost]
        public async Task<IActionResult> ReEditNews(ReEditNewsViewModel viewModel, IFormFile NewsImages)
        {
            if (viewModel.NewsId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var existingNews = await _context.SportNew
                .Where(e => e.NewsId == viewModel.NewsId)
                .FirstOrDefaultAsync();

            if (existingNews == null)
            {
                return NotFound();
            }

            var oldNewsStatus = existingNews.NewsStatus;

            if (ValidateUpdatedProperties(viewModel))
            {
                existingNews.NewsHeading = viewModel.NewsTitle;
                existingNews.NewsBody = viewModel.NewsBody;
                existingNews.NewsStatus = NewsStatus.Awaiting_Approval;

                if (NewsImages != null && NewsImages.Length > 0)
                {
                    var uploadedImagePath = await _fileUploadService.UploadFileAsync(NewsImages);
                    existingNews.NewsImage = uploadedImagePath;
                }
                else
                {
                    existingNews.NewsImage = viewModel.NewsImage;
                }

                _context.Update(existingNews);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Re-edited sport news with heading {existingNews.NewsHeading}", user.Id);
                await _requestLogService.LogSuceededRequest("Sport news re-edited successfully.", StatusCodes.Status200OK);

                var newsAdminRoleName = "News Administrator";
                var subject = "News Re-Edited and Awaiting Approval";
                var emailBodyTemplate = $@"
            Dear {{0}},<br/><br/>
            The news item with the heading <b>{existingNews.NewsHeading}</b> has been re-edited and is now awaiting your review and approval.<br/><br/>
            Please review the updated news and take the necessary action.<br/><br/>
            If you have any questions, please contact us at support@ksfoundation.com.<br/><br/>
            Regards,<br/>
            Diski360 Support Team
                ";

                var newsAdminUsers = await _userManager.GetUsersInRoleAsync(newsAdminRoleName);

                foreach (var adminUser in newsAdminUsers)
                {
                    var personalizedEmailBody = string.Format(emailBodyTemplate, $"{adminUser.FirstName} {adminUser.LastName}");
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(adminUser.Email, subject, personalizedEmailBody, "Diski 360"));
                }

                if(User.IsInRole("News Administrator"))
                {
                    if (oldNewsStatus == NewsStatus.Awaiting_Approval)
                    {
                        TempData["Message"] = $"You have successfully updated the sport news with heading {existingNews.NewsHeading}.";

                        var encryptedNewsId = _encryptionService.Encrypt(existingNews.NewsId);

                        return RedirectToAction(nameof(NewsReview), new { newsId = encryptedNewsId });
                    }
                }
                else
                {
                    if (oldNewsStatus == NewsStatus.Awaiting_Approval)
                    {
                        TempData["Message"] = $"You have successfully updated the sport news with heading {existingNews.NewsHeading}.";
                        return RedirectToAction(nameof(SportNewsList));
                    }
                    else if (oldNewsStatus == NewsStatus.ToBeModified)
                    {
                        TempData["Message"] = $"You have successfully updated the sport news with heading {existingNews.NewsHeading}.";
                        return RedirectToAction(nameof(ToBeModifiedSportNews));
                    }
                    else
                    {
                        TempData["Message"] = $"You have successfully updated the sport news with heading {existingNews.NewsHeading}.";
                        return RedirectToAction(nameof(SportNewsList));
                    }
                }

            }

            return View(viewModel);
        }


        private bool ValidateUpdatedProperties(ReEditNewsViewModel viewModel)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(viewModel.NewsTitle, new ValidationContext(viewModel, null, null) { MemberName = "NewsTitle" }, validationResults);
            Validator.TryValidateProperty(viewModel.NewsBody, new ValidationContext(viewModel, null, null) { MemberName = "NewsBody" }, validationResults);
            return validationResults.Count == 0;
        }


        private bool SportNewsExists(int id)
        {

            return (_context.SportNew?.Any(e => e.NewsId == id)).GetValueOrDefault();
        }

        [Authorize(Roles = ("News Administrator"))]
        public async Task<IActionResult> ApproveNews(string newsId)
        {

            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);

            if (decryptedNewsId == null || _context.SportNew == null)
            {
                return NotFound();
            }

            var overallNewsReports = await _context.OverallNewsReports
               .FirstOrDefaultAsync();

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var sportNews = await _context.SportNew.FindAsync(decryptedNewsId);
            if (sportNews == null)
            {
                return NotFound();
            }

            overallNewsReports.ApprovedNewsCount++;
            overallNewsReports.PublishedNewsCount++;

            sportNews.NewsStatus = NewsStatus.Approved;
            sportNews.PublishedDate = DateTime.Now;
            sportNews.PublishedById = userId;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"You have successfully approved the sport news with heading {sportNews.NewsHeading}.";

            await _activityLogger.Log($"Approved sportnews with heading {sportNews.NewsHeading}", user.Id);
            return RedirectToAction(nameof(AwaitingApprovalSportNews));
        }

        [Authorize(Roles = ("News Administrator"))]
        [HttpGet]
        public async Task<IActionResult> AskReEditNews(string newsId)
        {
            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);


            if (newsId == null)
            {
                return NotFound();
            }

            var sportNews = await _context.SportNew
                .Where(s => s.NewsId == decryptedNewsId)
                .Include( s => s.AuthoredBy)
                .FirstOrDefaultAsync();

            var viewModel = new AskForReEditNewsViewModel
            {
                NewsId = decryptedNewsId,
                NewsTitle = sportNews.NewsHeading,
                NewsImage = sportNews.NewsImage,
                NewsBody = sportNews.NewsBody,
            };

            return View(viewModel);
        }

        [Authorize(Roles = ("News Administrator"))]
        [HttpPost]
        public async Task<IActionResult> AskReEditNews(AskForReEditNewsViewModel viewModel)
        {

            if (viewModel.NewsId == null || _context.SportNew == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var sportNews = await _context.SportNew.FindAsync(viewModel.NewsId);
            if (sportNews == null)
            {
                return NotFound();
            }

            var author = await _userManager.FindByIdAsync(sportNews.AuthoredById);
            if (author == null)
            {
                return NotFound();
            }

            sportNews.NewsStatus = NewsStatus.ToBeModified;
            sportNews.ModifiedDateTime = DateTime.Now;
            sportNews.ModifiedById = userId;
            sportNews.ReasonForReEdit = viewModel.ReasonForReEdit;

            _context.Update(sportNews);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Sent news with heading {sportNews.NewsHeading} back to author for modification", user.Id); await _requestLogService.LogSuceededRequest("Own goal recorded successfully.", StatusCodes.Status200OK);
            await _requestLogService.LogSuceededRequest("Re-edit sport news request sent successfully.", StatusCodes.Status200OK);


            var subject = "Request for News Re-Editing";
            var emailBodyTemplate = $@"
        Dear {author.FirstName} {author.LastName},<br/><br/>
        The news item with the heading <b>{sportNews.NewsHeading}</b> has been sent back for re-editing.<br/><br/>
        Please make the necessary modifications and resubmit the news.<br/><br/>
        If you have any questions or need further clarification, please contact us at support@ksfoundation.com.<br/><br/>
        Regards,<br/>
        Diski360 Support Team
            ";

            await _emailService.SendEmailAsync(author.Email, subject, emailBodyTemplate, "Diski 360");

            TempData["Message"] = $"You have successfully sent the news back to the author for re-editing: {sportNews.NewsHeading}.";
            return RedirectToAction(nameof(AwaitingApprovalSportNews));
        }

        [Authorize(Roles = ("News Administrator"))]
        public async Task<IActionResult> DeclineNews(string newsId)
        {

            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);

            if (decryptedNewsId == null || _context.SportNew == null)
            {
                return NotFound();
            }

            var overallNewsReports = await _context.OverallNewsReports
                 .FirstOrDefaultAsync();

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var sportNews = await _context.SportNew.FindAsync(decryptedNewsId);
            if (sportNews == null)
            {
                return NotFound();
            }

            overallNewsReports.RejectedNewsCount++;
            sportNews.NewsStatus = NewsStatus.Rejected;
            sportNews.RejectedDateTime = DateTime.Now;
            sportNews.RejectedById = userId;   

            await _context.SaveChangesAsync();
            TempData["Message"] = $"You have declined the sport news with heading {sportNews.NewsHeading}.";
            await _activityLogger.Log($"Declined sportnews with heading {sportNews.NewsHeading}", user.Id);
            await _requestLogService.LogSuceededRequest("Sport news declined successfully.", StatusCodes.Status200OK);

            return RedirectToAction(nameof(AwaitingApprovalSportNews));
        }

        [Authorize(Roles = ("News Updator"))]
        public async Task<IActionResult> DeleteSportNews(string newsId)
        {
            var decryptedNewsId = _encryptionService.DecryptToInt(newsId);

            if (newsId == null || _context.SportNew == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var sportNew = await _context.SportNew.FindAsync(decryptedNewsId);

            await _activityLogger.Log($"Deleted news with heading {sportNew.NewsHeading}", user.Id);

            TempData["Message"] = $"You have deleted sport news with heading {sportNew.NewsHeading}.";
            await _requestLogService.LogSuceededRequest("Sport news deleted successfully.", StatusCodes.Status200OK);

            _context.Remove(sportNew);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SportNewsList));
        }

    }
}
