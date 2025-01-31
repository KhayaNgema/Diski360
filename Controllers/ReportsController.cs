using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;

namespace MyField.Controllers
{
    public class ReportsController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEncryptionService _encryptionService;

        public ReportsController(Ksans_SportsDbContext context,
            FileUploadService fileUploadService,
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            RoleManager<IdentityRole> roleManager,
            IEncryptionService encryptionService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _roleManager = roleManager;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> SystemLogs()
        {
            var systemLogs = await _context.RequestLogs
                .OrderByDescending(ol => ol.TimeStamp)
                .ToListAsync();

            return View(systemLogs);
        }

        [Authorize]
        public async Task<IActionResult> FindTopScores(string divisionId)
        {
            var user = await _userManager.GetUserAsync(User);

            var userSubscription = await _context.Subscriptions
                .Where(us => us.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (userSubscription == null ||
                userSubscription.SubscriptionPlan != SubscriptionPlan.Premium ||
                (userSubscription.SubscriptionPlan == SubscriptionPlan.Premium &&
                 (userSubscription.SubscriptionStatus == SubscriptionStatus.Expired ||
                  userSubscription.SubscriptionStatus == SubscriptionStatus.Cancelled)))
            {
                return Json(new { success = false, redirectUrl = Url.Action("UserSubscribe", "Subscriptions") });
            }


            ViewBag.DivisionId = divisionId; 

            return PartialView("TopScoresPartial");
        }


        [Authorize]

        public async Task<IActionResult> FindTopAssists(string divisionId)
        {
            var user = await _userManager.GetUserAsync(User);

            var userSubscription = await _context.Subscriptions
                .Where(us => us.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (userSubscription == null ||
                userSubscription.SubscriptionPlan != SubscriptionPlan.Premium ||
                (userSubscription.SubscriptionPlan == SubscriptionPlan.Premium &&
                 (userSubscription.SubscriptionStatus == SubscriptionStatus.Expired ||
                  userSubscription.SubscriptionStatus == SubscriptionStatus.Cancelled)))
            {
                return Json(new { success = false, redirectUrl = Url.Action("UserSubscribe", "Subscriptions") });
            }

            ViewBag.DivisionId = divisionId;

            return PartialView("TopAssistsPartial");
        }

        [Authorize]
        public async Task<IActionResult> TopScores(string divisionId)
        {

            var decryptedDivisoinId = _encryptionService.DecryptToInt(divisionId);

            var topScores = await _context.TopScores
                .Where(t => t.League.IsCurrent && t.DivisionId == decryptedDivisoinId)
                .Include(t => t.Player)
                .ThenInclude(p => p.Club)
                .OrderByDescending(t => t.NumberOfGoals)
                .ToListAsync();

            return PartialView("_TopScoresPartial", topScores);
        }


        [Authorize]
        public async Task<IActionResult> TopAssists(string divisionId)
        {

            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var topAssists = await _context.TopAssists
                .Where(t => t.League.IsCurrent && t.DivisionId == decryptedDivisionId)
                .Include(t => t.Player)
                .ThenInclude(p => p.Club)
                .OrderByDescending(t => t.NumberOfAssists)
                .ToListAsync();

            return PartialView("_TopAssistsPartial", topAssists);
        }


        [Authorize]
        public async Task<IActionResult> TopScoresBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var topScores = await _context.TopScores
                .Where(t => t.League.IsCurrent &&
                t.DivisionId == divisionId)
                .Include(t => t.Player)
                .ThenInclude(p => p.Club)  
                .OrderByDescending(t => t.NumberOfGoals)
                .ToListAsync();

            return View(topScores);
        }


        [Authorize]
        public async Task<IActionResult> TopAssistsBackOffice()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var topAssists = await _context.TopAssists
                .Where(t => t.League.IsCurrent &&
                t.DivisionId == divisionId)
                .Include(t => t.Player)
                .ThenInclude(p => p.Club) 
                .OrderByDescending(t => t.NumberOfAssists)
                .ToListAsync();

            return View(topAssists);
        }



        [Authorize(Roles = ("Fans Administrator"))]
        public async Task<IActionResult> FansAccountsReports()
        {
            var fansAccountsReports = await _context.FansAccountsReports
                .ToListAsync();

            var overallFansAccountsCount = await GetOverallFansAccountsCountAsync();

            var activeFansAccountsCount = await GetActiveFansAccountsCountAsync();

            var inactiveFansAccountsCount = await GetInactiveFansAccountsCountAsync();

            var suspendedFansAccountsCount = await GetSuspendedFansAccountsCountAsync();

            foreach(var fansAccountReport in fansAccountsReports)
            {
                fansAccountReport.OverallFansAccountsCount = overallFansAccountsCount;

                fansAccountReport.ActiveFansAccountsCount = activeFansAccountsCount;

                fansAccountReport.InactiveFansAccountsCount = inactiveFansAccountsCount;

                fansAccountReport.SuspendedFansAccountsCount = suspendedFansAccountsCount;

                if(overallFansAccountsCount > 0)
                {
                    if(activeFansAccountsCount > 0)
                    {
                        fansAccountReport.ActiveFansAccountsRate = ((decimal)activeFansAccountsCount / overallFansAccountsCount) * 100;
                    }

                    if (inactiveFansAccountsCount > 0)
                    {
                        fansAccountReport.InactiveFansAccountsRate = ((decimal)inactiveFansAccountsCount / overallFansAccountsCount) * 100;
                    }

                    if (suspendedFansAccountsCount > 0)
                    {
                        fansAccountReport.SuspendedFansAccountsRate = ((decimal)suspendedFansAccountsCount / overallFansAccountsCount) * 100;
                    }

                    decimal totalFansAccountsRate = fansAccountReport.ActiveFansAccountsRate +
                              fansAccountReport.InactiveFansAccountsRate +
                              fansAccountReport.SuspendedFansAccountsRate;
                    if (totalFansAccountsRate > 0)
                    {
                        decimal adjustmentFactor = 100 / totalFansAccountsRate;
                        fansAccountReport.ActiveFansAccountsRate *= adjustmentFactor;
                        fansAccountReport.InactiveFansAccountsRate *= adjustmentFactor;
                        fansAccountReport.SuspendedFansAccountsRate *= adjustmentFactor;
                    }
                }
            }


            return View(fansAccountsReports);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> PersonnelFinancialReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var personnelFinancialReports = await _context.PersonnelFinancialReports
                .Where(pf => pf.DivisionId == divisionId)
                .ToListAsync();

            foreach (var personnelFinancialReport in personnelFinancialReports)
            {
                personnelFinancialReport.PendingPaymentFinesCount = personnelFinancialReport.RepayableFinesCount - personnelFinancialReport.PaidPaymentFinesCount;

                personnelFinancialReport.TotalUnpaidAmount = personnelFinancialReport.ExpectedRepayableAmount - personnelFinancialReport.TotalPaidAmount;

                if (personnelFinancialReport.RepayableFinesCount > 0)
                {
                    personnelFinancialReport.PaidFinesRate = ((decimal)personnelFinancialReport.PaidPaymentFinesCount / personnelFinancialReport.RepayableFinesCount) * 100;
                    personnelFinancialReport.PendingFinesRate = ((decimal)personnelFinancialReport.PendingPaymentFinesCount / personnelFinancialReport.RepayableFinesCount) * 100;
                    personnelFinancialReport.OverdueFinesRate = ((decimal)personnelFinancialReport.OverduePaymentFineCount / personnelFinancialReport.RepayableFinesCount) * 100;
                }
                else
                {
                    personnelFinancialReport.PaidFinesRate = 0;
                    personnelFinancialReport.PendingFinesRate = 0;
                    personnelFinancialReport.OverdueFinesRate = 0;
                }

                decimal totalFineRate = personnelFinancialReport.PaidFinesRate + personnelFinancialReport.PendingFinesRate + personnelFinancialReport.OverdueFinesRate;

                if (totalFineRate > 0)
                {
                    decimal adjustmentFactor = 100 / totalFineRate;
                    personnelFinancialReport.PaidFinesRate *= adjustmentFactor;
                    personnelFinancialReport.PendingFinesRate *= adjustmentFactor;
                    personnelFinancialReport.OverdueFinesRate *= adjustmentFactor;
                }
            }

            return View(personnelFinancialReports);
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> PersonnelAccountsReports()
        {

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var personnelAccountsReports = await _context.PersonnelAccountsReports
                .Where(pa => pa.DivisionId == divisionId)
                .ToListAsync();

            var overallPersonnelAccountsCount = await GetOverallPersonnelAccountsCountAsync();

            var activePersonnelAccountsCount = await GetActivePersonnelAccountsCountAsync();

            var inactivePersonnelAccountsCount = await GetInactivePersonnelAccountsCountAsync();

            var suspendedPersonnelAccountsCount = await GetSuspendedPersonnelAccountsCountAsync();


            foreach (var personnelAccountReport in personnelAccountsReports)
            {
                personnelAccountReport.OverallAccountsCount = overallPersonnelAccountsCount;

                personnelAccountReport.ActiveAccountsCount = activePersonnelAccountsCount;

                personnelAccountReport.InactiveAccountsCount = inactivePersonnelAccountsCount;

                personnelAccountReport.SuspendedAccountsCount = suspendedPersonnelAccountsCount;

                if(personnelAccountReport.OverallAccountsCount > 0)
                {
                    if(personnelAccountReport.ActiveAccountsCount > 0)
                    {
                        personnelAccountReport.ActiveAccountsRate = ((decimal)activePersonnelAccountsCount / overallPersonnelAccountsCount) * 100;
                    }

                    if(personnelAccountReport.InactiveAccountsCount > 0)
                    {
                        personnelAccountReport.InactiveAccountsRate = ((decimal)inactivePersonnelAccountsCount / overallPersonnelAccountsCount) *100;
                    }

                    if(personnelAccountReport.SuspendedAccountsCount > 0)
                    {
                        personnelAccountReport.SuspendedAccountsRate = ((decimal)suspendedPersonnelAccountsCount / overallPersonnelAccountsCount) * 100;
                    }

                    decimal totalPersonnelAccountsRate = personnelAccountReport.ActiveAccountsRate +
                               personnelAccountReport.InactiveAccountsRate +
                               personnelAccountReport.SuspendedAccountsRate;
                    if (totalPersonnelAccountsRate > 0)
                    {
                        decimal adjustmentFactor = 100 / totalPersonnelAccountsRate;
                        personnelAccountReport.ActiveAccountsRate *= adjustmentFactor;
                        personnelAccountReport.InactiveAccountsRate *= adjustmentFactor;
                        personnelAccountReport.SuspendedAccountsRate *= adjustmentFactor;
                    }

                }
            }


            return View(personnelAccountsReports);
        }

        public async Task<IActionResult> WarningsReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            return View();
        }

        public async Task<IActionResult> Feedback()
        {
            var feedback = await _context.TestUserFeedbacks
                .OrderByDescending(fd => fd.CreatedDateTime)
                .ToListAsync();

            return View(feedback);
        }

        [HttpGet]
        public async Task<IActionResult> TestFeedback()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> TestFeedback(FeedbackViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var feedback = new TestUserFeedback
                {
                    FeedbackText = viewModel.FeedbackText,
                    CreatedDateTime = DateTime.Now
                };

                _context.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Thanks for sharing your feedback successfully. We value your opinion.";

                return RedirectToAction(nameof(TestFeedback));
            }

            return View(viewModel);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> IndividualNewsReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var newsReports = await _context.IndividualNewsReports
                .Where(nr => nr.DivisionId == divisionId)
                .Include(n => n.SportNews)
                .ToListAsync();

            return View(newsReports);
        }

