using System;
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
    public class FixturesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly RequestLogService _requestLogService;

        public FixturesController(Ksans_SportsDbContext context,
            UserManager<UserBaseModel> userManager,
            RoleManager<IdentityRole> roleManager,
            IActivityLogger activityLogger,
            EmailService emailService,
            IEncryptionService encryptionService,
            RequestLogService requestLogService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _encryptionService = encryptionService;
            _requestLogService = requestLogService;
        }

        public async Task<IActionResult> AllLeaguesFixtures(string divisionId)
        {
            return PartialView("AllLeaguesFixturesPartial");
        }

        public async Task<IActionResult> AllFixtures(string divisionId)
        {
            var currentLeagues = await _context.League
                .Where(cl => cl.IsCurrent)
                .ToListAsync();

            var combinedFixtures = new List<Fixture>();

            foreach (var currentLeague in currentLeagues)
            {
                var leagueFixtures = await _context.Fixture
                    .Where(f => f.LeagueId == currentLeague.LeagueId &&
                                (f.FixtureStatus == FixtureStatus.Upcoming ||
                                 f.FixtureStatus == FixtureStatus.Postponed ||
                                 f.FixtureStatus == FixtureStatus.Live ||
                                 f.FixtureStatus == FixtureStatus.Interrupted))
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .Include(f => f.Division)
                    .OrderBy(f => f.KickOffDate)
                    .ThenBy(f => f.KickOffTime)
                    .ToListAsync();

                combinedFixtures.AddRange(leagueFixtures);
            }

            return PartialView("_AllFixturesPartial", combinedFixtures);
        }


        public async Task<IActionResult> LeagueFixtures(string divisionId)
        {
            ViewBag.DivisionId = divisionId;

            return PartialView("LeagueFixturesPartial");
        }

        [Authorize(Roles = ("Official"))]
        public async Task<IActionResult> MatchesToOfficiate()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var matchOfficials = await _context.MatchOfficials
                .Where(m => m.RefeereId == user.Id ||
                            m.AssistantOneId == user.Id ||
                            m.AssistantTwoId == user.Id &&
                            m.DivisionId == divisionId)

                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                .ToListAsync();

            var fixtureIds = matchOfficials.Select(m => m.FixtureId).Distinct().ToList();

            var matchesToOfficiate = await _context.Fixture
                .Where(f => fixtureIds.Contains(f.FixtureId) && f.FixtureStatus == FixtureStatus.Upcoming)
                .ToListAsync();

            return View(matchesToOfficiate);
        }

        [Authorize(Roles = ("Official"))]
        public async Task<IActionResult> PreviouslyOfficiatedMatches()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var matchOfficials = await _context.MatchOfficials
                .Where(m => m.RefeereId == user.Id ||
                m.AssistantOneId == user.Id ||
                m.AssistantTwoId == user.Id &&
                m.DivisionId == divisionId)
                .Include(m => m.Fixture)
                .Include(m => m.Fixture)
                .ThenInclude(m => m.HomeTeam)
                .Include(m => m.Fixture)
                .ThenInclude(m => m.AwayTeam)
                .FirstOrDefaultAsync();

            var previouslyOfficiatedMacthes = await _context.Fixture
                .Where(m => m.FixtureId == matchOfficials.Fixture.FixtureId &&
                m.FixtureStatus == FixtureStatus.Ended)
                .ToListAsync();

            return View(previouslyOfficiatedMacthes);
        }

        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> FixtureMatchOffcials(int fixtureId)
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var fixtureOfficials = await _context.MatchOfficials
                .Where(mo => mo.FixtureId == fixtureId &&
                mo.DivisionId == divisionId)
                .Include(s => s.Fixture)
                .Include(s => s.Refeere)
                .Include(s => s.AssistantOne)
                .Include(s => s.AssistantTwo)
                .ToListAsync();
            return PartialView("_FixtureMatchOfficialsPartial", fixtureOfficials);
        }

        [Authorize(Roles = ("Club Manager, Club Administrator, Player"))]
        public async Task<IActionResult> FixtureLineUpUpdate()
        {
            var loggedInuser = await _userManager.GetUserAsync(User);

            var user = await _userManager.Users
                .Include(u => (u as ClubManager).Club)
                .Include(u => (u as ClubAdministrator).Club)
                .Include(u => (u as Player).Club)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var divisionId = (loggedInuser as ClubAdministrator)?.DivisionId ??
                 (loggedInuser as ClubManager)?.DivisionId ??
                 (loggedInuser as Player)?.DivisionId ??
                 (loggedInuser as SportsMember)?.DivisionId ??
                 (loggedInuser as Officials)?.DivisionId ??
                 (loggedInuser as DivisionManager)?.DivisionId;

            if (user == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
                return View();
            }

            ClubManager clubManager = user as ClubManager;
            ClubAdministrator clubAdministrator = user as ClubAdministrator;
            Player player = user as Player;

            if (clubManager == null && clubAdministrator == null && player == null)
            {
                return RedirectToAction("Error", "Home");
            }

            int clubId = 0;
            string clubName = string.Empty;

            if (clubManager != null)
            {
                clubId = clubManager.ClubId;
                clubName = clubManager.Club?.ClubName ?? "Unknown Club";
            }
            else if (clubAdministrator != null)
            {
                clubId = clubAdministrator.ClubId;
                clubName = clubAdministrator.Club?.ClubName ?? "Unknown Club";
            }
            else if (player != null)
            {
                clubId = player.ClubId;
                clubName = player.Club?.ClubName ?? "Unknown Club";
            }

            ViewBag.ClubName = clubName;

            var fixtures = await _context.Fixture
                .Where(f => (f.HomeTeam.ClubId == clubId || f.AwayTeam.ClubId == clubId) &&
                            (f.FixtureStatus == FixtureStatus.Upcoming ||
                             f.FixtureStatus == FixtureStatus.Postponed ||
                             f.FixtureStatus == FixtureStatus.Interrupted ||
                             f.FixtureStatus == FixtureStatus.Live) &&
                             f.LeagueId == currentLeague.LeagueId &&
                             f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .OrderBy(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .ToListAsync();

            return View(fixtures);
        }

        [Authorize]
        public async Task<IActionResult> FixtureDetailsFans(string fixtureId)
        {
            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            if (decryptedFixtureId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains("Official"))
            {
                var userSubscription = await _context.Subscriptions
                    .Where(us => us.UserId == user.Id)
                    .FirstOrDefaultAsync();

                if (userSubscription == null || userSubscription.SubscriptionPlan != SubscriptionPlan.Premium)
                {
                    return RedirectToAction("UserSubscribe", "Subscriptions");
                }
            }

            var fixture = await _context.Fixture
                .Where(f => f.FixtureStatus == FixtureStatus.Upcoming ||
                            f.FixtureStatus == FixtureStatus.Postponed ||
                            f.FixtureStatus == FixtureStatus.Interrupted)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.Division)
                .FirstOrDefaultAsync(m => m.FixtureId == decryptedFixtureId);

            ViewBag.DivisionName = $"{fixture.Division.DivisionName}({fixture.Division.DivisionAbbr})";


            if (fixture == null)
            {
                return NotFound();
            }

            return View(fixture);
        }



        [Authorize(Roles = ("Club Manager"))]
        public async Task<IActionResult> FixtureDetails(string fixtureId)
        {
            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            if (decryptedFixtureId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var fixture = await _context.Fixture
                .Where(f => f.FixtureStatus == FixtureStatus.Upcoming ||
                            f.FixtureStatus == FixtureStatus.Postponed ||
                            f.FixtureStatus == FixtureStatus.Interrupted)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.Division)
                .OrderBy(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .FirstOrDefaultAsync(m => m.FixtureId == decryptedFixtureId);

            ViewBag.DivisionName = $"{fixture.Division.DivisionName}({fixture.Division.DivisionAbbr})";


            if (fixture == null)
            {
                return NotFound();
            }

            return View(fixture);
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> FixtureDetailsBackOffice(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var fixture = await _context.Fixture
                .Where(f => f.FixtureStatus == FixtureStatus.Upcoming ||
                            f.FixtureStatus == FixtureStatus.Postponed ||
                            f.FixtureStatus == FixtureStatus.Interrupted &&
                            f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.Division)
                .FirstOrDefaultAsync(m => m.FixtureId == id);

            var matchOfficials = await _context.MatchOfficials
                .Where(m => m.FixtureId == id &&
                m.DivisionId == divisionId)
                .Include(m => m.AssistantOne)
                .Include(m => m.AssistantTwo)
                .Include(m => m.Refeere)
                .FirstOrDefaultAsync();

            var newfixture = new FixtureDetailsBackOfficeViewModel
            {
                HomeTeamName = fixture.HomeTeam.ClubName,
                AwayTeamName = fixture.AwayTeam.ClubName,
                HomeTeamBadge = fixture.HomeTeam.ClubBadge,
                AwayTeamBadge = fixture.AwayTeam.ClubBadge,
                KickOffDate = fixture.KickOffDate,
                KickOffTime = fixture.KickOffTime,
                StadiumName = fixture.Location,
                RefereeName = $"{matchOfficials.Refeere.FirstName} {matchOfficials.Refeere.LastName}",
                AssistantOneName = $"{matchOfficials.AssistantOne.FirstName} {matchOfficials.AssistantOne.LastName}",
                AssistantTwoName = $"{matchOfficials.AssistantTwo.FirstName} {matchOfficials.AssistantTwo.LastName}",
            };

            ViewBag.DivisionName = $"{fixture.Division.DivisionName}({fixture.Division.DivisionAbbr})";

            if (fixture == null)
            {
                return NotFound();
            }

            return View(newfixture);
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator, Division Manager")]
        public async Task<IActionResult> FixturesBackOffice()
        {

            var user = await _userManager.GetUserAsync(User);

            int? divisionId = null;
            dynamic loggedInUser = null;

            var divisionManager = await _context.DivisionManagers
                .Where(lu => lu.Id == user.Id)
                .Include(lu => lu.Division)
                .FirstOrDefaultAsync();

            if (divisionManager != null)
            {
                divisionId = divisionManager.DivisionId;
                loggedInUser = divisionManager;
            }
            else
            {
                var sportsMember = await _context.SportMember
                    .Where(lu => lu.Id == user.Id)
                    .Include(lu => lu.Division)
                    .FirstOrDefaultAsync();

                if (sportsMember != null)
                {
                    divisionId = sportsMember.DivisionId;
                    loggedInUser = sportsMember;
                }
            }

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var currentLeague = await _context.League
                .Where(l => l.IsCurrent &&
                l.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
                return View(new List<Fixture>());
            }

            var fixtures = await _context.Fixture
                .Where(f => (f.FixtureStatus == FixtureStatus.Upcoming ||
                             f.FixtureStatus == FixtureStatus.Postponed ||
                             f.FixtureStatus == FixtureStatus.Interrupted ||
                             f.FixtureStatus == FixtureStatus.Live||
                             f.FixtureStatus == FixtureStatus.Ended) &&
                            f.LeagueId == currentLeague.LeagueId &&
                            f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .OrderByDescending(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .ToListAsync();

            ViewBag.CurrentSeason = currentLeague.LeagueYears;

            return View(fixtures);
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> FixturesBackOfficeUsers()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }

            var fixtures = await _context.Fixture
                .Where(f => (f.FixtureStatus == FixtureStatus.Upcoming ||
                             f.FixtureStatus == FixtureStatus.Postponed ||
                             f.FixtureStatus == FixtureStatus.Interrupted ||
                             f.FixtureStatus == FixtureStatus.Live) &&
                            f.LeagueId == currentLeague.LeagueId &&
                            f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .OrderBy(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .ToListAsync();

            foreach (var fixture in fixtures)
            {
                var matchReferee = await _context.MatchOfficials
                    .Where(mo => mo.FixtureId == fixture.FixtureId &&
                    mo.DivisionId == divisionId)
                    .Include(s => s.Refeere)
                    .FirstOrDefaultAsync();

                if (matchReferee != null && matchReferee.Refeere != null)
                {
                    ViewBag.MatchReferee = matchReferee.Refeere.FirstName + " " + matchReferee.Refeere.LastName;
                }
                else
                {
                    ViewBag.MatchReferee = "Referee information not found";
                }
            }

            var division = await _context.Divisions
                .Where(d => d.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division.DivisionName;

            return View(fixtures);
        }

        [Authorize(Roles = "Sport Administrator, Sport Coordinator")]
        public async Task<IActionResult> Fixtures()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
            }


            var upcomingFixtures = await _context.Fixture
                .Where(f => (f.FixtureStatus == FixtureStatus.Upcoming ||
                f.FixtureStatus == FixtureStatus.Live ||
                f.FixtureStatus == FixtureStatus.Interrupted) &&
                            f.LeagueId == currentLeague.LeagueId &&
                            f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.MatchOfficials)
                .OrderBy(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .ToListAsync();

            return View(upcomingFixtures);
        }


        public async Task<IActionResult> FixturesMain(string divisionId)
        {
            var decryptedDivisionId = _encryptionService.DecryptToInt(divisionId);

            var currentLeague = await _context.League
                .Where(l => l.IsCurrent && l.DivisionId == decryptedDivisionId)
                .FirstOrDefaultAsync();

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
                return View();
            }

            var upcomingFixtures = await _context.Fixture
                .Where(f =>
                    (f.FixtureStatus == FixtureStatus.Upcoming ||
                     f.FixtureStatus == FixtureStatus.Postponed ||
                     f.FixtureStatus == FixtureStatus.Interrupted ||
                     f.FixtureStatus == FixtureStatus.Live)&&
                     f.LeagueId == currentLeague.LeagueId &&
                    f.DivisionId == decryptedDivisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.MatchOfficials)
                .ToListAsync();

            var referees = upcomingFixtures
                .SelectMany(f => f.MatchOfficials)
                .GroupBy(mo => mo.FixtureId)
                .ToDictionary(
                    g => g.Key,
                    g => g.FirstOrDefault(mo => mo.Refeere != null)?.Refeere.FirstName + " " + g.FirstOrDefault(mo => mo.Refeere != null)?.Refeere.LastName ?? "Referee information not found"
                );

            ViewBag.Referees = referees;

            return PartialView("_FixturesPartial", upcomingFixtures);
        }

        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> BackOfficeFixtures()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent);

            if (currentLeague == null)
            {
                ModelState.AddModelError(string.Empty, "No current league found.");
                return View("Error");
            }

            var upcomingFixtures = await _context.Fixture
                .Where(f => (f.FixtureStatus == FixtureStatus.Upcoming ||
                             f.FixtureStatus == FixtureStatus.Postponed ||
                             f.FixtureStatus == FixtureStatus.Interrupted ||
                             f.FixtureStatus == FixtureStatus.Live) &&
                             f.LeagueId == currentLeague.LeagueId &&
                             f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.CreatedBy)
                .Include(f => f.ModifiedBy)
                .OrderBy(f => f.KickOffDate)
                .ThenBy(f => f.KickOffTime)
                .ToListAsync();

            foreach (var fixture in upcomingFixtures)
            {
                var matchReferee = await _context.MatchOfficials
                    .Where(mo => mo.FixtureId == fixture.FixtureId &&
                    mo.DivisionId == divisionId)
                    .Include(s => s.Refeere)
                    .FirstOrDefaultAsync();

                if (matchReferee != null && matchReferee.Refeere != null)
                {
                    ViewBag.MatchReferee = matchReferee.Refeere.FirstName + " " + matchReferee.Refeere.LastName;
                }
                else
                {
                    ViewBag.MatchReferee = "Referee information not found";
                }
            }

            var division = await _context.Divisions
                .Where(d => d.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            ViewBag.DivisionName = division.DivisionName;

            return PartialView("_BackOfficeFixturesPartial", upcomingFixtures);
        }


        [Authorize(Roles = ("Sport Administrator, Sport Coordinator"))]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var officials = await _context.Officials
                .Where(o => o.DivisionId == divisionId)
                .ToListAsync();

            ViewBag.Officials = new SelectList(officials.Select(o => new { UserId = o.Id, FullName = o.FirstName + " " + o.LastName }), "UserId", "FullName");

            ViewBag.Clubs = new SelectList(await _context.Club.Where(mo => mo.League.IsCurrent && mo.DivisionId == divisionId).ToListAsync(), "ClubId", "ClubName");

            return View(new FixtureViewModel());
        }


        [Authorize(Roles = ("Sport Administrator, Sport Coordinator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FixtureViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            if (ModelState.IsValid)
            {
                var currentLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

                var matchReport = await _context.MatchReports
                     .Where(m => m.Season.IsCurrent &&
                     m.DivisionId == divisionId)
                     .Include(m => m.Season)
                     .FirstOrDefaultAsync();

                if (currentLeague == null)
                {
                    ModelState.AddModelError(string.Empty, "No current league found.");
                    return View(viewModel);
                }

                var newFixture = new Fixture
                {
                    LeagueId = currentLeague.LeagueId,
                    FixtureId = viewModel.FixtureId,
                    HomeTeamId = viewModel.HomeTeamId,
                    AwayTeamId = viewModel.AwayTeamId,
                    KickOffDate = viewModel.KickOffDate,
                    KickOffTime = viewModel.KickOffTime,
                    Location = viewModel.Location,
                    CreatedById = userId,
                    ModifiedById = userId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    FixtureStatus = FixtureStatus.Upcoming,
                    DivisionId = divisionId
                };

                _context.Add(newFixture);
                await _context.SaveChangesAsync();

                var newOfficials = new MatchOfficials
                {
                    FixtureId = newFixture.FixtureId,
                    RefeereId = viewModel.Refeere,
                    AssistantOneId = viewModel.AssistantOne,
                    AssistantTwoId = viewModel.AssistantTwo,
                    DivisionId = divisionId
                };

                _context.Add(newOfficials);
                await _context.SaveChangesAsync();

                matchReport.FixturedMatchesCount++;

                await _context.SaveChangesAsync();

                var newSavedFixture = await _context.Fixture
                    .Where(f => f.Equals(newFixture))
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Created a new fixture between {newSavedFixture.HomeTeam.ClubName} and {newSavedFixture.AwayTeam.ClubName}", user.Id);

                await _requestLogService.LogSuceededRequest("Fixture scheduled successfully.", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully created a new fixture between {newSavedFixture.HomeTeam.ClubName} and {newSavedFixture.AwayTeam.ClubName} that will kickoff at {newSavedFixture.KickOffTime} on {newSavedFixture.KickOffDate.ToString("dddd, dd MMM yyyy")} at  {newSavedFixture.Location}.";
                return RedirectToAction(nameof(FixturesBackOffice));
            }

            var officials = await _context.Officials.Where(o => o.DivisionId == divisionId).ToListAsync();

            ViewBag.Officials = new SelectList(officials.Select(o => new { UserId = o.Id, FullName = o.FirstName + " " + o.LastName }), "UserId", "FullName");

            ViewBag.Clubs = new SelectList(await _context.Club.Where(mo => mo.League.IsCurrent && mo.DivisionId == divisionId).ToListAsync(), "ClubId", "ClubName");

            await _requestLogService.LogFailedRequest("Failed to schedule fixture.", StatusCodes.Status500InternalServerError);

            return View(viewModel);


        }


        [Authorize(Roles = ("Sport Coordinator"))]
        public async Task<IActionResult> ModifyFixture(string fixtureId)
        {

            var user = await _userManager.GetUserAsync(User);


            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            if (decryptedFixtureId == null)
            {
                return NotFound();
            }

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            var fixture = await _context.Fixture
                .Where(f => f.FixtureStatus == FixtureStatus.Upcoming ||
                            f.FixtureStatus == FixtureStatus.Postponed ||
                            f.FixtureStatus == FixtureStatus.Interrupted &&
                            f.DivisionId == divisionId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync(f => f.FixtureId == decryptedFixtureId);

            var matchOfficials = await _context.MatchOfficials
                .Where(mo => mo.FixtureId == decryptedFixtureId)
                .Include(mo => mo.Refeere)
                .Include(mo => mo.AssistantOne)
                .Include(mo => mo.AssistantTwo)
                .FirstOrDefaultAsync();

            if (fixture == null)
            {
                return NotFound();
            }

            var viewModel = new ModifyFixtureViewModel
            {

                FixtureId = decryptedFixtureId,
                HomeTeamId = fixture.HomeTeamId,
                AwayTeamId = fixture.AwayTeamId,
                KickOffDate = fixture.KickOffDate,
                KickOffTime = fixture.KickOffTime,
                Stadium = fixture.Location,
                FixtureStatus = fixture.FixtureStatus,
                RefereeId = matchOfficials.Refeere.Id,
                AssistantOneId = matchOfficials.AssistantOne.Id,
                AssistantTwoId = matchOfficials.AssistantTwoId
            };

            ViewBag.Clubs = new SelectList(_context.Club.Where(c => c.DivisionId == divisionId), "ClubId", "ClubName");


            ViewBag.FixtureStatusOptions = Enum.GetValues(typeof(FixtureStatus))
                                              .Cast<FixtureStatus>()
                                              .Select(v => new SelectListItem
                                              {
                                                  Text = v.ToString(),
                                                  Value = v.ToString()
                                              });

            var officials = await _context.Officials.Where(o => o.DivisionId == divisionId).ToListAsync();

            ViewBag.Officials = new SelectList(officials.Select(o => new { UserId = o.Id, FullName = o.FirstName + " " + o.LastName }), "UserId", "FullName");

            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Coordinator"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyFixture(ModifyFixtureViewModel viewModel)
        {
            if (viewModel.FixtureId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {

                var userId = user.Id;

                var existingFixture = await _context.Fixture
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync(f => f.FixtureId == viewModel.FixtureId);

                try
                {
                    if (existingFixture == null)
                    {
                        return NotFound();
                    }

                    existingFixture.HomeTeamId = viewModel.HomeTeamId;
                    existingFixture.AwayTeamId = viewModel.AwayTeamId;
                    existingFixture.KickOffTime = viewModel.KickOffTime;
                    existingFixture.KickOffDate = viewModel.KickOffDate;
                    existingFixture.Location = viewModel.Stadium;
                    existingFixture.FixtureStatus = viewModel.FixtureStatus;
                    existingFixture.ModifiedDateTime = DateTime.Now;
                    existingFixture.ModifiedById = userId;

                    var newOfficials = new MatchOfficials
                    {
                        FixtureId = existingFixture.FixtureId,
                        RefeereId = viewModel.RefereeId,
                        AssistantOneId = viewModel.AssistantOneId,
                        AssistantTwoId = viewModel.AssistantTwoId,
                    };

                    _context.Add(newOfficials);
                    _context.Update(existingFixture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FixtureExists(viewModel.FixtureId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                await _activityLogger.Log($"Modified a fixture between {existingFixture.HomeTeam.ClubName} and {existingFixture.AwayTeam.ClubName} ", user.Id);

                await _requestLogService.LogSuceededRequest("Fixture Updated Successfully.", StatusCodes.Status200OK);

                TempData["Message"] = $"You have successfully updated the fixture between {existingFixture.HomeTeam.ClubName} and {existingFixture.AwayTeam.ClubName}";

                return RedirectToAction(nameof(FixturesBackOffice));
            }



            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;


            ViewBag.Clubs = new SelectList(_context.Club.Where(c => c.DivisionId == divisionId), "ClubId", "ClubName");

            ViewBag.FixtureStatusOptions = Enum.GetValues(typeof(FixtureStatus))
                                              .Cast<FixtureStatus>()
                                              .Select(v => new SelectListItem
                                              {
                                                  Text = v.ToString(),
                                                  Value = v.ToString()
                                              });

            var officials = await _context.Officials
                .Where(o => o.DivisionId == divisionId)
                .ToListAsync();

            ViewBag.Officials = new SelectList(officials.Select(o => new { UserId = o.Id, FullName = o.FirstName + " " + o.LastName }), "UserId", "FullName");

            return View(viewModel);
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> InterruptFixture(string fixtureId)
        {
            var user = await _userManager.GetUserAsync(User);

            var matchReport = await _context.MatchReports
                .Where(m => m.Season.IsCurrent)
                .Include(m => m.Season)
                .FirstOrDefaultAsync();

            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == decryptedFixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            if (fixture == null)
            {
                return NotFound();
            }

            fixture.FixtureStatus = FixtureStatus.Interrupted;
            fixture.ModifiedDateTime = DateTime.Now;
            fixture.ModifiedById = user.Id;

            _context.Update(fixture);

            matchReport.InterruptedMatchesCount++;

            await _context.SaveChangesAsync();

                var subject = "Fixture Interruption Notification";
                var emailBodyTemplate = $@"
        Hi {{0}},<br/><br/>
        The fixture between {fixture.HomeTeam.ClubName} vs {fixture.AwayTeam.ClubName} has been interrupted.<br/><br/>
        Fixture Details:<br/>
        Date: {fixture.KickOffDate.ToShortDateString()}<br/>
        Time: {fixture.KickOffTime.ToShortTimeString()}<br/>
        Location: {fixture.Location}<br/><br/>
        Please check the updated fixture schedule.<br/><br/>
        Regards,<br/>
        Diski360 Team
            ";

                var users = _userManager.Users.ToList();

            foreach (var currentUser in users)
            {

                var userSubscription = await _context.Subscriptions
                    .Where(us => us.Equals(currentUser))
                    .FirstOrDefaultAsync();

                if (userSubscription.SubscriptionPlan == SubscriptionPlan.Premium || userSubscription.SubscriptionPlan == SubscriptionPlan.Club_Premium)
                {
                    var personalizedEmailBody = string.Format(emailBodyTemplate, $"{currentUser.FirstName} {currentUser.LastName}");
                    BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(
                        currentUser.Email,
                        subject,
                        personalizedEmailBody, "Diski 360"));
                }
                else
                {

                }

            }

            await _activityLogger.Log($"Interrupted a fixture between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}", user.Id);

            await _requestLogService.LogSuceededRequest("Match interrupted successfully.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully interrupted a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}";

            return RedirectToAction(nameof(FixturesBackOffice));
        }

        [Authorize(Roles = ("Sport Administrator"))]
        public async Task<IActionResult> PostponeFixture(string fixtureId)
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var matchReport = await _context.MatchReports
                .Where(m => m.Season.IsCurrent && 
                m.DivisionId == divisionId)
                .Include(m => m.Season)
                .FirstOrDefaultAsync();

            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == decryptedFixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            var fixtureLive = await _context.Live
                .Where(fl => fl.FixtureId == decryptedFixtureId)
                .FirstOrDefaultAsync();

            if(fixtureLive != null)
            {
                var liveEvents = await _context.LiveEvents
                    .Where(le => le.LiveId == fixtureLive.LiveId)
                    .FirstOrDefaultAsync();

                if (liveEvents != null)
                {
                    _context.Remove(liveEvents);
                }
                if (fixtureLive != null)
                {
                    _context.Remove(fixtureLive);
                }

                await _context.SaveChangesAsync();
            }

            if (fixture == null)
            {
                return NotFound();
            }


            fixture.FixtureStatus = FixtureStatus.Postponed;
            fixture.ModifiedDateTime = DateTime.Now;
            fixture.ModifiedById = user.Id;

            _context.Update(fixture);


            matchReport.PostponedMatchesRate++;

            await _context.SaveChangesAsync();


            var subject = "Fixture Postponement Notification";
            var emailBodyTemplate = $@"
        Hi {{0}},<br/><br/>
        The fixture between {fixture.HomeTeam.ClubName} vs {fixture.AwayTeam.ClubName} has been postponed.<br/><br/>
        Fixture Details:<br/>
        Date: {fixture.KickOffDate.ToShortDateString()}<br/>
        Time: {fixture.KickOffTime.ToShortTimeString()}<br/>
        Location: {fixture.Location}<br/><br/>
        Please check the updated fixture schedule.<br/><br/>
        Regards,<br/>
        Diski360 Support Team
    ";


            var users = _userManager.Users.ToList();

            foreach(var systemUser in users)
            {
                var personalizedEmailBody = string.Format(emailBodyTemplate, $"{systemUser.FirstName} {systemUser.LastName}");
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(
                    systemUser.Email,
                    subject,
                    personalizedEmailBody, "Diski 360"));
            }

            await _activityLogger.Log($"Postponed a fixture between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}", user.Id);

            await _requestLogService.LogSuceededRequest("Match postponed successfully.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have successfully postponed a fixture between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}";

            return RedirectToAction(nameof(FixturesBackOffice));
        }

        public async Task<IActionResult> DeleteFixture(string fixtureId)
        {
            var user = await _userManager.GetUserAsync(User);

            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            if (decryptedFixtureId == null)
            {
                return NotFound();
            }

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == decryptedFixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            await _activityLogger.Log($"Deleted a fixture between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}", user.Id);

            await _requestLogService.LogSuceededRequest("Match deleted successfully.", StatusCodes.Status200OK);

            TempData["Message"] = $"You have deleted a fixture between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}";

            _context.Remove(fixture);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(FixturesBackOffice));
        }

        private bool FixtureExists(int id)
        {
            return (_context.Fixture?.Any(e => e.FixtureId == id)).GetValueOrDefault();
        }
    }
}
