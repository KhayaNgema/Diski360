using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using MyField.Data;
using MyField.Interfaces;
using MyField.Models;
using MyField.Services;
using MyField.ViewModels;
using System.Linq;


namespace MyField.Controllers
{
    public class LivesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly IEncryptionService _encryptionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<MatchHub> _hubContext;
        private readonly RequestLogService _requestLogService;

        public LivesController(Ksans_SportsDbContext context,
            FileUploadService fileUploadService,
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            IEncryptionService encryptionService,
            IServiceProvider serviceProvider,
            IHubContext<MatchHub> hubContext,
            RequestLogService requestLogService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _encryptionService = encryptionService;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _requestLogService = requestLogService;
        }

        [HttpGet]
        public async Task<IActionResult> MatchOverview(int fixtureId)
        {
            var overviewViewModel = new OverviewViewModel
            {
                FixtureId = fixtureId
            };

            return PartialView("_OverviewPartial", overviewViewModel);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> StartLive(string fixtureId)
        {
            var decryptedFixtureId = _encryptionService.DecryptToInt(fixtureId);

            var user = await _userManager.GetUserAsync(User);

            var userSubscription = await _context.Subscriptions
                .Where(us => us.UserId == user.Id)
                .FirstOrDefaultAsync();

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == decryptedFixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.Division)
                .FirstOrDefaultAsync();

            if (!User.IsInRole("Sport Coordinator"))
            {
                if (userSubscription == null || userSubscription.SubscriptionPlan != SubscriptionPlan.Premium)
                {
                    return RedirectToAction("UserSubscribe", "Subscriptions");
                }


                if (userSubscription.SubscriptionPlan == SubscriptionPlan.Premium && fixture.FixtureStatus == FixtureStatus.Live)
                {
                    var userLeaderboard = await _context.CompetitionParticipants
                        .Where(ub => ub.UserId == user.Id)
                        .FirstOrDefaultAsync();

                    userLeaderboard.Points++;

                    _context.Update(userLeaderboard);
                    await _context.SaveChangesAsync();
                }
            }

            if (fixture == null)
            {
                return NotFound("Fixture not found.");
            }

            var startLiveViewModel = new StartLiveViewModel
            {
                FixtureId = decryptedFixtureId,
                FixturedClubs = $"{fixture.HomeTeam.ClubName} vs {fixture.AwayTeam.ClubName}",
                KickOffDate = fixture.KickOffDate,
                KickOffTime = fixture.KickOffTime,
                HomeTeamId = fixture.HomeTeamId,
                AwayTeamId = fixture.AwayTeamId,
                HomeTeamName = fixture.HomeTeam.ClubName,
                HomeTeamBadge = fixture.HomeTeam.ClubBadge,
                AwayTeamName = fixture.AwayTeam.ClubName,
                AwayTeamBadge = fixture.AwayTeam.ClubBadge,
                LiveTime = 0,
                HomeTeamScore = 0,
                AwayTeamScore = 0
            };

            var homePlayers = await _context.Player
                .Where(p => p.ClubId == fixture.HomeTeamId &&
                p.IsOnPitch)
                .Select(p => new
                {
                    PlayerId = p.Id,
                    FullName = $"{p.FirstName} {p.LastName}",
                    JerseyNumber = p.JerseyNumber
                })
                .ToListAsync();
            var homeCardPlayers = await _context.Player
               .Where(p => p.ClubId == fixture.HomeTeamId)
               .Select(p => new
               {
               PlayerId = p.Id,
               FullName = $"{p.FirstName} {p.LastName}",
               JerseyNumber = p.JerseyNumber
               })
               .ToListAsync();




            var awayPlayers = await _context.Player
               .Where(p => p.ClubId == fixture.AwayTeamId &&
               p.IsOnPitch)
               .Select(p => new
               {
                   PlayerId = p.Id,
                   FullName = $"{p.FirstName} {p.LastName}",
                   JerseyNumber = p.JerseyNumber
               })
               .ToListAsync();

            var awayCardPlayers = await _context.Player
             .Where(p => p.ClubId == fixture.AwayTeamId)
            .Select(p => new
            {
            PlayerId = p.Id,
            FullName = $"{p.FirstName} {p.LastName}",
            JerseyNumber = p.JerseyNumber
            })
            .ToListAsync();


            var homeLineUpXI = await _context.LineUpXI
               .Where(l => l.FixtureId == decryptedFixtureId &&
               l.ClubId == fixture.HomeTeam.ClubId &&
               l.ClubPlayer.IsOnPitch && l.ClubPlayer.HasPlayed)
               .Include(l => l.ClubPlayer)
               .Select(l => new
               {
                   PlayerId = l.ClubPlayer.Id,
                   FullName = $"{l.ClubPlayer.FirstName} {l.ClubPlayer.LastName}",
                   JerseyNumber = l.ClubPlayer.JerseyNumber
               })
              .ToListAsync();

            var homeLineUpSubstitutes = await _context.LineUpSubstitutes
                .Where(l => l.FixtureId == decryptedFixtureId &&
                l.ClubId == fixture.HomeTeam.ClubId &&
                l.ClubPlayer.HasPlayed == false && l.ClubPlayer.IsOnPitch == false)
                .Include(l => l.ClubPlayer)
                .Select(l => new
                {
                    PlayerId = l.ClubPlayer.Id,
                    FullName = $"{l.ClubPlayer.FirstName} {l.ClubPlayer.LastName}",
                    JerseyNumber = l.ClubPlayer.JerseyNumber
                })
                .ToListAsync();

            var awayLineUpXI = await _context.LineUpXI
                .Where(l => l.FixtureId == decryptedFixtureId &&
                l.ClubId == fixture.AwayTeam.ClubId &&
                l.ClubPlayer.IsOnPitch && l.ClubPlayer.HasPlayed)
                .Include(l => l.ClubPlayer)
                .Select(l => new
                {
                    PlayerId = l.ClubPlayer.Id,
                    FullName = $"{l.ClubPlayer.FirstName} {l.ClubPlayer.LastName}",
                    JerseyNumber = l.ClubPlayer.JerseyNumber
                })
                .ToListAsync();

            var awayLineUpSubstitutes = await _context.LineUpSubstitutes
                .Where(l => l.FixtureId == decryptedFixtureId &&
                l.ClubId == fixture.AwayTeam.ClubId &&
                l.ClubPlayer.HasPlayed == false && l.ClubPlayer.IsOnPitch == false)
                .Include(l => l.ClubPlayer)
                .Select(l => new
                {
                    PlayerId = l.ClubPlayer.Id,
                    FullName = $"{l.ClubPlayer.FirstName} {l.ClubPlayer.LastName}",
                    JerseyNumber = l.ClubPlayer.JerseyNumber
                })
                .ToListAsync();



            var homeGoalCombinedViewModel = new HomeGoalCombinedViewModel
            {
                Players = homePlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                HomeTeam = fixture.HomeTeam.ClubName
            };

            var yellowCardTypes = Enum.GetValues(typeof(YellowCardReason))
                  .Cast<YellowCardReason>()
                  .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() })
                  .ToList();

            var redCardTypes = Enum.GetValues(typeof(RedCardReason))
                .Cast<RedCardReason>()
                .Select(p => new SelectListItem { Value = p.ToString(), Text = p.ToString() })
                .ToList();

            var awayGoalCombinedViewModel = new AwayGoalCombinedViewModel
            {
                Players = awayPlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                AwayTeam = fixture.AwayTeam.ClubName
            };

            var homeYellowViewModel = new HomeYellowViewModel
            {
                Players = homeCardPlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                HomeTeam = fixture.HomeTeam.ClubName,

                YellowCardTypes = yellowCardTypes
            };

            var awayYellowViewModel = new AwayYellowViewModel
            {
                Players = awayCardPlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                AwayTeam = fixture.AwayTeam.ClubName,

                YellowCardTypes = yellowCardTypes
            };


            var homeRedViewModel = new HomeRedViewModel
            {
                Players = homeCardPlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                HomeTeam = fixture.HomeTeam.ClubName,

                RedCardTypes = redCardTypes
            };

            var awayRedViewModel = new AwayRedViewModel
            {
                Players = awayCardPlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                AwayTeam = fixture.AwayTeam.ClubName,

                RedCardTypes = redCardTypes
            };

            var homePenaltyViewModel = new HomePenaltyViewModel
            {
                Players = homePlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                HomeTeam = fixture.HomeTeam.ClubName
            };

            var awayPenaltyViewModel = new AwayPenaltyViewModel
            {
                Players = awayPlayers.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                AwayTeam = fixture.AwayTeam.ClubName
            };


            var homeSubViewModel = new HomeSubViewModel
            {
                StartingXi = homeLineUpXI.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                Substitutes = homeLineUpSubstitutes.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                HomeTeam = fixture.HomeTeam.ClubName
            };

            var awaySubViewModel = new AwaySubViewModel
            {
                StartingXi = awayLineUpXI.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                Substitutes = awayLineUpSubstitutes.Select(p => new
                {
                    p.PlayerId,
                    p.FullName,
                    p.JerseyNumber
                }).ToList(),

                FixtureId = fixture.FixtureId,

                AwayTeam = fixture.AwayTeam.ClubName,
            };


            var userRole = await _context.UserRoles
                 .Where(ur => ur.UserId == user.Id)
                 .Join(_context.Roles,
                 ur => ur.RoleId,
                 r => r.Id,
                 (ur, r) => r.Name)
                .FirstOrDefaultAsync();

            var overviewViewModel = new OverviewViewModel
            {
                FixtureId = decryptedFixtureId
            };

            var liveMatchViewModel = new LiveMatchViewModel
            {
                ReasonForInterruption = null
            };

            var homeOwnGoalViewModel = new HomeOwnGoalViewModel
            {
                Players = awayPlayers,
                HomeTeam = fixture.HomeTeam.ClubName,
                FixtureId = fixture.FixtureId
            };

            var awayOwnGoalViewModel = new AwayOwnGoalViewModel
            {
                Players = homePlayers,
                AwayTeam = fixture.AwayTeam.ClubName,
                FixtureId = fixture.FixtureId
            };

            var combinedViewModel = new CombinedStartLiveViewModel
            {
                StartLiveViewModel = startLiveViewModel,
                HomeGoalCombinedViewModel = homeGoalCombinedViewModel,
                AwayGoalCombinedViewModel = awayGoalCombinedViewModel,
                HomeYellowViewModel = homeYellowViewModel,
                AwayYellowViewModel = awayYellowViewModel,
                HomeRedViewModel = homeRedViewModel,
                AwayRedViewModel = awayRedViewModel,
                HomePenaltyViewModel = homePenaltyViewModel,
                AwayPenaltyViewModel = awayPenaltyViewModel,
                HomeSubViewModel = homeSubViewModel,
                AwaySubViewModel = awaySubViewModel,
                AddedTime = 0,
                UserRole = userRole,
                OverviewViewModel = overviewViewModel,
                LiveMatchViewModel = liveMatchViewModel,
                HomeOwnGoalViewModel = homeOwnGoalViewModel,
                AwayOwnGoalViewModel = awayOwnGoalViewModel
            };

            ViewBag.DivisionName = $"{fixture.Division.DivisionName}({fixture.Division.DivisionAbbr})";

            return View(combinedViewModel);
        }

        [Authorize]
        public async Task<IActionResult> MatchLineUpsFans(int fixtureId)
        {
            var matchLineUps = _context.LineUpXI
                .Where(mo => mo.FixtureId == fixtureId)
                .Include(s => s.Fixture)
                .FirstOrDefault();

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == matchLineUps.FixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            ViewBag.FixtureId = fixture.FixtureId;
            ViewBag.HomeTeamId = fixture.HomeTeam.ClubId;
            ViewBag.AwayTeamId = fixture.AwayTeam.ClubId;

            return PartialView("_LiveMatchLineUpsFansPartial", matchLineUps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMatch([FromBody] StartLiveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingLiveMatch = await _context.Live
                .Where(lm => lm.FixtureId == viewModel.FixtureId)
                .FirstOrDefaultAsync();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent)
                .FirstOrDefaultAsync();

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == viewModel.FixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            fixture.FixtureStatus = FixtureStatus.Live;

            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            _context.Update(fixture);

            if (existingLiveMatch != null)
            {
                if (existingLiveMatch.ISEnded)
                {
                    TempData["Message"] = "Cannot start match, it has already ended!";
                    return BadRequest(new { success = false, message = TempData["Message"] });
                }

                TempData["Message"] = "Match already started!";
                return BadRequest(new { success = false, message = TempData["Message"] });
            }

            var newLive = new Live
            {
                FixtureId = viewModel.FixtureId,
                LeagueId = currentSeason.LeagueId,
                HomeTeamScore = viewModel.HomeTeamScore,
                AwayTeamScore = viewModel.AwayTeamScore,
                ISEnded = false,
                IsHalfTime = false,
                IsLive = true,
                LiveTime = 0,
                DivisionId = divisionId
            };

            _context.Live.Add(newLive);
            await _context.SaveChangesAsync();

            RecurringJob.AddOrUpdate(
                $"update-live-time-{newLive.LiveId}",
                () => UpdateLiveTime(newLive.LiveId),
                Cron.Minutely);

            TempData["Message"] = "Live match started successfully!";

            await _activityLogger.Log($"Started a live match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}", user.Id);

            await _requestLogService.LogSuceededRequest("Live match started successfully.", StatusCodes.Status200OK);

            return Ok(new { success = true, liveId = newLive.LiveId, message = TempData["Message"] });
        }


        public async Task UpdateLiveTime(int liveId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<Ksans_SportsDbContext>();

                var live = await context.Live.FindAsync(liveId);
                if (live == null || !live.IsLive) return;

                live.LiveTime++;
                context.Update(live);
                await context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveMatchStatus(int fixtureId)
        {
            var liveMatch = await _context.Live
                .Where(l => l.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            if (liveMatch == null)
            {
                return NotFound();
            }

            var response = new
            {
                LiveTime = liveMatch.LiveTime,
                IsLive = liveMatch.IsLive,
                IsHalfTime = liveMatch.IsHalfTime,
                IsEnded = liveMatch.ISEnded,
                WentToHalfTime = liveMatch.WentToHalfTime,
                HomeTeamScore = liveMatch.HomeTeamScore,
                AwayTeamScore = liveMatch.AwayTeamScore,
                AddTime = liveMatch.AddedTime,
                HalfTimeScore = liveMatch.HalfTimeScore,
                RecordedTime = liveMatch.RecordedTime,
                IsInterrupted = liveMatch.IsInterrupted
            };

            await _hubContext.Clients.Group(fixtureId.ToString()).SendAsync("ReceiveUpdate", fixtureId);

            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> HalfTime(int fixtureId)
        {
            var user = await _userManager.GetUserAsync(User);

            var liveMatch = await _context.Live
                .Where(l => l.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            if (liveMatch.LiveTime < 45)
            {
                return BadRequest(new { success = false, message = "A match can't be paused for halftime break before reaching 45 minutes of play." });
                await _requestLogService.LogFailedRequest("Failed to pause a match for halftime break due to minimun half time rule.", StatusCodes.Status500InternalServerError);
            }
            else
            {
                var fixture = await _context.Fixture
                               .Where(f => f.FixtureId == fixtureId)
                               .Include(f => f.HomeTeam)
                               .Include(f => f.AwayTeam)
                               .FirstOrDefaultAsync();

                await _activityLogger.Log($"Paused match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} to halftime at {liveMatch.LiveTime}", user.Id);

                if (liveMatch != null)
                {
                    liveMatch.IsLive = false;
                    liveMatch.IsHalfTime = true;
                    liveMatch.ISEnded = false;
                    liveMatch.LiveTime = 45;
                    liveMatch.WentToHalfTime = true;
                    liveMatch.AddedTime = 0;
                    liveMatch.HalfTimeScore = $"{liveMatch.HomeTeamScore} - {liveMatch.AwayTeamScore}";
                    liveMatch.RecordedTime = DateTime.Now;

                    _context.Update(liveMatch);
                    await _context.SaveChangesAsync();

                    await _requestLogService.LogSuceededRequest("Match paused for halftime successfully.", StatusCodes.Status200OK);

                    return Ok();
                }
            }

            return NotFound("Live match not found.");
        }


        [HttpPost]
        public async Task<IActionResult> ResumeMatch(int fixtureId)
        {
            var user = await _userManager.GetUserAsync(User);

            var liveMatch = await _context.Live
                .Where(l => l.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == fixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            if (liveMatch != null)
            {
                liveMatch.IsLive = true;
                liveMatch.IsHalfTime = false;
                liveMatch.ISEnded = false;
                liveMatch.LiveTime = 45;

                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                return Ok();
            }

            await _activityLogger.Log($"Resumed match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} from halftime at {liveMatch.LiveTime}", user.Id);

            await _requestLogService.LogSuceededRequest("Match resumed from halftime successfully.", StatusCodes.Status200OK);

            return NotFound("Live match not found.");
        }

        [HttpGet]
        public async Task<IActionResult> HomeGoal()
        {
            return PartialView("_HomeGoalPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HomeGoal(int fixtureId, string goalScoredBy, string assistedBy, string scoredTime, StartLiveViewModel viewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                liveMatch.HomeTeamScore++;

                var newGoal = new LiveGoalHolder
                {
                    LeagueId = liveMatch.LeagueId,
                    ScoredById = goalScoredBy,
                    LiveId = liveMatch.LiveId,
                    ScoredTime = scoredTime,
                    RecordedTime = DateTime.Now,
                    AssistedById = assistedBy,
                };

                _context.Add(newGoal);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                await _requestLogService.LogSuceededRequest("Home goal recorded successfully.", StatusCodes.Status200OK);

                await _activityLogger.Log($"Recorded home goal in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes and new score is {liveMatch.HomeTeamScore}-{liveMatch.AwayTeamScore}", user.Id);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }





        [HttpGet]
        public async Task<IActionResult> AwayGoal()
        {

            return PartialView("_AwayGoalPartial");
        }

        [HttpPost]
        public async Task<IActionResult> AwayGoal(int fixtureId, string goalScoredBy, string assistedBy, string scoredTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                liveMatch.AwayTeamScore++;

                var newGoal = new LiveGoalHolder
                {
                    LeagueId = liveMatch.LeagueId,
                    ScoredById = goalScoredBy,
                    LiveId = liveMatch.LiveId,
                    ScoredTime = scoredTime,
                    RecordedTime = DateTime.Now,
                    AssistedById = assistedBy
                };

                _context.Add(newGoal);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Recorded away goal in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes and new score is {liveMatch.HomeTeamScore}-{liveMatch.AwayTeamScore}", user.Id);

                await _requestLogService.LogSuceededRequest("Away goal recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> HomeYellow()
        {
            return PartialView("_HomeYellowPartial");
        }

        [HttpPost]
        public async Task<IActionResult> HomeYellow(int fixtureId, string commitedBy, string cardTime, YellowCardReason yellowCardReason, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newYellowCard = new LiveYellowCardHolder
                {
                    LeagueId = liveMatch.LeagueId,
                    YellowCommitedById = commitedBy,
                    LiveId = liveMatch.LiveId,
                    YellowCardTime = cardTime,
                    RecordedTime = DateTime.Now,
                    YellowCardReason = yellowCardReason
                };
                _context.Add(newYellowCard);
                _context.Update(liveMatch);

                var offender = await _context.Player
                    .Where(o => o.Id == commitedBy)
                    .FirstOrDefaultAsync();

                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Recorded a yellow card in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes for {offender.FirstName} {offender.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Yellow card recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> AwayYellow()
        {
            return PartialView("_AwayYellowPartial");
        }

        [HttpPost]
        public async Task<IActionResult> AwayYellow(int fixtureId, string commitedBy, YellowCardReason yellowCardReason, string cardTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newYellowCard = new LiveYellowCardHolder
                {
                    LeagueId = liveMatch.LeagueId,
                    YellowCommitedById = commitedBy,
                    LiveId = liveMatch.LiveId,
                    YellowCardTime = cardTime,
                    RecordedTime = DateTime.Now,
                    YellowCardReason = yellowCardReason
                };

                _context.Add(newYellowCard);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var offender = await _context.Player
                    .Where(o => o.Id == commitedBy)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded a yellow card in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes for {offender.FirstName} {offender.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Yellow card recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> HomeRed()
        {
            return PartialView("_HomeRedPartial");
        }

        [HttpPost]
        public async Task<IActionResult> HomeRed(int fixtureId, string commitedBy, RedCardReason redCardReason, string cardTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var player = await _context.Player
                    .Where(p => p.Id == commitedBy)
                    .FirstOrDefaultAsync();

                player.IsOnPitch = false;

                _context.Update(player);
                await _context.SaveChangesAsync();

                var newRedCard = new LiveRedCardHolder
                {
                    LeagueId = liveMatch.LeagueId,
                    RedCommitedById = commitedBy,
                    LiveId = liveMatch.LiveId,
                    RedCardTime = cardTime,
                    RecordedTime = DateTime.Now,
                    RedCardReason = redCardReason
                };

                _context.Add(newRedCard);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var offender = await _context.Player
                    .Where(o => o.Id == commitedBy)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded a red card in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes for {offender.FirstName} {offender.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Red card recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> AwayRed()
        {
            return PartialView("_AwayRedPartial");
        }

        [HttpPost]
        public async Task<IActionResult> AwayRed(int fixtureId, string commitedBy, RedCardReason redCardReason, string cardTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {
                    return View(viewModel);
                }

                var player = await _context.Player
                     .Where(p => p.Id == commitedBy)
                     .FirstOrDefaultAsync();

                player.IsOnPitch = false;

                _context.Update(player);
                await _context.SaveChangesAsync();

                var newRedCard = new LiveRedCardHolder
                {
                    LeagueId = liveMatch.LeagueId,
                    RedCommitedById = commitedBy,
                    LiveId = liveMatch.LiveId,
                    RedCardTime = cardTime,
                    RecordedTime = DateTime.Now,
                    RedCardReason = redCardReason
                };

                _context.Add(newRedCard);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var offender = await _context.Player
                    .Where(o => o.Id == commitedBy)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded a yellow card in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes for {offender.FirstName} {offender.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Red card recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> HomePenalty()
        {

            return PartialView("_HomePenaltyPartial");
        }

        [HttpPost]
        public async Task<IActionResult> HomePenalty(int fixtureId, string takenBy, string penaltyTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newPenalty = new Penalty
                {
                    LeagueId = liveMatch.LeagueId,
                    LiveId = liveMatch.LiveId,
                    PenaltyTime = penaltyTime,
                    PlayerId = takenBy,
                    RecordedTime = DateTime.Now
                };

                liveMatch.HomeTeamScore++;

                _context.Add(newPenalty);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var penaltyTaker = await _context.Player
                    .Where(pt => pt.Id == takenBy) 
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded a home penalty in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes taken by {penaltyTaker.FirstName} {penaltyTaker.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Home penalty recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> AwayPenalty()
        {
            return PartialView("_AwayPenaltyPartial");
        }

        [HttpPost]
        public async Task<IActionResult> AwayPenalty(int fixtureId, string takenBy, string penaltyTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newPenalty = new Penalty
                {
                    LeagueId = liveMatch.LeagueId,
                    LiveId = liveMatch.LiveId,
                    PenaltyTime = penaltyTime,
                    PlayerId = takenBy,
                    RecordedTime = DateTime.Now
                };

                liveMatch.AwayTeamScore++;

                _context.Add(newPenalty);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var penaltyTaker = await _context.Player
                    .Where(pt => pt.Id == takenBy)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded away penalty in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes taken by {penaltyTaker.FirstName} {penaltyTaker.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Away penalty recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> HomeSub()
        {
            return PartialView("_HomeSubPartial");
        }

        [HttpPost]
        public async Task<IActionResult> HomeSub(int fixtureId, string outPlayer, string inPlayer, string subTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                var playerPerformanceReport = await _context.PlayerPerformanceReports
                    .Where(p => p.PlayerId == inPlayer && p.League.IsCurrent)
                    .Include(p => p.Player)
                    .FirstOrDefaultAsync();

                var playerIn = await _context.Player
                    .Where(p => p.Id == inPlayer)
                    .FirstOrDefaultAsync();

                var playerOut = await _context.Player
                     .Where(p => p.Id == outPlayer)
                     .FirstOrDefaultAsync();

                playerPerformanceReport.AppearancesCount++;
                playerIn.HasPlayed = true;
                playerIn.IsOnPitch = true;
                playerOut.IsOnPitch = false;

                var playerToLineUp = new LineUpXI
                {
                    FixtureId = fixtureId,
                    PlayerId = inPlayer,
                    ClubId = playerIn.ClubId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CreatedById = user.Id,
                    ModifiedById = user.Id,
                };

                var playerToSub = new LineUpSubstitutes
                {
                    FixtureId = fixtureId,
                    PlayerId = outPlayer,
                    ClubId = playerOut.ClubId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CreatedById = user.Id,
                    ModifiedById = user.Id,
                };


                _context.Add(playerToLineUp);
                _context.Add(playerToSub);
                _context.Update(playerOut);
                _context.Update(playerIn);
                _context.Update(playerPerformanceReport);


                var playerToMoveFromSubs = await _context.LineUpSubstitutes
                    .Where(pt => pt.ClubPlayer.Id == inPlayer)
                    .FirstOrDefaultAsync();

                var playerToMoveFromLineup = await _context.LineUpXI
                    .Where(pt => pt.ClubPlayer.Id == outPlayer)
                    .FirstOrDefaultAsync();

                _context.LineUpSubstitutes.Remove(playerToMoveFromSubs);
                _context.LineUpXI.Remove(playerToMoveFromLineup);
                await _context.SaveChangesAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newSub = new Substitute
                {
                    LeagueId = liveMatch.LeagueId,
                    InPlayerId = inPlayer,
                    LiveId = liveMatch.LiveId,
                    OutPlayerId = outPlayer,
                    SubTime = subTime,
                    RecordedTime = DateTime.Now
                };

                _context.Add(newSub);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Recorded home substitute in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes and {playerOut.FirstName} {playerOut.LastName} was replaced by {playerIn.FirstName} {playerIn.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Home susbstitute recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> AwaySub()
        {
            return PartialView("_AwaySubPartial");
        }

        [HttpPost]
        public async Task<IActionResult> AwaySub(int fixtureId, string outPlayer, string inPlayer, string subTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                var playerPerformanceReport = await _context.PlayerPerformanceReports
                    .Where(p => p.PlayerId == inPlayer && p.League.IsCurrent)
                    .Include(p => p.Player)
                    .FirstOrDefaultAsync();

                var playerIn = await _context.Player
                    .Where(p => p.Id == inPlayer)
                    .FirstOrDefaultAsync();

                var playerOut = await _context.Player
                    .Where(p => p.Id == outPlayer)
                    .FirstOrDefaultAsync();

                playerPerformanceReport.AppearancesCount++;

                playerIn.HasPlayed = true;
                playerIn.IsOnPitch = true;
                playerOut.IsOnPitch = false;

                var playerToLineUp = new LineUpXI
                {
                    FixtureId = fixtureId,
                    PlayerId = inPlayer,
                    ClubId = playerIn.ClubId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CreatedById = user.Id,
                    ModifiedById = user.Id,
                };

                var playerToSub = new LineUpSubstitutes
                {
                    FixtureId = fixtureId,
                    PlayerId = outPlayer,
                    ClubId = playerOut.ClubId,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CreatedById = user.Id,
                    ModifiedById = user.Id,
                };


                _context.Add(playerToLineUp);
                _context.Add(playerToSub);
                _context.Update(playerOut);
                _context.Update(playerIn);
                _context.Update(playerPerformanceReport);


                var playerToMoveFromSubs = await _context.LineUpSubstitutes
                    .Where(pt => pt.ClubPlayer.Id == inPlayer)
                    .FirstOrDefaultAsync();

                var playerToMoveFromLineup = await _context.LineUpXI
                    .Where(pt => pt.ClubPlayer.Id == outPlayer)
                    .FirstOrDefaultAsync();

                _context.LineUpSubstitutes.Remove(playerToMoveFromSubs);
                _context.LineUpXI.Remove(playerToMoveFromLineup);
                await _context.SaveChangesAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newSub = new Substitute
                {
                    LeagueId = liveMatch.LeagueId,
                    InPlayerId = inPlayer,
                    LiveId = liveMatch.LiveId,
                    OutPlayerId = outPlayer,
                    SubTime = subTime,
                    RecordedTime = DateTime.Now
                };

                _context.Add(newSub);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Recorded away substitute in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes and {playerOut.FirstName} {playerOut.LastName} was replaced by {playerIn.FirstName} {playerIn.LastName}.", user.Id);

                await _requestLogService.LogSuceededRequest("Away substitute recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddTime()
        {
            return PartialView("_AddTimePartial");
        }

        [HttpPost]
        public async Task<IActionResult> AddTime(int fixtureId, int addedTime)
        {
            var user = await _userManager.GetUserAsync(User);

            var liveMatch = await _context.Live
                .Where(l => l.FixtureId == fixtureId && l.IsLive)
                .Include(l => l.League)
                .FirstOrDefaultAsync();

            var fixture = await _context.Fixture
                .Where(f => f.FixtureId == fixtureId)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .FirstOrDefaultAsync();

            liveMatch.AddedTime = addedTime;

            _context.Update(liveMatch);
            await _context.SaveChangesAsync();

            await _activityLogger.Log($"Added {addedTime} minutes to a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName}.", user.Id);

            await _requestLogService.LogSuceededRequest("Match time added successfully.", StatusCodes.Status200OK);

            return Ok(new { success = true });
        }




        [HttpGet]
        public async Task<IActionResult> HomeOwnGoal()
        {
            return PartialView("_HomeOwnGoalPartial");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HomeOwnGoal(int fixtureId, string ownGoalScoredBy, string ownGoalTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                var playerPerformanceReport = await _context.PlayerPerformanceReports
                    .Where(p => p.PlayerId == ownGoalScoredBy && p.League.IsCurrent)
                    .Include(p => p.Player)
                    .FirstOrDefaultAsync();

                liveMatch.HomeTeamScore++;

                _context.Update(playerPerformanceReport);
                await _context.SaveChangesAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newOwnGoal = new LiveOwnGoalHolder
                {
                    OwnGoalScoredById = ownGoalScoredBy,
                    OwnGoalTime = ownGoalTime,
                    RecordedTime = DateTime.Now,
                    LeagueId = liveMatch.LeagueId,
                    LiveId = liveMatch.LiveId,
                    
                };

                _context.Add(newOwnGoal);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var ownGoalScorer = await _context.Player
                    .Where(ogs => ogs.Id == ownGoalScoredBy)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded own goal in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes scored by {ownGoalScorer.FirstName} {ownGoalScorer.LastName}", user.Id);

                await _requestLogService.LogSuceededRequest("Own goal recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }



        [HttpGet]
        public async Task<IActionResult> AwayOwnGoal()
        {
            return PartialView("_HomeOwnGoalPartial");
        }

        [HttpPost]
        public async Task<IActionResult> AwayOwnGoal(int fixtureId, string ownGoalScoredBy, string ownGoalTime, StartLiveViewModel viewModel)
        {
            try
            {
                Console.WriteLine($"Received Fixture ID: {fixtureId}");

                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                var playerPerformanceReport = await _context.PlayerPerformanceReports
                    .Where(p => p.PlayerId == ownGoalScoredBy && p.League.IsCurrent)
                    .Include(p => p.Player)
                    .FirstOrDefaultAsync();

                liveMatch.AwayTeamScore++;

                _context.Update(playerPerformanceReport);
                await _context.SaveChangesAsync();

                if (liveMatch == null)
                {
                    Console.WriteLine("Live match not found.");
                    return BadRequest(new { success = false, message = "Live match not found." });
                }

                if (!liveMatch.IsLive)
                {

                    return View(viewModel);
                }

                var newOwnGoal = new LiveOwnGoalHolder
                {
                    OwnGoalScoredById = ownGoalScoredBy,
                    OwnGoalTime = ownGoalTime,
                    RecordedTime = DateTime.Now,
                    LeagueId = liveMatch.LeagueId,
                    LiveId = liveMatch.LiveId,

                };

                _context.Add(newOwnGoal);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                var ownGoalScorer = await _context.Player
                    .Where(ogs => ogs.Id == ownGoalScoredBy)
                    .FirstOrDefaultAsync();

                await _activityLogger.Log($"Recorded own goal in a  match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes scored by {ownGoalScorer.FirstName} {ownGoalScorer.LastName}", user.Id);

                await _requestLogService.LogSuceededRequest("Own goal recorded successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetHomeEvents(int fixtureId)
        {
            var homeTeam = await _context.Fixture
                .Where(h => h.FixtureId == fixtureId)
                .Include(h => h.HomeTeam)
                .FirstOrDefaultAsync();


            var awayTeam = await _context.Fixture
             .Where(h => h.FixtureId == fixtureId)
             .Include(h => h.AwayTeam)
             .FirstOrDefaultAsync();


            if (homeTeam == null)
            {
                return NotFound("Fixture not found.");
            }

            var liveMatch = await _context.Live
                .Where(l => l.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            if (liveMatch == null)
            {
                return NotFound("Live match not found.");
            }


            var liveOwnGoals = await _context.LiveOwnGoalHolders
                .Where(g => g.LiveId == liveMatch.LiveId &&
                            g.OwnGoalScoredBy.ClubId == awayTeam.AwayTeam.ClubId)
                .Include(g => g.OwnGoalScoredBy)
                .Select(g => new
                {
                    OwnGoalScoredBy = g.OwnGoalScoredBy != null
                        ? $"{g.OwnGoalScoredBy.FirstName[0]}. {g.OwnGoalScoredBy.LastName}"
                        : "Unknown",
                    ScoredTime = g.OwnGoalTime,
                    RecordedTime = g.RecordedTime
                })
                .OrderByDescending(g => g.RecordedTime)
                .ToListAsync();



            var liveGoals = await _context.LiveGoalHolders
                .Where(g => g.LiveId == liveMatch.LiveId &&
                g.ScoredBy.ClubId == homeTeam.HomeTeam.ClubId)
                .Include(g => g.ScoredBy)
                .Include(g => g.AssistedBy)
                .Select(g => new
                {
                    ScoreBy = g.ScoredBy != null
                        ? $"{g.ScoredBy.FirstName[0]}. {g.ScoredBy.LastName}"
                        : "Unknown",
                    ScoredTime = g.ScoredTime,
                    RecordedTime = g.RecordedTime,
                    Assist = g.AssistedBy != null
                        ? $"{g.AssistedBy.FirstName[0]} . {g.AssistedBy.LastName}"
                        : "Unknown"
                })
                .OrderByDescending(g => g.RecordedTime)
                .ToListAsync();

            var yellowCards = await _context.LiveYellowCardHolders
                .Where(y => y.LiveId == liveMatch.LiveId &&
                y.YellowCommitedBy.ClubId == homeTeam.HomeTeam.ClubId)
                .Include(y => y.YellowCommitedBy)
                .ToListAsync();

            var processedYellowCards = yellowCards.Select(y => new
            {
                IssuedTime = y.YellowCardTime,
                IssuedTo = y.YellowCommitedBy != null
                    ? $"{y.YellowCommitedBy.FirstName[0]}. {y.YellowCommitedBy.LastName}"
                    : "Unknown",
                RecordedTime = y.RecordedTime,
                YellowCardReason = y.YellowCardReason != null
                    ? Enum.GetName(typeof(YellowCardReason), y.YellowCardReason)
                    : "Unknown"
            })
            .OrderByDescending(y => y.RecordedTime)
            .ToList();

            var redCards = await _context.LiveRedCardHolders
                .Where(r => r.LiveId == liveMatch.LiveId &&
                r.RedCommitedBy.ClubId == homeTeam.HomeTeam.ClubId)
                .Include(r => r.RedCommitedBy)
                .ToListAsync();


            var processedRedCards = redCards.Select(r => new
            {
                IssuedTime = r.RedCardTime,
                IssuedTo = r.RedCommitedBy != null
                    ? $"{r.RedCommitedBy.FirstName[0]}. {r.RedCommitedBy.LastName}"
                    : "Unknown",
                RecordedTime = r.RecordedTime,
                RedCardReason = r.RedCardReason != null
                    ? Enum.GetName(typeof(RedCardReason), r.RedCardReason)
                    : "Unknown",
            })
            .OrderByDescending(r => r.RecordedTime)
            .ToList();



            var penalties = await _context.Penalties
                .Where(p => p.LiveId == liveMatch.LiveId &&
                p.Player.ClubId == homeTeam.HomeTeam.ClubId)
                .Include(p => p.Player)
                .Select(p => new
                {
                    PenaltyTime = p.PenaltyTime,
                    TakenBy = p.Player != null
                        ? $"{p.Player.FirstName[0]}. {p.Player.LastName}"
                        : "Unknown",
                    RecordedTime = p.RecordedTime,
                })
                .OrderByDescending(p => p.RecordedTime)
                .ToListAsync();


            var substitutes = await _context.Substitutes
                .Where(s => s.LiveId == liveMatch.LiveId &&
                 s.InPlayer.ClubId == homeTeam.HomeTeam.ClubId &&
                 s.OutPlayer.ClubId == homeTeam.HomeTeam.ClubId)
                .Include(s => s.InPlayer)
                .Include(s => s.OutPlayer)
                .Select(s => new
                {
                    PlayerIn = s.InPlayer != null
                        ? $"{s.InPlayer.FirstName[0]}. {s.InPlayer.LastName}"
                        : "Unknown",
                    PlayerOut = s.OutPlayer != null
                        ? $"{s.OutPlayer.FirstName[0]}. {s.OutPlayer.LastName}"
                        : "Unknown",
                    SubstitutionTime = s.SubTime,
                    RecordedTime = s.RecordedTime,
                })
                .OrderByDescending(s => s.RecordedTime)
                .ToListAsync();

            var events = new
            {
                liveGoals,
                yellowCards = processedYellowCards,
                redCards = processedRedCards,
                penalties,
                substitutes,
                liveOwnGoals
            };


            await _hubContext.Clients.Group(fixtureId.ToString()).SendAsync("ReceiveUpdate", fixtureId);

            return Ok(events);
        }

        [HttpGet]
        public async Task<IActionResult> GetAwayEvents(int fixtureId)
        {
            var awayTeam = await _context.Fixture
                .Where(h => h.FixtureId == fixtureId)
                .Include(h => h.AwayTeam)
                .FirstOrDefaultAsync();


            var homeTeam = await _context.Fixture
              .Where(h => h.FixtureId == fixtureId)
              .Include(h => h.HomeTeam)
              .FirstOrDefaultAsync();

            if (awayTeam == null)
            {
                return NotFound("Fixture not found.");
            }

            var liveMatch = await _context.Live
                .Where(l => l.FixtureId == fixtureId)
                .FirstOrDefaultAsync();

            if (liveMatch == null)
            {
                return NotFound("Live match not found.");
            }


            var liveOwnGoals = await _context.LiveOwnGoalHolders
                .Where(g => g.LiveId == liveMatch.LiveId &&
                            g.OwnGoalScoredBy.ClubId == homeTeam.HomeTeam.ClubId)
                .Include(g => g.OwnGoalScoredBy)
                .Select(g => new
                {
                    OwnGoalScoredBy = g.OwnGoalScoredBy != null
                        ? $"{g.OwnGoalScoredBy.FirstName[0]}. {g.OwnGoalScoredBy.LastName}"
                        : "Unknown",
                    ScoredTime = g.OwnGoalTime,
                    RecordedTime = g.RecordedTime
                })
                .OrderByDescending(g => g.RecordedTime)
                .ToListAsync();


            var liveGoals = await _context.LiveGoalHolders
                .Where(g => g.LiveId == liveMatch.LiveId &&
                g.ScoredBy.ClubId == awayTeam.AwayTeam.ClubId)
                .Include(g => g.ScoredBy)
                 .Include(g => g.AssistedBy)
                .Select(g => new
                {
                    ScoreBy = g.ScoredBy != null
                        ? $"{g.ScoredBy.FirstName[0]}. {g.ScoredBy.LastName}"
                        : "Unknown",
                    ScoredTime = g.ScoredTime,
                    RecordedTime = g.RecordedTime,
                    Assist = g.AssistedBy != null
                        ? $"{g.AssistedBy.FirstName[0]} . {g.AssistedBy.LastName}"
                        : "Unknown"
                })
                .OrderByDescending(g => g.RecordedTime)
                .ToListAsync();

            var yellowCards = await _context.LiveYellowCardHolders
                .Where(y => y.LiveId == liveMatch.LiveId &&
                y.YellowCommitedBy.ClubId == awayTeam.AwayTeam.ClubId)
                .Include(y => y.YellowCommitedBy)
                .ToListAsync();

            var processedYellowCards = yellowCards.Select(y => new
            {
                IssuedTime = y.YellowCardTime,
                IssuedTo = y.YellowCommitedBy != null
                    ? $"{y.YellowCommitedBy.FirstName[0]}. {y.YellowCommitedBy.LastName}"
                    : "Unknown",
                RecordedTime = y.RecordedTime,
                YellowCardReason = y.YellowCardReason != null
                    ? Enum.GetName(typeof(YellowCardReason), y.YellowCardReason)
                    : "Unknown"
            })
            .OrderByDescending(y => y.RecordedTime)
            .ToList();

            var redCards = await _context.LiveRedCardHolders
                .Where(r => r.LiveId == liveMatch.LiveId &&
                r.RedCommitedBy.ClubId == awayTeam.AwayTeam.ClubId)
                .Include(r => r.RedCommitedBy)
                .ToListAsync();

            var processedRedCards = redCards.Select(r => new
            {
                IssuedTime = r.RedCardTime,
                IssuedTo = r.RedCommitedBy != null
                    ? $"{r.RedCommitedBy.FirstName[0]}. {r.RedCommitedBy.LastName}"
                    : "Unknown",
                RecordedTime = r.RecordedTime,
                RedCardReason = r.RedCardReason != null
                    ? Enum.GetName(typeof(RedCardReason), r.RedCardReason)
                    : "Unknown",
            })
            .OrderByDescending(r => r.RecordedTime)
            .ToList();

            var penalties = await _context.Penalties
                .Where(p => p.LiveId == liveMatch.LiveId &&
                p.Player.ClubId == awayTeam.AwayTeam.ClubId)
                .Include(p => p.Player)
                .Select(p => new
                {
                    PenaltyTime = p.PenaltyTime,
                    TakenBy = p.Player != null
                        ? $"{p.Player.FirstName[0]}. {p.Player.LastName}"
                        : "Unknown",
                    RecordedTime = p.RecordedTime,
                })
                .OrderByDescending(p => p.RecordedTime)
                .ToListAsync();

            var substitutes = await _context.Substitutes
                .Where(s => s.LiveId == liveMatch.LiveId &&
                 s.InPlayer.ClubId == awayTeam.AwayTeam.ClubId &&
                 s.OutPlayer.ClubId == awayTeam.AwayTeam.ClubId)
                .Include(s => s.InPlayer)
                .Include(s => s.OutPlayer)
                .Select(s => new
                {
                    PlayerIn = s.InPlayer != null
                        ? $"{s.InPlayer.FirstName[0]}. {s.InPlayer.LastName}"
                        : "Unknown",
                    PlayerOut = s.OutPlayer != null
                        ? $"{s.OutPlayer.FirstName[0]}. {s.OutPlayer.LastName}"
                        : "Unknown",
                    SubstitutionTime = s.SubTime,
                    RecordedTime = s.RecordedTime,
                })
                .OrderByDescending(s => s.RecordedTime)
                .ToListAsync();

            var events = new
            {
                liveGoals,
                yellowCards = processedYellowCards,
                redCards = processedRedCards,
                penalties,
                substitutes,
                liveOwnGoals
            };


            await _hubContext.Clients.Group(fixtureId.ToString()).SendAsync("ReceiveUpdate", fixtureId);

            return Ok(events);
        }


        [Authorize(Roles = "Sport Coordinator, Sport Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InterruptLive(int fixtureId, string interruptionReason)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsLive)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                liveMatch.LiveStatus = LiveStatus.Interrupted;
                liveMatch.IsInterrupted = true;
                liveMatch.ReasonForInterruption = interruptionReason;
                liveMatch.IsLive = false;

                fixture.FixtureStatus = FixtureStatus.Interrupted;
                fixture.InterruptReason = interruptionReason;

                _context.Update(liveMatch); 
                _context.Update(fixture);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Interrupted a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes due to {interruptionReason}", user.Id);

                await _requestLogService.LogSuceededRequest("Match interrupted successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to interrupt match: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }


        [Authorize(Roles = "Sport Coordinator, Sport Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResumeLive(int fixtureId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var liveMatch = await _context.Live
                    .Where(l => l.FixtureId == fixtureId && l.IsInterrupted)
                    .Include(l => l.League)
                    .FirstOrDefaultAsync();

                var fixture = await _context.Fixture
                    .Where(f => f.FixtureId == fixtureId)
                    .Include(f => f.HomeTeam)
                    .Include(f => f.AwayTeam)
                    .FirstOrDefaultAsync();

                liveMatch.LiveStatus = LiveStatus.Ongoing;
                liveMatch.IsInterrupted = false;
                liveMatch.IsLive = true;

                fixture.FixtureStatus = FixtureStatus.Live;
                fixture.InterruptReason= null;

                _context.Update(fixture);
                _context.Update(liveMatch);
                await _context.SaveChangesAsync();

                await _activityLogger.Log($"Resumed a match between {fixture.HomeTeam.ClubName} and {fixture.AwayTeam.ClubName} at {liveMatch.LiveTime} minutes.", user.Id);

                await _requestLogService.LogSuceededRequest("Match resumed successfully.", StatusCodes.Status200OK);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to resume match: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }
    }
}