        [Authorize(Roles = ("News Administrator, News Updator"))]
        public async Task<IActionResult> NewsReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var newsReports = await _context.OverallNewsReports
                .Where(nr => nr.DivisionId == divisionId)
               
                .FirstOrDefaultAsync();

            var overallReportsCount = await GetOveralNewsItemsCount();

            if (overallReportsCount > 0)
            {
                newsReports.ApprovedNewsRate = ((decimal)newsReports.ApprovedNewsCount / overallReportsCount) * 100;
                newsReports.PublishedNewsRate= ((decimal)newsReports.PublishedNewsCount / overallReportsCount) * 100;
                newsReports.RejectedNewsRate = ((decimal)newsReports.RejectedNewsCount / overallReportsCount) * 100;

                decimal totalNewsRate = newsReports.ApprovedNewsRate + newsReports.PublishedNewsRate + newsReports.RejectedNewsRate;
                if (totalNewsRate > 0)
                {
                    decimal adjustmentFactor = 100 / totalNewsRate;
                    newsReports.RejectedNewsRate *= adjustmentFactor;
                    newsReports.PublishedNewsRate *= adjustmentFactor;
                    newsReports.ApprovedNewsRate *= adjustmentFactor;
                }
            }


            return View(newsReports);
        }
        [Authorize(Roles = ("Club Administrator, Club Manager, Player"))]
        public async Task<IActionResult> ClubPerformanceReports()
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

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var clubSubscription = await _context.Subscriptions
                .Where(cs => cs.ClubId == clubId)
                .FirstOrDefaultAsync();

            if (clubSubscription == null ||
                clubSubscription.SubscriptionPlan != SubscriptionPlan.Club_Premium ||
                (clubSubscription.SubscriptionPlan == SubscriptionPlan.Club_Premium &&
                 (clubSubscription.SubscriptionStatus == SubscriptionStatus.Expired ||
                  clubSubscription.SubscriptionStatus == SubscriptionStatus.Cancelled)))
            {
                return RedirectToAction("ClubSubscribe", "Subscriptions");
            }

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubPerformanceReports = await _context.ClubPerformanceReports
                .Where(c => c.ClubId == clubId && c.League.IsCurrent &&
                c.DivisionId == divisionId)
                .Include(c => c.League)
                .Include(c => c.Club)
                .ToListAsync();

            // Get the total number of clubs in the division
            var totalClubs = await GetClubsCount();

            // Calculate matches per team
            var matchesPerTeam = (totalClubs - 1) * 2;

            foreach (var clubPerformanceReport in clubPerformanceReports)
            {
                // Set GamesToPlayCount to matches per team
                clubPerformanceReport.GamesToPlayCount = matchesPerTeam;

                // Calculate remaining metrics
                clubPerformanceReport.GamesNotPlayedCount = clubPerformanceReport.GamesToPlayCount - clubPerformanceReport.GamesPlayedCount;

                if (matchesPerTeam > 0)
                {
                    clubPerformanceReport.GamesPlayedRate = ((decimal)clubPerformanceReport.GamesPlayedCount / matchesPerTeam) * 100;
                    clubPerformanceReport.GamesNotPlayedRate = ((decimal)clubPerformanceReport.GamesNotPlayedCount / matchesPerTeam) * 100;
                    clubPerformanceReport.GamesWinRate = ((decimal)clubPerformanceReport.GamesWinCount / matchesPerTeam) * 100;
                    clubPerformanceReport.GamesLoseRate = ((decimal)clubPerformanceReport.GamesLoseCount / matchesPerTeam) * 100;
                    clubPerformanceReport.GamesDrawRate = ((decimal)clubPerformanceReport.GamesDrawCount / matchesPerTeam) * 100;

                    decimal totalMatchesRate = clubPerformanceReport.GamesPlayedRate + clubPerformanceReport.GamesNotPlayedRate;
                    if (totalMatchesRate > 0)
                    {
                        decimal adjustmentFactor = 100 / totalMatchesRate;
                        clubPerformanceReport.GamesPlayedRate *= adjustmentFactor;
                        clubPerformanceReport.GamesNotPlayedRate *= adjustmentFactor;
                    }

                    decimal totalPerformanceRate = clubPerformanceReport.GamesWinRate +
                                                   clubPerformanceReport.GamesLoseRate +
                                                   clubPerformanceReport.GamesDrawRate;
                    if (totalPerformanceRate > 0)
                    {
                        decimal adjustmentFactor = 100 / totalPerformanceRate;
                        clubPerformanceReport.GamesWinRate *= adjustmentFactor;
                        clubPerformanceReport.GamesLoseRate *= adjustmentFactor;
                        clubPerformanceReport.GamesDrawRate *= adjustmentFactor;
                    }
                }
            }

            var club = await _context.Club
                .FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            ViewBag.ClubName = club?.ClubName;

            return View(clubPerformanceReports);
        }


        [Authorize(Roles = "Club Administrator")]
        public async Task<IActionResult> ClubTransferReport()
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
            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId;

            if (clubId == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var clubTransferReports = await _context.ClubTransferReports
                .Where(c => c.ClubId == clubId && c.League.IsCurrent &&
                c.DivisionId == divisionId)
                .Include(c => c.Club)
                .Include(c => c.League)
                .ToListAsync();

            foreach (var clubTransferReport in clubTransferReports)
            {
                var overallMatchCount = await GetOverallTransfersCountAsync(clubId.Value);
                clubTransferReport.OverallTransfersCount = overallMatchCount;

                clubTransferReport.NotActionedIncomingTransferCount =
                    clubTransferReport.IncomingTransfersCount -
                    (clubTransferReport.SuccessfulIncomingTransfersCount +
                     clubTransferReport.RejectedIncomingTransfersCount);

                clubTransferReport.NotActionedOutgoigTransferCount =
                    clubTransferReport.OutgoingTransfersCount -
                    (clubTransferReport.SuccessfulOutgoingTransfersCount +
                     clubTransferReport.RejectedOutgoingTransfersCount);

                if (clubTransferReport.OverallTransfersCount > 0)
                {
                    var overallTransfersCount = clubTransferReport.OverallTransfersCount;

                    clubTransferReport.OutgoingTransferRate =
                        clubTransferReport.OutgoingTransfersCount > 0 ?
                        Math.Round(((decimal)clubTransferReport.OutgoingTransfersCount / overallTransfersCount) * 100, 2) : 0;

                    clubTransferReport.IncomingTransferRate =
                        clubTransferReport.IncomingTransfersCount > 0 ?
                        Math.Round(((decimal)clubTransferReport.IncomingTransfersCount / overallTransfersCount) * 100, 2) : 0;

                    if (clubTransferReport.IncomingTransfersCount > 0)
                    {
                        clubTransferReport.SuccessfullIncomingTransferRate =
                            Math.Round(((decimal)clubTransferReport.SuccessfulIncomingTransfersCount / clubTransferReport.IncomingTransfersCount) * 100, 2);

                        clubTransferReport.RejectedIncomingTransferRate =
                            Math.Round(((decimal)clubTransferReport.RejectedIncomingTransfersCount / clubTransferReport.IncomingTransfersCount) * 100, 2);

                        clubTransferReport.NotActionedIncomingTransferRate =
                            Math.Round(((decimal)clubTransferReport.NotActionedIncomingTransferCount / clubTransferReport.IncomingTransfersCount) * 100, 2);
                    }
                    else
                    {
                        clubTransferReport.SuccessfullIncomingTransferRate = 0;
                        clubTransferReport.RejectedIncomingTransferRate = 0;
                        clubTransferReport.NotActionedIncomingTransferRate = 0;
                    }

                    if (clubTransferReport.OutgoingTransfersCount > 0)
                    {
                        clubTransferReport.SuccessfullOutgoingTransferRate =
                            Math.Round(((decimal)clubTransferReport.SuccessfulOutgoingTransfersCount / clubTransferReport.OutgoingTransfersCount) * 100, 2);

                        clubTransferReport.RejectedOutgoingTransferRate =
                            Math.Round(((decimal)clubTransferReport.RejectedOutgoingTransfersCount / clubTransferReport.OutgoingTransfersCount) * 100, 2);

                        clubTransferReport.NotActionedOutgoingTransferRate =
                            Math.Round(((decimal)clubTransferReport.NotActionedOutgoigTransferCount / clubTransferReport.OutgoingTransfersCount) * 100, 2);
                    }
                    else
                    {
                        clubTransferReport.SuccessfullOutgoingTransferRate = 0;
                        clubTransferReport.RejectedOutgoingTransferRate = 0;
                        clubTransferReport.NotActionedOutgoingTransferRate = 0;
                    }

                    decimal totalTransfersRate = clubTransferReport.OutgoingTransferRate + clubTransferReport.IncomingTransferRate;
                    if (totalTransfersRate != 0)
                    {
                        decimal adjustmentFactor = 100 / totalTransfersRate;
                        clubTransferReport.OutgoingTransferRate = Math.Round(clubTransferReport.OutgoingTransferRate * adjustmentFactor, 2);
                        clubTransferReport.IncomingTransferRate = Math.Round(clubTransferReport.IncomingTransferRate * adjustmentFactor, 2);
                    }

                    decimal totalSuccessRates = clubTransferReport.SuccessfullIncomingTransferRate +
                                                clubTransferReport.RejectedIncomingTransferRate +
                                                clubTransferReport.NotActionedIncomingTransferRate;
                    if (totalSuccessRates != 0)
                    {
                        decimal adjustmentFactor = 100 / totalSuccessRates;
                        clubTransferReport.SuccessfullIncomingTransferRate = Math.Round(clubTransferReport.SuccessfullIncomingTransferRate * adjustmentFactor, 2);
                        clubTransferReport.RejectedIncomingTransferRate = Math.Round(clubTransferReport.RejectedIncomingTransferRate * adjustmentFactor, 2);
                        clubTransferReport.NotActionedIncomingTransferRate = Math.Round(clubTransferReport.NotActionedIncomingTransferRate * adjustmentFactor, 2);
                    }

                    decimal totalRejectedRates = clubTransferReport.SuccessfullOutgoingTransferRate +
                                                 clubTransferReport.RejectedOutgoingTransferRate +
                                                 clubTransferReport.NotActionedOutgoingTransferRate;
                    if (totalRejectedRates != 0)
                    {
                        decimal adjustmentFactor = 100 / totalRejectedRates;
                        clubTransferReport.SuccessfullOutgoingTransferRate = Math.Round(clubTransferReport.SuccessfullOutgoingTransferRate * adjustmentFactor, 2);
                        clubTransferReport.RejectedOutgoingTransferRate = Math.Round(clubTransferReport.RejectedOutgoingTransferRate * adjustmentFactor, 2);
                        clubTransferReport.NotActionedOutgoingTransferRate = Math.Round(clubTransferReport.NotActionedOutgoingTransferRate * adjustmentFactor, 2);
                    }
                }
            }

            var club = await _context.Club
                .FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            ViewBag.ClubName = club?.ClubName;

            return View(clubTransferReports);
        }


        [Authorize(Roles = ("Sport Administrator, Sport Coordinator, Division Manager, Club Administrator"))]
        public async Task<int> GetClubsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var clubCount = await _context.Club
                .Where(c => c.IsActive == true &&
                c.League.IsCurrent && c.DivisionId == divisionId)
                .CountAsync();

            return clubCount;
        }


        [Authorize(Roles = "Club Administrator, Club Manager, Player")]
        public async Task<IActionResult> PlayerPerformance()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
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

            var clubSubscription = await _context.Subscriptions
                .Where(cs => cs.ClubId == clubId)
                .FirstOrDefaultAsync();

            if (clubSubscription == null ||
                clubSubscription.SubscriptionPlan != SubscriptionPlan.Club_Premium ||
                (clubSubscription.SubscriptionPlan == SubscriptionPlan.Club_Premium &&
                 (clubSubscription.SubscriptionStatus == SubscriptionStatus.Expired ||
                  clubSubscription.SubscriptionStatus == SubscriptionStatus.Cancelled)))
            {
                return RedirectToAction("ClubSubscribe", "Subscriptions");
            }

            var club = await _context.Club.FirstOrDefaultAsync(mo => mo.ClubId == clubId);

            if (club == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var myPlayersPerformanceReports = await _context.PlayerPerformanceReports
                .Where(p => p.Player.ClubId == clubId
                        && p.League.IsCurrent
                        && !p.Player.IsDeleted)
                .Include(m => m.Player)
                .ToListAsync();

            ViewBag.ClubName = club.ClubName;

            return View(myPlayersPerformanceReports);
        }


        public async Task<IActionResult> OverallPlayerStats()
        {
            return View();
        }


        [Authorize(Roles = ("Sport Administrator, Sport Coordinator, Division Manager"))]
        public async Task<IActionResult> MatchReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var matchReport = await _context.MatchReports
                .Where(m => m.Season.IsCurrent && m.DivisionId == divisionId && m.DivisionId == divisionId)
                .Include(m => m.Season)
                .FirstOrDefaultAsync();

            

            var overallMatchCount = await GetOverallMatchCountAsync() * (await GetOverallMatchCountAsync() - 1);

            if (overallMatchCount > 0)
            {
                matchReport.MatchesToBePlayedCount = overallMatchCount;
            }
            else
            {
                matchReport.MatchesToBePlayedCount = 0;
            }

            matchReport.UnreleasedFixturesCount = matchReport.MatchesToBePlayedCount - matchReport.FixturedMatchesCount;

            decimal fixturedMatchesRate = 0;
            decimal unfixturedMatchesRate = 0;
            decimal playedMatchesRate = 0;
            decimal postponedMatchesRate = 0;
            decimal interruptedMatchesRate = 0;

            if (matchReport.MatchesToBePlayedCount > 0)
            {
                fixturedMatchesRate = ((decimal)matchReport.FixturedMatchesCount / matchReport.MatchesToBePlayedCount) * 100;
                unfixturedMatchesRate = ((decimal)(matchReport.MatchesToBePlayedCount - matchReport.FixturedMatchesCount) / matchReport.MatchesToBePlayedCount) * 100;
                playedMatchesRate = ((decimal)matchReport.PlayedMatchesCounts / matchReport.MatchesToBePlayedCount) * 100;
                postponedMatchesRate = ((decimal)matchReport.PostponedMatchesCount / matchReport.MatchesToBePlayedCount) * 100;
                interruptedMatchesRate = ((decimal)matchReport.InterruptedMatchesCount / matchReport.MatchesToBePlayedCount) * 100;

                decimal totalPlayRates = playedMatchesRate + postponedMatchesRate + interruptedMatchesRate;

                if (totalPlayRates != 0)
                {
                    decimal adjustmentFactor = 100 / totalPlayRates;
                    playedMatchesRate *= adjustmentFactor;
                    postponedMatchesRate *= adjustmentFactor;
                    interruptedMatchesRate *= adjustmentFactor;
                }
            }

            matchReport.FixturedMatchesRate = fixturedMatchesRate;
            matchReport.UnfixturedMatchesRate = unfixturedMatchesRate;
            matchReport.PlayedMatchesRate = playedMatchesRate;
            matchReport.PostponedMatchesRate = postponedMatchesRate;
            matchReport.InterruptedMatchesRate = interruptedMatchesRate;

            var matchReports = await _context.MatchReports
                .Where(m => m.Season.IsCurrent && m.DivisionId == divisionId)
                .Include(m => m.Season)
                .ToListAsync();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.CurrentSeason = currentSeason?.LeagueYears; 

            return View(matchReports);
        }


        [Authorize(Roles = ("Sport Administrator, Sport Coordinator, Division Manager"))]
        public async Task<IActionResult> MatchResultsReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var matchResultsReport = await _context.MatchResultsReports
                .Where(m => m.Season.IsCurrent && m.DivisionId == divisionId && 
                m.DivisionId == divisionId)
                .Include(m => m.Season)
                .FirstOrDefaultAsync();

            if (matchResultsReport == null)
            {
                return NotFound();
            }

            var overallMatchResultsCount = (await GetOverallMatchResultsCountAsync()) * (await GetOverallMatchResultsCountAsync() - 1);


            if (overallMatchResultsCount > 0)
            {
                matchResultsReport.ExpectedResultsCount = overallMatchResultsCount;
            }
            else
            {
                matchResultsReport.ExpectedResultsCount = 0;
            }

            matchResultsReport.UnreleasedResultsCount = matchResultsReport.ExpectedResultsCount - matchResultsReport.ReleasedResultsCount;

            decimal releasedResultsRate = 0;
            decimal unreleasedResultsRate = 0;
            decimal winningRate = 0;
            decimal losingRate = 0;
            decimal drawingRate = 0;

            if (matchResultsReport.ExpectedResultsCount > 0)
            {
                releasedResultsRate = ((decimal)matchResultsReport.ReleasedResultsCount / matchResultsReport.ExpectedResultsCount) * 100;
                unreleasedResultsRate = ((decimal)matchResultsReport.UnreleasedResultsCount / matchResultsReport.ExpectedResultsCount) * 100;

                if (matchResultsReport.ReleasedResultsCount > 0)
                {
                    winningRate = ((decimal)matchResultsReport.WinsCount / matchResultsReport.ReleasedResultsCount) * 100;
                    losingRate = ((decimal)matchResultsReport.LosesCount / matchResultsReport.ReleasedResultsCount) * 100;
                    drawingRate = ((decimal)matchResultsReport.DrawsCount / matchResultsReport.ReleasedResultsCount) * 100;

                    decimal totalRate = winningRate + losingRate + drawingRate;
                    if (totalRate != 100)
                    {
                        decimal adjustmentFactor = 100 / totalRate;
                        winningRate *= adjustmentFactor;
                        losingRate *= adjustmentFactor;
                        drawingRate *= adjustmentFactor;
                    }
                }
            }

            matchResultsReport.ReleasedResultsRate = releasedResultsRate;
            matchResultsReport.UnreleasedMatchesRate = unreleasedResultsRate;
            matchResultsReport.WinningRate = winningRate;
            matchResultsReport.LosingRate = losingRate;
            matchResultsReport.DrawingRate = drawingRate;




            var matchResultsReports = await _context.MatchResultsReports
                .Where(m => m.Season.IsCurrent && m.DivisionId == divisionId)
                .Include(m => m.Season)
                .ToListAsync();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent && c.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.CurrentSeason = currentSeason?.LeagueYears;

            return View(matchResultsReports);
        }


        public async Task<int> GetOverallFansAccountsCountAsync()
        {
            return await _context.Fans
                .CountAsync();
        }

        public async Task<int> GetActiveFansAccountsCountAsync()
        {
            return await _context.Fans
                .Where( f => f.IsActive)
                .CountAsync();
        }

        public async Task<int> GetInactiveFansAccountsCountAsync()
        {
            return await _context.Fans
                .Where(f => !f.IsActive)
                .CountAsync();
        }

        public async Task<int> GetSuspendedFansAccountsCountAsync()
        {
            return await _context.Fans
                .Where(f => !f.IsSuspended)
                .CountAsync();
        }

        public async Task<int> GetOverallPersonnelAccountsCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var users = _context.UserBaseModel
                .Where(u =>
                    (u is Player && ((Player)u).DivisionId == divisionId) ||
                    (u is ClubManager && ((ClubManager)u).DivisionId == divisionId) ||
                    (u is ClubAdministrator && ((ClubAdministrator)u).DivisionId == divisionId) ||
                    (u is SportsMember && ((SportsMember)u).DivisionId == divisionId) ||
                    (u is DivisionManager && ((DivisionManager)u).DivisionId == divisionId))
                .ToList();

            int count = 0;

            foreach (var useraccount in users)
            {
                var roles = await _userManager.GetRolesAsync(useraccount);
                if (roles.Any())
                {
                    count++;
                }
            }

            return count;
        }

        public async Task<int> GetActivePersonnelAccountsCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var users = _context.UserBaseModel
                .Where(u =>
                    (
                        (u is Player && ((Player)u).DivisionId == divisionId) ||
                        (u is ClubManager && ((ClubManager)u).DivisionId == divisionId) ||
                        (u is ClubAdministrator && ((ClubAdministrator)u).DivisionId == divisionId) ||
                        (u is SportsMember && ((SportsMember)u).DivisionId == divisionId) ||
                        (u is DivisionManager && ((DivisionManager)u).DivisionId == divisionId)
                    ) &&
                    u.IsActive)
                .ToList();

            int count = 0;

            foreach (var useraccount in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    count++;
                }
            }

            return count;
        }


        public async Task<int> GetOveralNewsItemsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var newsItems = await _context.SportNew
            .Where(ni => ni.DivisionId == divisionId)
            .CountAsync();

            return newsItems;
        }


        public async Task<int> GetInactivePersonnelAccountsCountAsync()
        {

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var users = _context.UserBaseModel
                .Where(u =>
                    (
                        (u is Player && ((Player)u).DivisionId == divisionId) ||
                        (u is ClubManager && ((ClubManager)u).DivisionId == divisionId) ||
                        (u is ClubAdministrator && ((ClubAdministrator)u).DivisionId == divisionId) ||
                        (u is SportsMember && ((SportsMember)u).DivisionId == divisionId) ||
                        (u is DivisionManager && ((DivisionManager)u).DivisionId == divisionId)
                    ) &&
                    !u.IsActive)
                .ToList();

            int count = 0;

            foreach (var useraccount in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    count++;
                }
            }

            return count;
        }


        public async Task<int> GetSuspendedPersonnelAccountsCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var users = _context.UserBaseModel
                .Where(u =>
                    (
                        (u is Player && ((Player)u).DivisionId == divisionId) ||
                        (u is ClubManager && ((ClubManager)u).DivisionId == divisionId) ||
                        (u is ClubAdministrator && ((ClubAdministrator)u).DivisionId == divisionId) ||
                        (u is SportsMember && ((SportsMember)u).DivisionId == divisionId) ||
                        (u is DivisionManager && ((DivisionManager)u).DivisionId == divisionId)
                    ) &&
                    u.IsSuspended) 
                .ToList();


            int count = 0;

            foreach (var useraccount in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    count++;
                }
            }

            return count;
        }


        public async Task<int> GetOverallMatchesToPlayCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            return await _context.Club
                .Where(c => c.League.IsCurrent && c.DivisionId == divisionId)
                .CountAsync();
        }

        public async Task<int> GetOverallMatchResultsCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            return await _context.Club
                .Where(c => c.League.IsCurrent && c.DivisionId == divisionId)
                .CountAsync();
        }

        public async Task<int> GetOverallTransfersCountAsync(int clubId)
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId;

            return await _context.Transfer
                .Where(c =>
                    (c.SellerClub.ClubId == clubId || c.CustomerClub.ClubId == clubId) &&
                    (c.League.IsCurrent || c.League.IsCurrent && c.DivisionId == divisionId)) 
                .CountAsync();
        }


        public async Task<int> GetOverallMatchCountAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            return await _context.Club
                  .Where(c => c.League.IsCurrent && c.DivisionId == divisionId)
                  .CountAsync();
        }



        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<IActionResult> TransfersReports()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var transferReport = await _context.TransfersReports
                 .Where(m => m.Season.IsCurrent && m.DivisionId == divisionId)
                 .Include(m => m.Season)
                 .Include(m => m.TransferPeriod)
                 .FirstOrDefaultAsync();

            decimal purchasedPercentage = ((decimal)transferReport.PurchasedPlayersCount / transferReport.TransferMarketCount) * 100;
            decimal declinedPercentage = ((decimal)transferReport.DeclinedTransfersCount / transferReport.TransferMarketCount) * 100;
            decimal notStartedPercentage = ((decimal)(transferReport.TransferMarketCount - transferReport.PurchasedPlayersCount - transferReport.DeclinedTransfersCount) / transferReport.TransferMarketCount) * 100;

            decimal successfulTransferRate = purchasedPercentage;
            decimal unsuccessfulTransferRate = declinedPercentage;
            decimal notStartedTransferRate = notStartedPercentage;

            transferReport.SuccessfulTranferRate = successfulTransferRate;
            transferReport.UnsuccessfulTranferRate = unsuccessfulTransferRate;
            transferReport.NotStartedTransferRate = notStartedTransferRate;

            var transferReports = await _context.TransfersReports
                 .Where(m => m.Season.IsCurrent &&
                 m.DivisionId == divisionId)
                 .Include(m => m.Season)
                 .FirstOrDefaultAsync();

            var currentSeason = await _context.League
                  .Where(c => c.IsCurrent)
                  .FirstOrDefaultAsync();


            ViewBag.CurrentSeason = currentSeason.LeagueYears;

            return View(transferReports);
        }
    }
}
