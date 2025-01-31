using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;
using System.Diagnostics;
using MyField.ViewModels;
using MyField.Services;
using Microsoft.AspNetCore.Authorization;
using MyField.Interfaces;


namespace MyField.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;

        public HomeController(ILogger<HomeController> logger, 
            Ksans_SportsDbContext db,
            UserManager<UserBaseModel> userManager,
            EmailService emailService,
            IEncryptionService encryptionService)

        {
            _context = db;
            _logger = logger;
            _userManager = userManager;
            _emailService = emailService;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> DivisionHomePage(string tab, string divisionId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                ViewBag.ActiveTab = string.IsNullOrEmpty(tab) ? "sportnews" : tab;
                ViewBag.DivisionId = divisionId; 
            }
            else
            {
                ViewBag.ActiveTab = string.IsNullOrEmpty(tab) ? "sportnews" : tab;
                ViewBag.DivisionId = divisionId; 
            }

            return View();
        }



        public async Task<IActionResult> Index(string tab)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                var roles = await _userManager.GetRolesAsync(user);

                if (user.IsFirstTimeLogin && roles.Any())
                {
                    user.IsFirstTimeLogin = false;
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    return Redirect("/Identity/Account/Manage/ChangeFirstTimeLoginPassword");
                }
                else
                {
                    ViewBag.ActiveTab = string.IsNullOrEmpty(tab) ? "sportnews" : tab;
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                ViewBag.ActiveTab = string.IsNullOrEmpty(tab) ? "sportnews" : tab;
                return View();
            }
        }


        public IActionResult ErrorPage(string errorMessage)
        {
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.AnnouncementsCount = await GetAnnouncementsCount();

            var user = await _userManager.GetUserAsync(User);

            var roles = await _userManager.GetRolesAsync(user);


            if (roles.Contains("System Administrator"))
            {
                var transactionsReport = await _context.TransactionsReports
                    .FirstOrDefaultAsync();

                var totalTransactionsCount = await _context.Payments
                    .CountAsync();

                var succeededTransactionsCount = await _context.Payments
                    .Where(st => st.Status == PaymentPaymentStatus.Successful)
                    .CountAsync();

                var failedTransactionsCount = await _context.Payments
                    .Where(ft => ft.Status == PaymentPaymentStatus.Unsuccessful)
                    .CountAsync();

                var userAccountsReport = await _context.UserAccountsReports
                    .FirstOrDefaultAsync();

                var totalUserAccountsCount = await _context.UserBaseModel
                    .CountAsync();

                var activeUserAccountsCount = await _context.UserBaseModel
                    .Where(au => au.IsActive &&
                    !au.IsDeleted &&
                    !au.IsSuspended)
                    .CountAsync();

                var inactiveUserAccountsCount = await _context.UserBaseModel
                    .Where(iu => !iu.IsActive)
                    .CountAsync();

                var suspendedUserAccountsCount = await _context.UserBaseModel
                    .Where(su => su.IsSuspended)
                    .CountAsync();

                var deletedUserAccountsCount = await _context.UserBaseModel
                    .Where(su => su.IsDeleted)
                    .CountAsync();

                var systemPerfomanceReport = await _context.SystemPerformanceReports
                    .FirstOrDefaultAsync();

                var allsystemRequestsCount = await _context.RequestLogs
                    .CountAsync();

                var succeededRequestsCount = await _context.RequestLogs
                    .Where(sl => sl.RequestType == RequestType.Succeeded)
                    .CountAsync();

                var failedRequestsCount = await _context.RequestLogs
                    .Where(sl => sl.RequestType == RequestType.Failed)
                    .CountAsync();

                var onboardingRequestsReport = await _context.OnboardingRequestsReports
                    .FirstOrDefaultAsync();

                var onboardingRequestCount = await _context.OnboardingRequests
                    .CountAsync();

                var pendingOnboardingRequestsCount = await _context.OnboardingRequests
                    .Where(ar => ar.RequestStatus == RequestStatus.Pending)
                    .CountAsync();

                var approvedOnboardingRequestsCount = await _context.OnboardingRequests
                    .Where(ar => ar.RequestStatus == RequestStatus.Approved)
                    .CountAsync();

                var rejectedOnboardingRequestsCount = await _context.OnboardingRequests
                    .Where(rr => rr.RequestStatus == RequestStatus.Rejected)
                    .CountAsync();

                var completedOnboardingRequestsCount = await _context.OnboardingRequests
                    .Where(rr => rr.RequestStatus == RequestStatus.Completed)
                    .CountAsync();

                TransactionsReportsViewModel transactionsReportViewModel = null;
                UserAccountsReportViewModel userAccountsReportViewModel = null;
                OnboardingRequestReportViewModel onboardingRequestsViewModel = null;
                SystemPerformanceReportViewModel systemPerformanceReportViewModel = null;

                if (transactionsReport != null)
                {
                    transactionsReportViewModel = new TransactionsReportsViewModel
                    {
                        SuccessfulPaymentsCount = succeededTransactionsCount,
                        TotalTransactionsCount = totalTransactionsCount,
                        UnsuccessfulPaymentsCount = failedTransactionsCount
                    };
                }

                if (userAccountsReport != null)
                {
                    userAccountsReportViewModel = new UserAccountsReportViewModel
                    {
                        TotalUserAccountsCount = userAccountsReport.TotalUserAccountsCount,
                        ActiveUserAccountsCount = activeUserAccountsCount,
                        InactiveUserAccountsCount = inactiveUserAccountsCount,
                        SuspendedUserAccountsCount = suspendedUserAccountsCount,
                        DeletedUserAccountsCount = deletedUserAccountsCount
                    };
                }

                if (onboardingRequestsReport != null)
                {
                    onboardingRequestsViewModel = new OnboardingRequestReportViewModel
                    {
                        OnboardingRequestsTotalCount = onboardingRequestCount,
                        ApprovedRequestsCount = approvedOnboardingRequestsCount,
                        RejectedRequestsCount = rejectedOnboardingRequestsCount,
                        PendingRequestsCount = pendingOnboardingRequestsCount,
                        CompletedRequestsCount = completedOnboardingRequestsCount
                    };
                }

                if (systemPerfomanceReport != null)
                {
                    systemPerformanceReportViewModel = new SystemPerformanceReportViewModel
                    {
                        TotalRequests = allsystemRequestsCount,
                        SucceededRequests = succeededRequestsCount,
                        FailedRequests = failedRequestsCount
                    };
                }

                var combinedViewModel = new CombinedSystemAdministratorReportsViewModel
                {
                    TransactionsReportsViewModel = transactionsReportViewModel,
                    OnboardingRequestReportViewModel = onboardingRequestsViewModel,
                    UserAccountsReportViewModel = userAccountsReportViewModel,
                    SystemPerformanceReportViewModel = systemPerformanceReportViewModel
                };

                ViewBag.TotalSystemLogs = allsystemRequestsCount;

                ViewBag.TotalUserAccounts = totalUserAccountsCount;

                return View("SystemAdministratorDashboard", combinedViewModel);
            }

            else if (roles.Contains("Sport Coordinator"))
            {
                var loggedInUser = await _context.SportMember
                    .Where(lu => lu.Id == user.Id)
                    .Include(lu => lu.Division)
                    .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser.Division.DivisionName;

                ViewBag.DivisionBadge = loggedInUser.Division.DivisionBadge;

                ViewBag.ClubsCount = await GetClubsCount();

                ViewBag.FixturesCount = await GetFixturesCount();

                ViewBag.MatchResultsCount = await GetMatchResultsCount();

                ViewBag.SportCoordinatorsMeetingsCount = await GetSportCoordinatorsMeetingsCount();

                return View("SportsCoordinatorDashboard");
            }
            else if (roles.Contains("Player"))
            {
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

                var club = await _context.Club
                    .Where(mo => mo.ClubId == clubId)
                    .FirstOrDefaultAsync();

                ViewBag.MyClubFixturesCount = await GetMyClubFixturesCount();
                ViewBag.MyClubMatchResultsCount = await GetMyClubMatchResultsCount();
                ViewBag.PlayersMeetingsCount = await GetPlayersMeetingsCount();

                return View("PlayerDashboard", club);
            }
            else if (roles.Contains("Club Manager"))
            {
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

                ViewBag.MyClubFixturesCount = await GetMyClubFixturesCount();
                ViewBag.MyClubMatchResultsCount = await GetMyClubMatchResultsCount();
                ViewBag.ClubManagersMeetingsCount = await GetClubManagersMeetingsCount();

                var club = await _context.Club
                    .Where(mo => mo.ClubId == clubId)
                    .FirstOrDefaultAsync();

                if(club.ClubManager.IsContractEnded)
                {
                    return View("Index");
                }
                else
                {
                    return View("ClubManagerDashboard", club);
                }
                
            }
            else if (roles.Contains("Club Administrator"))
            {


               
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

                var club = await _context.Club
                    .Where(mo => mo.ClubId == clubId)
                    .FirstOrDefaultAsync();

                ViewBag.MyClubmanagersCount = await GetMyClubManagersCount();

                ViewBag.MyClubPlayersCount = await GetMyClubPlayersCount();

                ViewBag.MyClubFixturesCount = await GetMyClubFixturesCount();

                ViewBag.MyClubTransferRequestsCount = await GetMyClubTransferRequestsCount();

                ViewBag.CLubAdministratorsMeetingsCount = await GetClubAdministratorsMeetingsCount();

                return View("ClubAdministratorDashboard", club);
            }
            else if (roles.Contains("News Updator"))
            {
                var loggedInUser = await _context.SportMember
                     .Where(lu => lu.Id == user.Id)
                     .Include(lu => lu.Division)
                     .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser.Division.DivisionName;

                ViewBag.DivisionBadge = loggedInUser.Division.DivisionBadge;

                ViewBag.NewsUpdatersMeetingsCount = await GetNewsUpdaterMeetingsCount();

                ViewBag.ApprovedNewsCount = await GetApprovedNewsCount();

                ViewBag.PublishedNewsCount = await GetPublishedNewsCount();

                ViewBag.ToBeModifiedNewsCount = await GetToBeModifiedCount();

                return View("NewsUpdatorDashboard");
            }
            else if(roles.Contains("Sport Administrator"))
            {

                var loggedInUser = await _context.SportMember
                   .Where(lu => lu.Id == user.Id)
                   .Include(lu => lu.Division)
                   .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser.Division.DivisionName;

                ViewBag.DivisionBadge = loggedInUser.Division.DivisionBadge;
                ViewBag.ClubsCount = await GetClubsCount();

                ViewBag.FixturesCount = await GetFixturesCount();

                ViewBag.MatchResultsCount = await GetMatchResultsCount();

                ViewBag.ClubFinesCount = await GetClubFinesCount();

                ViewBag.SportAdminsMeetings = await GetSportAdminsMeetingsCount();

                return View("SportAdministratorDashboard");
            }
            else if (roles.Contains("News Administrator"))
            {
                var loggedInUser = await _context.SportMember
                         .Where(lu => lu.Id == user.Id)
                         .Include(lu => lu.Division)
                         .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser.Division.DivisionName;

                ViewBag.DivisionBadge = loggedInUser.Division.DivisionBadge;
                ViewBag.NewsPendingApprovalCount = await GetNewsPendingApprovalCount();

                ViewBag.ApprovedNewsCount = await GetApprovedNewsCount();

                ViewBag.PublishedNewsCount = await GetPublishedNewsCount();

                ViewBag.ToBeModifiedNewsCount = await GetToBeModifiedCount();

                ViewBag.NewsAdminsMeetingsCount = await GetNewsAdminMeetingsCount();

                return View("NewsAdministratorDashboard");
            }
            else if (roles.Contains("Division Manager"))
            {
                var loggedInUser = await _context.DivisionManagers
                      .Where(lu => lu.Id == user.Id)
                      .Include(lu => lu.Division)
                      .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

                ViewBag.DivisionBadge = loggedInUser?.Division?.DivisionBadge;
                ViewBag.ClubsCount = await GetClubsCount();

                ViewBag.FixturesCount = await GetFixturesCount();

                ViewBag.MatchResultsCount = await GetMatchResultsCount();

                ViewBag.ClubFinesCount = await GetClubFinesCount();

                ViewBag.SportManagersMeetings = await GetSportManagersMeetingsCount();


                return View("DivisionManagerDashboard");
            }
            else if (roles.Contains("Fans Administrator"))
            {

                ViewBag.DivisionFansCount = await GetDivisionFansCount();

                ViewBag.FansAdminsMeetingsCount = await GetFansAdminMeetingsCount();

                ViewBag.FansSupportQueriesCount = await GetFansSupportQueriesCount();

                return View("FansAdministratorDashboard");
            }
            else if (roles.Contains("Personnel Administrator"))
            {
                var loggedInUser = await _context.SportMember
                      .Where(lu => lu.Id == user.Id)
                      .Include(lu => lu.Division)
                      .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

                ViewBag.DivisionBadge = loggedInUser?.Division?.DivisionBadge;

                ViewBag.SportAdminsCount = await GetSportAdminsCount();

                ViewBag.SportManagersCount = await GetDivisionManagersCount();

                ViewBag.SportCoordinatorsCount = await GetSportCoordinatorsCount();

                ViewBag.OfficialsCount = await GetOfficialsCount();

                ViewBag.ClubAdminsCount = await GetClubAdminsCount();

                ViewBag.ClubManagersCount = await GetClubManagersCount();

                ViewBag.DivisionPlayersCount = await GetDivisionPlayersCount();

                ViewBag.NewsAdminsCount = await GetNewsAdminsCount();

                ViewBag.NewsUpdatersCount = await GetNewsUpdatersCount();

                ViewBag.FansAdminsCount = await GetFansAdminsCount();

                ViewBag.PersonnelAdminsMeetingsCount = await GetPersonnelAdminsMeetingsCount();

                return View("PersonnelAdministratorDashboard");
            }
            else if (roles.Contains("Official"))
            {
                var loggedInUser = await _context.Officials
                     .Where(lu => lu.Id == user.Id)
                     .Include(lu => lu.Division)
                     .FirstOrDefaultAsync();

                ViewBag.DivisionName = loggedInUser.Division.DivisionName;

                ViewBag.DivisionBadge = loggedInUser.Division.DivisionBadge;

                ViewBag.MatchesToOfficiateCount = await GetMatchesToOfficiateCount();

                ViewBag.PreviouslyOfficiatedMatches = await PreviousylOfficiatedMatchesCount();

                ViewBag.OfficialsMeetingsCount = await GetOfficialsMeetingsCount();

                ViewBag.SystemAdminsCount = await GetSystemAdminsCount();

                return View("OfficialsDashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<int> GetMyClubManagersCount()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var clubAdmin = await _context.ClubAdministrator
                .Where(ca => ca.Id == user.Id)
                .Include(ca => ca.Division)
                .FirstOrDefaultAsync();


            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return 0;
            }

            int myClubmanagersCount = await _context.ClubManager
                .Where(m => m.ClubId == clubAdministrator.ClubId && 
                !m.IsContractEnded && 
                !m.IsDeleted && 
                m.DivisionId == clubAdmin.DivisionId)
                .CountAsync();

                return myClubmanagersCount;
        }

        [Authorize(Roles = ("Club Administrator"))]
        public async Task<int> GetMyClubPlayersCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var userId = user.Id;


            var clubAdmin = await _context.ClubAdministrator
                .Where(ca => ca.Id == user.Id)
                .Include(ca => ca.Division)
                .FirstOrDefaultAsync();

            if (user == null || !(user is ClubAdministrator clubAdministrator))
            {
                return 0;
            }

            int myPlayersCount = await _context.Player
                .Where(m => m.ClubId == clubAdministrator.ClubId &&
                !m.IsDeleted &&
                m.DivisionId == clubAdmin.DivisionId)
                .CountAsync();

            return myPlayersCount;
        }

        [Authorize(Roles = "Club Administrator, Club Manager, Player")]
        public async Task<int> GetMyClubFixturesCount()
        {
            var user = await _userManager.GetUserAsync(User);

             
            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                         (user as ClubManager)?.DivisionId ??
                         (user as Player)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                return 0; 
            }

            if (user == null)
            {
                return 0;
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;


            if (clubId == null)
            {
                return 0;
            }


            int myClubFixturesCount = await _context.Fixture
                .Where(m => (m.HomeTeam.ClubId == clubId || m.AwayTeam.ClubId == clubId) &&
                            (m.FixtureStatus == FixtureStatus.Upcoming ||
                             m.FixtureStatus == FixtureStatus.Postponed ||
                             m.FixtureStatus == FixtureStatus.Interrupted) &&
                            m.LeagueId == currentLeague.LeagueId &&
                            m.DivisionId == divisionId)
                .CountAsync();

            return myClubFixturesCount;
        }


        [Authorize(Roles = "Club Administrator")]
        public async Task<int> GetMyClubTransferRequestsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return 0;
            }

            if (!(user is ClubAdministrator clubAdministrator) &&
                !(user is ClubManager clubManager) &&
                !(user is Player clubPlayer))
            {
                return 0;
            }

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId;

            if (clubId == null)
            {
                return 0;
            }

            int myClubTransferRequestsCount = await _context.Transfer
                .Where(m => m.SellerClub.ClubId == clubId &&
                            m.Status == TransferStatus.Pending &&
                            m.DivisionId == divisionId)
                .CountAsync();

            return myClubTransferRequestsCount;
        }

        [Authorize(Roles = "Club Administrator")]
        public async Task<int> GetClubAdministratorsMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var clubAdmin = await _context.ClubAdministrator
                .Where(ca => ca.Id == user.Id)
                .Include(ca => ca.Division)
                .FirstOrDefaultAsync();

            var clubAdminsMeetings = await _context.Meeting
                .Where(c =>
                    (c.MeetingAttendees == MeetingAttendees.Everyone ||
                     c.MeetingAttendees == MeetingAttendees.Club_Administrators) &&
                    c.DivisionId == clubAdmin.DivisionId)
                .CountAsync();

            return clubAdminsMeetings;
        }


        [Authorize]
        public async Task<int> GetAnnouncementsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var announcementsCount = await _context.Announcements
                .Where(a => a.DivisionId == divisionId)
                .CountAsync();

            return announcementsCount;
        }

        [Authorize(Roles = "Club Administrator, Club Manager, Player")]
        public async Task<int> GetMyClubMatchResultsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var clubId = (user as ClubAdministrator)?.ClubId ??
                         (user as ClubManager)?.ClubId ??
                         (user as Player)?.ClubId;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                return 0;
            }

            if (clubId == null || divisionId == null)
            {
                return 0;
            }

            var myClubMatchResultsCount = await _context.MatchResult
                .Where(m => (m.HomeTeam.ClubId == clubId || m.AwayTeam.ClubId == clubId) &&
                            m.LeagueId == currentLeague.LeagueId &&
                            m.DivisionId == divisionId)
                .CountAsync();

            return myClubMatchResultsCount;
        }

        [Authorize(Roles = "Club Manager")]
        public async Task<int> GetClubManagersMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubManager)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var clubManagersMeetings = await _context.Meeting
                .Where(c => (c.MeetingAttendees == MeetingAttendees.Everyone ||
                             c.MeetingAttendees == MeetingAttendees.Club_Managers) &&
                             c.DivisionId == divisionId)
                .CountAsync();

            return clubManagersMeetings;
        }

        [Authorize(Roles = "Fans Administrator")]
        public async Task<int> GetDivisionFansCount()
        {
            var userIdsWithRoles = await _context.UserRoles
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            var usersWithoutRoles = await _context.Users
                .Where(user => !userIdsWithRoles.Contains(user.Id))
                .CountAsync();

            return usersWithoutRoles;
        }

        [Authorize(Roles = "Fans Administrator")]
        public async Task<int> GetFansAdminMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var fansAdminMeetings = await _context.Meeting
                .Where(c => (c.MeetingAttendees == MeetingAttendees.Everyone ||
                             c.MeetingAttendees == MeetingAttendees.Fans_Administrators) &&
                            c.DivisionId == divisionId)
                .CountAsync();

            return fansAdminMeetings;
        }


        [Authorize(Roles = "Fans Administrator")]
        public async Task<int> GetFansSupportQueriesCount()
        {
            var fansSupportQueries = 0;
            return fansSupportQueries;
        }

        [Authorize(Roles = "News Administrator, News Updator")]
        public async Task<int> GetNewsPendingApprovalCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var newsPendingApproval = await _context.SportNew
                .Where(n => n.NewsStatus == NewsStatus.Awaiting_Approval &&
                            n.DivisionId == divisionId)
                .CountAsync();

            return newsPendingApproval;
        }

        [Authorize(Roles = "News Administrator, News Updator")]
        public async Task<int> GetApprovedNewsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var approvedNews = await _context.SportNew
                .Where(n => n.NewsStatus == NewsStatus.Approved &&
                            n.DivisionId == divisionId)
                .CountAsync();

            return approvedNews;
        }

        [Authorize(Roles = "News Administrator, News Updator")]
        public async Task<int> GetPublishedNewsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var publishedNewsCount = await _context.SportNew
                .Where(n => n.NewsStatus == NewsStatus.Approved &&
                            n.DivisionId == divisionId)
                .CountAsync();

            return publishedNewsCount;
        }

        [Authorize(Roles = "News Administrator, News Updator")]
        public async Task<int> GetToBeModifiedCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var toBeModifiedNewsCount = await _context.SportNew
                .Where(n => n.NewsStatus == NewsStatus.ToBeModified &&
                            n.DivisionId == divisionId)
                .CountAsync();

            return toBeModifiedNewsCount;
        }

        [Authorize(Roles = "News Administrator")]
        public async Task<int> GetNewsAdminMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var newsAdminMeetings = await _context.Meeting
                .Where(c => (c.MeetingAttendees == MeetingAttendees.Everyone ||
                             c.MeetingAttendees == MeetingAttendees.News_Administrators) &&
                             c.DivisionId == divisionId)
                .CountAsync();

            return newsAdminMeetings;
        }

        [Authorize(Roles = "News Updator")]
        public async Task<int> GetNewsUpdaterMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var newsUpdaterMeetings = await _context.Meeting
                .Where(c => (c.MeetingAttendees == MeetingAttendees.Everyone ||
                             c.MeetingAttendees == MeetingAttendees.News_Updaters) &&
                             c.DivisionId == divisionId)
                .CountAsync();

            return newsUpdaterMeetings;
        }



        [Authorize(Roles = "Official")]
        public async Task<int> GetMatchesToOfficiateCount()
        {
            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as Officials)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);


            if (divisionId == null)
            {
                return 0;
            }

            var matchOfficials = await _context.MatchOfficials
                .Where(m => (m.RefeereId == user.Id ||
                             m.AssistantOneId == user.Id ||
                             m.AssistantTwoId == user.Id) &&
                             m.DivisionId == divisionId &&
                             m.Fixture.LeagueId == currentLeague.LeagueId)
                .Include(m => m.Fixture)
                .ToListAsync();

            var officiatedFixtureIds = matchOfficials
                .Select(m => m.FixtureId)
                .Distinct()
                .ToList();

            var matchesToOfficiateCount = await _context.Fixture
                .Where(m => officiatedFixtureIds.Contains(m.FixtureId) &&
                            m.FixtureStatus == FixtureStatus.Upcoming &&
                            m.DivisionId == divisionId)
                .CountAsync();

            return matchesToOfficiateCount;
        }

        [Authorize(Roles = "Official")]
        public async Task<int> PreviousylOfficiatedMatchesCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as Officials)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);


            if (divisionId == null)
            {
                return 0;
            }

            var matchOfficials = await _context.MatchOfficials
                .Where(m => (m.RefeereId == user.Id ||
                             m.AssistantOneId == user.Id ||
                             m.AssistantTwoId == user.Id) &&
                             m.DivisionId == divisionId &&
                             m.Fixture.LeagueId == currentLeague.LeagueId)
                .Include(m => m.Fixture)
                .ToListAsync();

            var officiatedFixtureIds = matchOfficials
                .Select(m => m.FixtureId)
                .Distinct()
                .ToList();

            var previoulsyOfficiatedMatchesCount = await _context.Fixture
                .Where(m => officiatedFixtureIds.Contains(m.FixtureId) &&
                            m.FixtureStatus == FixtureStatus.Ended &&
                            m.DivisionId == divisionId)
                .CountAsync();

            return previoulsyOfficiatedMatchesCount;
        }

        [Authorize(Roles = "Official")]
        public async Task<int> GetOfficialsMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as Officials)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var officialsMeetings = await _context.Meeting
                .Where(c => (c.MeetingAttendees == MeetingAttendees.Everyone ||
                             c.MeetingAttendees == MeetingAttendees.Officials) &&
                             c.DivisionId == divisionId)
                .CountAsync();

            return officialsMeetings;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetSportAdminsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Sport Administrator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var sportAdminsCount = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) &&
                            u.IsDeleted == false &&
                            u.DivisionId == divisionId)
                .CountAsync();

            return sportAdminsCount;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetDivisionManagersCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Division Manager");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            if (divisionId == null)
            {
                return 0;
            }

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var sportManagersCount = await _context.DivisionManagers
                .Where(u => userIds.Contains(u.Id) &&
                            u.IsDeleted == false &&
                            u.DivisionId == divisionId)
                .CountAsync();

            return sportManagersCount;
        }


        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetSportCoordinatorsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Sport Coordinator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var sportCoordinatorsCount = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return sportCoordinatorsCount;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetOfficialsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Official");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var officialsCount = await _context.Officials
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return officialsCount;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetClubAdminsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Club Administrator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var clubAdminsCount = await _context.ClubAdministrator
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return clubAdminsCount;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetClubManagersCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Club Manager");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var clubManagersCount = await _context.ClubManager
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return clubManagersCount;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetDivisionPlayersCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Player");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var divisionPlayersCount = await _context.Player
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return divisionPlayersCount;
        }

        [Authorize(Roles = "Personnel Administrator")]
        public async Task<int> GetNewsAdminsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "News Administrator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);
            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var newsAdminsCount = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return newsAdminsCount;
        }


        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<int> GetNewsUpdatersCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "News Updator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var newsUpdatersCount = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return newsUpdatersCount;
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<int> GetFansAdminsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Fans Administrator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var fansAdminsCount = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false && u.DivisionId == divisionId)
                .CountAsync();

            return fansAdminsCount;
        }



        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<int> GetSystemAdminsCount()
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "System Administrator");

            if (role == null)
            {
                return 0;
            }

            var user = await _userManager.GetUserAsync(User);

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var systemAdminsCount = await _context.SportMember
                .Where(u => userIds.Contains(u.Id) && u.IsDeleted == false)
                .CountAsync();

            return systemAdminsCount;
        }

        [Authorize(Roles = ("Personnel Administrator"))]
        public async Task<int> GetPersonnelAdminsMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;


            var personnelAdminsMeetings = await _context.Meeting
                .Where(c => c.MeetingAttendees == MeetingAttendees.Everyone ||
                c.MeetingAttendees == MeetingAttendees.Personnel_Administrators && c.DivisionId == divisionId)
                .CountAsync();

            return personnelAdminsMeetings;
        }

        [Authorize(Roles = ("Player"))]
        public async Task<int> GetPlayersMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as Player)?.DivisionId;

            var playersMeetings = await _context.Meeting
                .Where(c => c.MeetingAttendees == MeetingAttendees.Everyone ||
                c.MeetingAttendees == MeetingAttendees.Players && c.DivisionId == divisionId)
                .CountAsync();

            return playersMeetings;
        }

        [Authorize(Roles = ("Sport Administrator, Sport Coordinator, Division Manager"))]
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
                .Where( c => c.IsActive == true &&
                c.League.IsCurrent && c.DivisionId == divisionId)
                .CountAsync();

            return clubCount;
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator, Division Manager")]
        public async Task<int> GetFixturesCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId ??
                             (user as SportsMember)?.DivisionId ??
                             (user as Officials)?.DivisionId ??
                             (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League
                                              .FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            // Handle scenarios where currentLeague could be null
            var fixturesCount = 0;
            if (currentLeague != null)
            {
                fixturesCount = await _context.Fixture
                                              .Where(f => f.LeagueId == currentLeague.LeagueId &&
                                                          (divisionId == null || f.DivisionId == divisionId))
                                              .CountAsync();
            }

            return fixturesCount;
        }


        [Authorize(Roles = "Sport Administrator, Sport Coordinator, Division Manager")]
        public async Task<int> GetMatchResultsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                             (user as ClubManager)?.DivisionId ??
                             (user as Player)?.DivisionId ??
                             (user as SportsMember)?.DivisionId ??
                             (user as Officials)?.DivisionId ??
                             (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League
                                              .FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            var matchResultsCount = 0;
            if (currentLeague != null)
            {
                matchResultsCount = await _context.MatchResult
                                                  .Where(m => m.LeagueId == currentLeague.LeagueId &&
                                                              (divisionId == null || m.DivisionId == divisionId))
                                                  .CountAsync();
            }

            return matchResultsCount;
        }


        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<int> GetClubFinesCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var clubFinesCount = await _context.Fines
                .Where(c => c.Club != null &&
                c.Offender == null &&
                c.PaymentStatus == PaymentStatus.Pending &&
                c.DivisionId == divisionId)
                .CountAsync();

            return clubFinesCount;
        }

        [Authorize(Roles = ("System Administrator"))]
        public async Task<int> GetAllSystemUsersCount()
        {
            var allSystemUsers = await _context.Users
                .CountAsync();

            return allSystemUsers;
        }

        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<int> GetSportAdminsMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            var sportAdminsMeetings = await _context.Meeting
                .Where(sm => sm.DivisionId == divisionId)
                .CountAsync();

            return sportAdminsMeetings;
        }

        [Authorize(Roles = ("Sport Coordinator"))]
        public async Task<int> GetSportCoordinatorsMeetingsCount()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as SportsMember)?.DivisionId;

            var sportCoordinatorsMeetings = await _context.Meeting
                .Where(c => c.MeetingAttendees == MeetingAttendees.Everyone ||
                c.MeetingAttendees == MeetingAttendees.Sport_Coordinators &&
                c.DivisionId == divisionId)
                .CountAsync();

            return sportCoordinatorsMeetings;
        }

        [Authorize(Roles = ("Division Manager"))]
        public async Task<int> GetSportManagersMeetingsCount()
        {

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var sportManagersMeetings = await _context.Meeting
                .Where(c => c.MeetingAttendees == MeetingAttendees.Everyone ||
                c.MeetingAttendees == MeetingAttendees.Division_Managers && 
                c.DivisionId == divisionId)
                .CountAsync();

            return sportManagersMeetings;
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AccountCreatedSuccessfully()
        {
            return View();
        }


        public IActionResult TermsAndConditions()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {

            var errorMessages = TempData["Errors"] as List<string> ?? new List<string>();


            var viewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessages = errorMessages
            };

            return View(viewModel);
        }



    }
}