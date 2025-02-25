using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AutoMapper;
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
    public class LeaguesController : Controller
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<UserBaseModel> _userManager;
        private readonly IActivityLogger _activityLogger;
        private readonly EmailService _emailService;
        private readonly RequestLogService _requestLogService;

        public LeaguesController(Ksans_SportsDbContext context,
            IMapper mapper,
            UserManager<UserBaseModel> userManager,
            IActivityLogger activityLogger,
            EmailService emailService,
            RequestLogService requestLogService)
        {
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _activityLogger = activityLogger;
            _emailService = emailService;
            _requestLogService = requestLogService;
        }

        [Authorize(Roles = ("Sport Administrator, Division Manager"))]

        public async Task<IActionResult> LeagueCode()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent &&
                c.DivisionId == divisionId)
                .FirstOrDefaultAsync();

            return PartialView("_LeagueCodePartial", currentSeason);
        }


        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> ClubCodes()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                 (user as ClubManager)?.DivisionId ??
                 (user as Player)?.DivisionId ??
                 (user as SportsMember)?.DivisionId ??
                 (user as Officials)?.DivisionId ??
                 (user as DivisionManager)?.DivisionId;

            var clubs = await _context.Club
                .Where(c => c.League.IsCurrent && 
                c.DivisionId == divisionId)
                .ToListAsync();

            return PartialView("_ClubCodesPartial", clubs);
        }

        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> Leagues()
        {
            var user = await _userManager.GetUserAsync(User);

            var divisionId = (user as ClubAdministrator)?.DivisionId ??
                   (user as ClubManager)?.DivisionId ??
                   (user as Player)?.DivisionId ??
                   (user as SportsMember)?.DivisionId ??
                   (user as Officials)?.DivisionId ??
                   (user as DivisionManager)?.DivisionId;

            var loggedInUser = await _context.SportMember
               .Where(lu => lu.Id == user.Id)
               .Include(lu => lu.Division)
               .FirstOrDefaultAsync();

            ViewBag.DivisionName = loggedInUser?.Division?.DivisionName;

            var leagues = _context.League
                .Where(s => s.DivisionId == divisionId)
                .OrderByDescending(l => l.CreatedDateTime)
                .Include(s =>s.CreatedBy)
                .Include(s => s.ModifiedBy)
                .ToList();
            return View(leagues);
        }

        [Authorize(Roles = ("Sport Administrator, Division Manager"))]
        public async Task<IActionResult> SecretCodes()
        {
            return View();
        }

        [Authorize(Roles = ("Division Manager"))]
        public IActionResult StartLeague()
        {
            return View();
        }

        [Authorize(Roles = ("Division Manager"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartLeague(LeagueViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var userId = user.Id;

                    var divisionId = (user as ClubAdministrator)?.DivisionId ??
                         (user as ClubManager)?.DivisionId ??
                         (user as Player)?.DivisionId ??
                         (user as SportsMember)?.DivisionId ??
                         (user as Officials)?.DivisionId ??
                         (user as DivisionManager)?.DivisionId;

                    var oldLeague = await _context.League.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

                    var oldTransferPeriod = await _context.TransferPeriod.FirstOrDefaultAsync(l => l.IsCurrent && l.DivisionId == divisionId);

                    if (oldLeague != null && oldLeague.LeagueCode != viewModel.OldLeagueCode)
                    {
                        TempData["Message"] = "League code does not match the league you are trying to end.";
                        return View(viewModel);
                    }

                    if (oldTransferPeriod != null)
                    {
                        await _activityLogger.Log($"Ended season {oldLeague.LeagueYears}", user.Id);
                        oldTransferPeriod.IsCurrent = false;
                        _context.Update(oldLeague);
                        await _context.SaveChangesAsync();
                    }

                    if (oldLeague != null)
                    {
                        oldLeague.IsCurrent = false;
                        _context.Update(oldLeague);
                        await _context.SaveChangesAsync();

                        var oldStandings = await _context.Standing.Where(s => s.LeagueId == oldLeague.LeagueId && s.DivisionId == divisionId).ToListAsync();
/*                        var oldMatchResults = await _context.MatchResult.Where(m => m.LeagueId == oldLeague.LeagueId && m.DivisionId == divisionId).ToListAsync();*/
                     /*   var oldFixtures = await _context.Fixture.Where(f => f.LeagueId == oldLeague.LeagueId && f.DivisionId == divisionId).ToListAsync();*/
                        var oldClubs = await _context.Club.Where(c => c.LeagueId == oldLeague.LeagueId && c.DivisionId == divisionId).ToListAsync();
/*                        var oldMatchFormations = await _context.MatchFormation.ToListAsync();*/
                        var oldMatchReports = await _context.MatchReports.Where(m => m.LeagueId == oldLeague.LeagueId && m.DivisionId == divisionId).ToListAsync();
                        var oldMatchResultsReports = await _context.MatchResultsReports.Where(r => r.LeagueId == oldLeague.LeagueId).ToListAsync();
                        var oldTransfersReports = await _context.TransfersReports.Where(t => t.LeagueId == oldLeague.LeagueId).ToListAsync();
                        var oldClubTransfersReports = await _context.ClubTransferReports.Where(t => t.LeagueId == oldLeague.LeagueId).ToListAsync();
                        var oldClubPerformanceReports = await _context.ClubPerformanceReports.Where(t => t.LeagueId == oldLeague.LeagueId).ToListAsync();
                        var oldPlayerPerformanceReports = await _context.PlayerPerformanceReports.Where(t => t.LeagueId == oldLeague.LeagueId).ToListAsync();
/*                        var oldLives = await _context.Live.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();*/
/*                        var oldTopScores = await _context.TopScores.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();
                        var oldTopAssists = await _context.TopAssists.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();*/
/*                        var oldLiveGoals = await _context.LiveGoalHolders.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();
                        var oldSubstitutes = await _context.Substitutes.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();
                        var oldYellowCards = await _context.LiveYellowCardHolders.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();
                        var oldRedCards = await _context.LiveRedCardHolders.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();*/
/*                        var oldLineUps = await _context.LineUp.Where(t => t.Fixture.LeagueId == oldLeague.LeagueId).Include(t => t.Fixture).ToListAsync();
                        var oldLineUpXI = await _context.LineUpXI.Where(t => t.Fixture.LeagueId == oldLeague.LeagueId).Include(t => t.ClubPlayer).Include(t => t.Fixture).ToListAsync();
                        var oldLineUpSubs = await _context.LineUpSubstitutes.Where(t => t.Fixture.League.LeagueId == oldLeague.LeagueId).Include(t => t.ClubPlayer).Include(t => t.Fixture).ToListAsync();*/
/*                        var oldOwnGoals = await _context.LiveOwnGoalHolders.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).Include(t => t.Live).Include(t => t.OwnGoalScoredBy).ToListAsync();*/
/*                        var oldMatchOfficials = await _context.MatchOfficials.Where(t => t.Fixture.League.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).ToListAsync();*/
/*                        var oldLiveEvents = await _context.LiveEvents.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).Include(t => t.Live).Include(t => t.League).ToListAsync();
                        var oldPenalties = await _context.Penalties.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).Include(t => t.Live).ToListAsync();
                        var oldGoals= await _context.LiveGoals.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).Include(t => t.Live).Include(t => t.ScoreBy).Include(t => t.AssistedBy).ToListAsync();
                        var oldRed = await _context.RedCards.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId  ).Include(t => t.Live).Include(t => t.RedCommitedBy).ToListAsync(); 
                        var oldYellow = await _context.YellowCards.Where(t => t.LeagueId == oldLeague.LeagueId && t.DivisionId == divisionId).Include(t => t.Live).Include(t => t.YellowCommitedBy).ToListAsync();
*//*
                        foreach (var y in oldYellow)
                        {
                            var archivedYellow = new Yellow_Archive
                            {
                                EventId = y.EventId,
                                LiveId = y.LiveId,
                                LeagueId = y.LeagueId,
                                YellowCommitedById = y.YellowCommitedById,
                                YellowCardTime = y.YellowCardTime,
                                RecordedTime = y.RecordedTime,
                                YellowCardReason = y.YellowCardReason,
                                DivisionId = y.DivisionId
                            };
                            _context.Yellow_Archives.Add(archivedYellow);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var r in oldRed)
                        {
                            var archivedRed = new Red_Archive
                            {
                                EventId = r.EventId,
                                LiveId = r.LiveId,
                                LeagueId = r.LeagueId,
                                RedCommitedById = r.RedCommitedById,
                                RecordedTime = r.RecordedTime,
                                RedCardTime = r.RedCardTime,
                                RedCardReason = r.RedCardReason,
                                DivisionId = r.DivisionId
                            };
                            _context.Red_Archives.Add(archivedRed);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var g in oldGoals)
                        {
                            var archivedGoals = new Goals_Archive
                            {

                                EventId = g.EventId,
                                LiveId = g.LiveId,
                                LeagueId = g.LeagueId,
                                ScoreById = g.ScoreById,
                                AssistedById = g.AssistedById,
                                RecordedTime = g.RecordedTime,
                                ScoredTime = g.ScoredTime,
                                DivisionId = g.DivisionId

                            };
                            _context.Goals_Archives.Add(archivedGoals);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var p in oldPenalties)
                        {
                            var archivedPenalties = new Penalty_Archive
                            {
                                EventId = p.EventId,
                                LiveId = p.LiveId,
                                LeagueId = p.LeagueId,
                                PenaltyTime = p.PenaltyTime,
                                PlayerId = p.PlayerId,
                                RecordedTime = p.RecordedTime,
                                DivisionId = p.DivisionId
                            };
                            _context.Penalty_Archives.Add(archivedPenalties);
                            await _context.SaveChangesAsync();
                        }
*/
                        foreach (var s in oldStandings)
                        {
                            var archivedStanding = new Standings_Archive
                            {
                                StandingId = s.StandingId,
                                ClubId = s.ClubId,
                                Position = s.Position,
                                MatchPlayed = s.MatchPlayed,
                                Points = s.Points,
                                Wins = s.Wins,
                                Lose = s.Lose,
                                GoalsScored = s.GoalsScored,
                                GoalsConceded = s.GoalsConceded,
                                GoalDifference = s.GoalDifference,
                                Draw = s.Draw,
                                CreatedDateTime = s.CreatedDateTime,
                                ModifiedDateTime = s.ModifiedDateTime,
                                CreatedById = s.CreatedById,
                                ModifiedById = s.ModifiedById,
                                Last5Games = s.Last5Games,
                                LeagueId = s.LeagueId,
                                DivisionId = s.DivisionId
                            };
                            _context.Standings_Archive.Add(archivedStanding);
                            await _context.SaveChangesAsync();
                        }

                      /*  foreach (var m in oldMatchResults)
                        {
                            var archivedMatchResult = new MatchResults_Archive
                            {
                                ResultsId = m.ResultsId,
                                FixtureId = m.FixtureId,
                                AwayTeamId = m.AwayTeamId,
                                HomeTeamId = m.HomeTeamId,
                                HomeTeamScore = m.HomeTeamScore,
                                AwayTeamScore = m.AwayTeamScore,
                                MatchDate = m.MatchDate,
                                CreatedDateTime = m.CreatedDateTime,
                                ModifiedDateTime = m.ModifiedDateTime,
                                CreatedById = m.CreatedById,
                                ModifiedById = m.ModifiedById,
                                Location = m.Location,
                                LeagueId = m.LeagueId,
                                DivisionId = m.DivisionId
                            };
                            _context.MatchResults_Archive.Add(archivedMatchResult);
                            await _context.SaveChangesAsync();
                        }*/
/*
                        if (oldMatchFormations != null)
                        {
                            foreach (var m in oldMatchFormations)
                            {
                                var archivedMatchFormation = new MatchFormation_Archive
                                {
                                    MatchFormationId = m.MatchFormationId,
                                    ClubId = m.ClubId,
                                    FormationId = m.FormationId,
                                    FixtureId = m.FixtureId,
                                    CreatedDateTime = m.CreatedDateTime,
                                    CreatedById = m.CreatedById,
                                    ModifiedDateTime = m.ModifiedDateTime,
                                    ModifiedById = m.ModifiedById,
                                };
                                _context.MatchFormation_Archive.Add(archivedMatchFormation);
                                await _context.SaveChangesAsync();
                            }
                        }

                        foreach (var s in oldLineUpXI)
                        {
                            var archivedLineUpXI = new LineUpXI_Archive
                            {
                                LineUpXIId = s.LineUpXIId,
                                FixtureId = s.FixtureId,
                                CreatedDateTime = s.CreatedDateTime,
                                ModifiedDateTime = s.CreatedDateTime,
                                ClubId = s.ClubId,
                                CreatedById = s.CreatedById,
                                ModifiedById = s.ModifiedById,
                                PlayerId = s.ClubPlayer.Id
                                
                            };

                            _context.LineUpXI_Archives.Add(archivedLineUpXI);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var s in oldLineUpSubs)
                        {
                            var archivedLineUpSubs = new LineUpSubstitutes_Archive
                            {
                                LineUpSubstituteId = s.LineUpSubstituteId,
                                FixtureId = s.FixtureId,
                                CreatedDateTime = s.CreatedDateTime,
                                ModifiedDateTime = s.CreatedDateTime,
                                ClubId = s.ClubId,
                                CreatedById = s.CreatedById,
                                ModifiedById = s.ModifiedById,
                                PlayerId = s.ClubPlayer.Id,

                            };

                            _context.LineUpSubstitutes_Archives.Add(archivedLineUpSubs);
                            await _context.SaveChangesAsync();
                        }
*/
                       /* foreach (var r in oldOwnGoals)
                        {
                            var archivedOwnGoals = new OwnGoals_Archive
                            {
                                EventId = r.EventId,
                                LeagueId = r.LeagueId,
                                RecordedTime = r.RecordedTime,
                                OwnGoalScoredById = r.OwnGoalScoredById,
                                OwnGoalTime = r.OwnGoalTime,
                                LiveId = r.LiveId,
                                DivisionId = r.DivisionId
                            };

                            _context.OwnGoals_Archives.Add(archivedOwnGoals);
                            await _context.SaveChangesAsync();
                        }*/
/*
                        foreach (var s in oldLineUps)
                        {
                            var archivedLineUpArchives = new LineUps_Archive
                            {
                                LineUpId = s.LineUpId,
                                FixtureId = s.FixtureId,
                                CreatedDateTime = s.CreatedDateTime,
                                ModifiedDateTime = s.CreatedDateTime,
                                LineUpSubstitutes = s.LineUpSubstitutes,
                                ClubId = s.ClubId,
                                CreatedById = s.CreatedById,
                                LineUpXI = s.LineUpXI,
                                ModifiedById = s.ModifiedById
                            };

                            _context.LineUps_Archives.Add(archivedLineUpArchives);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var m in oldMatchOfficials)
                        {
                            var archivedMatchOfficials = new MatchOfficials_Archive
                            {
                                MatchOfficialsId = m.MatchOfficialsId,
                                FixtureId = m.FixtureId,
                                RefeereId = m.RefeereId,
                                AssistantOneId = m.AssistantOneId,
                                AssistantTwoId = m.AssistantTwoId,
                                DivisionId = m.DivisionId

                            };

                            _context.MatchOfficials_Archives.Add(archivedMatchOfficials);
                            await _context.SaveChangesAsync();
                        }*/

                        foreach (var c in oldClubs)
                        {
                            var existingClub = await _context.Club.FindAsync(c.ClubId);

                            if (existingClub != null)
                            {
                                existingClub.IsActive = false;
                                existingClub.Status = ClubStatus.Previous_Season;

                                _context.Update(existingClub);
                                await _context.SaveChangesAsync();
                            }
                        }

                        foreach (var m in oldMatchReports)
                        {
                            var archivedMatchReports = new MatchReports_Archive
                            {
                                ReportId = m.ReportId,
                                LeagueId = m.LeagueId,
                                MatchesToBePlayedCount = m.MatchesToBePlayedCount,
                                PlayedMatchesCounts = m.PlayedMatchesCounts,
                                InterruptedMatchesCount = m.InterruptedMatchesCount,
                                PostponedMatchesCount = m.PostponedMatchesCount,
                                FixturedMatchesCount = m.FixturedMatchesCount,
                                FixturedMatchesRate = m.FixturedMatchesRate,
                                UnfixturedMatchesRate = m.UnfixturedMatchesRate,
                                InterruptedMatchesRate = m.InterruptedMatchesRate,
                                PlayedMatchesRate = m.PlayedMatchesRate,
                                PostponedMatchesRate = m.PostponedMatchesRate,
                                UnreleasedFixturesCount = m.UnreleasedFixturesCount,
                                DivisionId = m.DivisionId
                            };

                            _context.MatchReportsArchive.Add(archivedMatchReports);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var r in oldMatchResultsReports)
                        {
                            var archivedMatchResultsReports = new MatchResultsReports_Archive
                            {
                                ReportId = r.ReportId,
                                LeagueId = r.LeagueId,
                                ExpectedResultsCount = r.ExpectedResultsCount,
                                ReleasedResultsCount = r.ReleasedResultsCount,
                                WinsCount = r.WinsCount,
                                LosesCount = r.LosesCount,
                                DrawsCount = r.DrawsCount,
                                ReleasedResultsRate = r.ReleasedResultsRate,
                                UnreleasedMatchesRate = r.UnreleasedMatchesRate,
                                WinningRate = r.WinningRate,
                                LosingRate = r.LosingRate,
                                DrawingRate = r.DrawingRate,
                                UnreleasedResultsCount = r.UnreleasedResultsCount,
                                DivisionId = r.DivisionId
                            };

                            _context.MatchResultsReports_Archive.Add(archivedMatchResultsReports);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var t in oldTransfersReports)
                        {
                            var archivedTransfersReports = new TransfersReports_Archive
                            {
                                ReportId = t.ReportId,
                                LeagueId = t.LeagueId,
                                TransferPeriodId = t.TransferPeriodId,
                                TransferMarketCount = t.TransferMarketCount,
                                PurchasedPlayersCount = t.PurchasedPlayersCount,
                                DeclinedTransfersCount = t.DeclinedTransfersCount,
                                TranferAmount = t.TranferAmount,
                                ClubsCut = t.ClubsCut,
                                AssociationCut = t.AssociationCut,
                                SuccessfulTranferRate = t.SuccessfulTranferRate,
                                UnsuccessfulTranferRate = t.UnsuccessfulTranferRate,
                                NotStartedTransferRate = t.NotStartedTransferRate,
                                DivisionId = t.DivisionId
                            };

                            _context.TransfersReports_Archive.Add(archivedTransfersReports);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var c in oldClubTransfersReports)
                        {
                            var archivedClubTransfersReports = new ClubTransferReports_Archive
                            {
                                ReportId = c.ReportId,
                                ClubId = c.ClubId,
                                LeagueId = c.LeagueId,
                                OverallTransfersCount = c.OverallTransfersCount,
                                IncomingTransfersCount = c.IncomingTransfersCount,
                                OutgoingTransfersCount = c.OutgoingTransfersCount,
                                SuccessfulIncomingTransfersCount = c.SuccessfulIncomingTransfersCount,
                                SuccessfulOutgoingTransfersCount = c.SuccessfulOutgoingTransfersCount,
                                RejectedIncomingTransfersCount = c.RejectedIncomingTransfersCount,
                                RejectedOutgoingTransfersCount = c.RejectedOutgoingTransfersCount,
                                IncomingTransferRate = c.IncomingTransferRate,
                                OutgoingTransferRate = c.OutgoingTransferRate,
                                SuccessfullIncomingTransferRate = c.SuccessfullIncomingTransferRate,
                                SuccessfullOutgoingTransferRate = c.SuccessfullOutgoingTransferRate,
                                RejectedIncomingTransferRate = c.RejectedIncomingTransferRate,
                                RejectedOutgoingTransferRate = c.RejectedOutgoingTransferRate,
                                DivisionId = c.DivisionId
                            };

                            _context.ClubTransferReports_Archive.Add(archivedClubTransfersReports);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var c in oldPlayerPerformanceReports)
                        {
                            var archivedPlayerPerformanceReports = new PlayerPerformanceReports_Archive
                            {
                                ReportId = c.ReportId,
                                PlayerId = c.PlayerId,
                                LeagueId = c.LeagueId,
                                AppearancesCount = c.AppearancesCount,
                                GoalsScoredCount = c.GoalsScoredCount,
                                AssistsCount = c.AssistsCount,
                                YellowCardCount = c.YellowCardCount,
                                RedCardCount = c.RedCardCount,
                                DivisionId = c.DivisionId,
                            };

                            _context.PlayerPerformanceReports_Archives.Add(archivedPlayerPerformanceReports);
                            await _context.SaveChangesAsync();
                        }
/*
                        foreach (var l in oldLiveEvents)
                        {
                            var archivedLiveEvents = new LiveEvents_Archives
                            {
                                EventId = l.EventId,
                                LiveId = l.Live.LiveId,
                                LeagueId= l.LeagueId,
                                RecordedTime = l.RecordedTime,
                                DivisionId = l.DivisionId
                            };

                            _context.LiveEvents_Archives.Add(archivedLiveEvents);
                            await _context.SaveChangesAsync();
                        }*/

                        foreach (var c in oldClubPerformanceReports)
                        {
                            var archivedClubPerformanceReports = new ClubPerformanceReports_Archive
                            {
                                ReportId = c.ReportId,
                                ClubId = c.ClubId,
                                LeagueId = c.LeagueId,
                                GamesToPlayCount = c.GamesToPlayCount,
                                GamesPlayedCount = c.GamesPlayedCount,
                                GamesNotPlayedCount = c.GamesNotPlayedCount,
                                GamesWinCount = c.GamesWinCount,
                                GamesLoseCount = c.GamesLoseCount,
                                GamesDrawCount = c.GamesDrawCount,
                                GamesPlayedRate = c.GamesPlayedRate,
                                GamesNotPlayedRate = c.GamesNotPlayedRate,
                                GamesWinRate = c.GamesWinRate,
                                GamesLoseRate = c.GamesLoseRate,
                                GamesDrawRate = c.GamesDrawRate,
                                DivisionId = c.DivisionId
                            };

                            _context.ClubPerformanceReports_Archive.Add(archivedClubPerformanceReports);
                            await _context.SaveChangesAsync();
                        }
/*
                        foreach (var l in oldTopScores)
                        {
                            var archivedTopScores = new TopScores_Archive
                            {
                                TopScoreId = l.TopScoreId,
                                LeagueId = l.LeagueId,
                                PlayerId = l.PlayerId,
                                NumberOfGoals = l.NumberOfGoals,
                                DivisionId = l.DivisionId
                            };

                            _context.TopScores_Archives.Add(archivedTopScores);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var l in oldTopAssists)
                        {
                            var archivedTopAssists = new TopAssists_Archive
                            {
                                TopAssistId = l.TopAssistId,
                                LeagueId = l.LeagueId,
                                PlayerId = l.PlayerId,
                                NumberOfAssists = l.NumberOfAssists,
                                DivisionId = l.DivisionId
                            };

                            _context.TopAssists_Archives.Add(archivedTopAssists);
                            await _context.SaveChangesAsync();
                        }*/

/*
                        foreach (var g in oldLiveGoals)
                        {
                            var archivedLiveGoals = new LiveGoals_Archive
                            {
                                ScoredById = g.ScoredById,
                                LeagueId = g.LeagueId,
                                EventId = g.EventId,
                                RecordedTime = g.RecordedTime,
                                AssistedById = g.AssistedById,
                                ScoredTime = g.ScoredTime,
                                LiveId = g.LiveId,
                                DivisionId = g.DivisionId
                            };

                            _context.LiveGoals_Archives.Add(archivedLiveGoals);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var s in oldSubstitutes)
                        {
                            var archivedSubstitutes = new Substitute_Archive
                            {
                                EventId = s.EventId,
                                LiveId = s.LiveId,
                                InPlayerId = s.InPlayerId,
                                OutPlayerId = s.OutPlayerId,
                                RecordedTime = s.RecordedTime,
                                LeagueId = s.LeagueId,
                                SubTime = s.SubTime,
                                DivisionId = s.DivisionId
                            };

                            _context.Substitute_Archives.Add(archivedSubstitutes);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var y in oldYellowCards)
                        {
                            var archivedYellowCards = new LiveYellowCard_Archive
                            {
                                EventId = y.EventId,
                                LeagueId = y.LeagueId,
                                YellowCardTime = y.YellowCommitedById,
                                RecordedTime = y.RecordedTime,
                                LiveId = y.LiveId,
                                YellowCommitedById = y.YellowCommitedById,
                                DivisionId = y.DivisionId
                            };

                            _context.YellowCard_Archives.Add(archivedYellowCards);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var r in oldRedCards)
                        {
                            var archivedRedCards = new LiveRedCard_Archive
                            {
                                EventId = r.EventId,
                                LeagueId = r.LeagueId,
                                RedCardTime = r.RedCardTime,
                                RecordedTime = r.RecordedTime,
                                LiveId = r.LiveId,
                                RedCommitedById= r.RedCommitedById,
                                DivisionId = r.DivisionId
                            };

                            _context.LiveRedCard_Archives.Add(archivedRedCards);
                            await _context.SaveChangesAsync();
                        }

                        foreach (var l in oldLives)
                        {
                            var archivedLives = new Live_Archive
                            {
                                LiveId = l.LiveId,
                                LeagueId = l.LeagueId,
                                FixtureId = l.FixtureId,
                                HomeTeamScore = l.HomeTeamScore,
                                AwayTeamScore = l.AwayTeamScore,
                                AddedTime = l.AddedTime,
                                ISEnded = l.ISEnded,
                                IsHalfTime = l.IsHalfTime,
                                IsLive = l.IsLive,
                                LiveTime = l.LiveTime,
                                WentToHalfTime = l.WentToHalfTime,
                                IsInterrupted = l.IsInterrupted,
                                ReasonForInterruption = l.ReasonForInterruption,
                                LiveStatus = l.LiveStatus,
                                HalfTimeScore = l.HalfTimeScore,
                                RecordedTime = l.RecordedTime,
                                DivisionId = l.DivisionId
                            };

                            _context.Live_Archives.Add(archivedLives);
                            await _context.SaveChangesAsync();
                        }*/

                      /*  foreach (var f in oldFixtures)
                        {
                            var archivedFixture = new Fixtures_Archive
                            {
                                FixtureId = f.FixtureId,
                                HomeTeamId = f.HomeTeamId,
                                AwayTeamId = f.AwayTeamId,
                                KickOffDate = f.KickOffDate,
                                KickOffTime = f.KickOffTime,
                                Location = f.Location,
                                CreatedDateTime = f.CreatedDateTime,
                                ModifiedDateTime = f.ModifiedDateTime,
                                CreatedById = f.CreatedById,
                                ModifiedById = f.ModifiedById,
                                FixtureStatus = f.FixtureStatus,
                                LeagueId = f.LeagueId,
                                DivisionId = f.DivisionId
                            };
                            _context.Fixtures_Archive.Add(archivedFixture);
                            await _context.SaveChangesAsync();
                        }*/

/*                        _context.RedCards.RemoveRange(oldRed);
                        _context.LiveGoals.RemoveRange(oldGoals);
                        _context.Penalties.RemoveRange(oldPenalties);
                        _context.Penalties.RemoveRange(oldPenalties);*/
                        _context.Standing.RemoveRange(oldStandings);
/*                        _context.MatchResult.RemoveRange(oldMatchResults);*/
/*                        _context.MatchFormation.RemoveRange(oldMatchFormations);
*//*                        _context.LiveOwnGoalHolders.RemoveRange(oldOwnGoals);*//*
                        _context.LineUpXI.RemoveRange(oldLineUpXI);
                        _context.LineUpSubstitutes.RemoveRange(oldLineUpSubs);
                        _context.LineUp.RemoveRange(oldLineUps);
                        _context.MatchOfficials.RemoveRange(oldMatchOfficials);*/
                        _context.MatchReports.RemoveRange(oldMatchReports);
                        _context.MatchResultsReports.RemoveRange(oldMatchResultsReports);
                        _context.TransfersReports.RemoveRange(oldTransfersReports);
                        _context.ClubTransferReports.RemoveRange(oldClubTransfersReports);
                        _context.PlayerPerformanceReports.RemoveRange(oldPlayerPerformanceReports);
/*                        _context.LiveEvents.RemoveRange(oldLiveEvents);*/
                        _context.ClubPerformanceReports.RemoveRange(oldClubPerformanceReports);
/*                        _context.TopScores.RemoveRange(oldTopScores);
                        _context.TopAssists.RemoveRange(oldTopAssists);*/
/*                        _context.LiveGoalHolders.RemoveRange(oldLiveGoals);
                        _context.Substitutes.RemoveRange(oldSubstitutes);
                        _context.LiveYellowCardHolders.RemoveRange(oldYellowCards);
                        _context.LiveRedCardHolders.RemoveRange(oldRedCards);
                        _context.YellowCards.RemoveRange(oldYellow);*/

                        await _context.SaveChangesAsync();

/*                        _context.Fixture.RemoveRange(oldFixtures);*/
/*                        _context.Live.RemoveRange(oldLives);
*/
                        await _context.SaveChangesAsync();

                    }

                    var newLeague = new League
                    {
                        LeagueYears = viewModel.LeagueYears,
                        CreatedById = userId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedById = userId,
                        ModifiedDateTime = DateTime.Now,
                        IsCurrent = true,
                        LeagueCode = GenerateLeagueCode(),
                        DivisionId = divisionId
                    };

                    _context.Add(newLeague);
                    await _context.SaveChangesAsync();

                    var newSeason = await _context.League
                           .Where(n => n.Equals(newLeague)).FirstOrDefaultAsync();

                    var transferPeriod = new TransferPeriod
                    {
                        LeagueId = newSeason.LeagueId,
                        PeriodOpenCount = 0,
                        IsOpened = false,
                        CreatedDateTime = DateTime.Now,
                        CreatedById = user.Id,
                        ModifiedDateTime = DateTime.Now,
                        ModifiedById = user.Id,
                        IsCurrent = true,
                        DivisionId = divisionId
                    };

                    _context.Add(transferPeriod);
                    await _context.SaveChangesAsync();


                    var matchReports = new MatchReports
                    {
                        LeagueId = newSeason.LeagueId,
                        MatchesToBePlayedCount = 0,
                        FixturedMatchesCount = 0,
                        UnreleasedFixturesCount = 0,
                        InterruptedMatchesCount = 0,
                        FixturedMatchesRate = 0,
                        UnfixturedMatchesRate = 0,
                        PlayedMatchesRate = 0,
                        InterruptedMatchesRate = 0,
                        PostponedMatchesRate = 0,
                        PlayedMatchesCounts = 0,
                        PostponedMatchesCount = 0,
                        DivisionId = divisionId
                    };

                    _context.Add(matchReports);
                    await _context.SaveChangesAsync();

                    var newTransferReports = new TransfersReports
                    {
                        LeagueId = newSeason.LeagueId,
                        TransferMarketCount = 0,
                        PurchasedPlayersCount = 0,
                        DeclinedTransfersCount = 0,
                        TransferPeriodId = transferPeriod.TransferPeriodId,
                        TranferAmount = 0,
                        AssociationCut = 0,
                        ClubsCut = 0,
                        SuccessfulTranferRate = 0,
                        UnsuccessfulTranferRate = 0,
                        NotStartedTransferRate = 0,
                        DivisionId = divisionId

                    };

                    _context.Add(newTransferReports);
                    await _context.SaveChangesAsync();

                    var newMatchResultsReports = new MatchResultsReports
                    {
                        LeagueId = newSeason.LeagueId,
                        ExpectedResultsCount = 0,
                        ReleasedResultsCount = 0,
                        UnreleasedResultsCount = 0,
                        WinsCount = 0,
                        LosesCount = 0,
                        DrawsCount = 0,
                        ReleasedResultsRate = 0,
                        UnreleasedMatchesRate = 0,
                        WinningRate = 0,
                        LosingRate = 0,
                        DrawingRate = 0,
                        DivisionId = divisionId
                    };

                    _context.Add(newMatchResultsReports);
                    await _context.SaveChangesAsync();

                    var allPlayers = await _context.Player
                        .Where(a => !a.IsDeleted && 
                        a.Club.DivisionId == divisionId)
                        .ToListAsync();

                    var newPlayerPerformanceReports = new List<PlayerPerformanceReport>();

                    var newPlayerScores = new List<TopScore>();

                    var newPlayerAssists = new List<TopAssist>();

                    foreach (var p in allPlayers)
                    {
                        var newPlayerPerformanceReport = new PlayerPerformanceReport
                        {
                            LeagueId = newSeason.LeagueId,
                            PlayerId = p.Id,
                            AppearancesCount = 0,
                            GoalsScoredCount = 0,
                            AssistsCount = 0,
                            YellowCardCount = 0,
                            RedCardCount = 0,
                            OwnGoalsScoredCount = 0,
                            DivisionId = divisionId
                        };

                        newPlayerPerformanceReports.Add(newPlayerPerformanceReport);

                        var newPlayerScore = new TopScore
                        {
                            PlayerId = p.Id,
                            LeagueId = newLeague.LeagueId,
                            NumberOfGoals = 0,
                            DivisionId = divisionId
                        };

                        newPlayerScores.Add(newPlayerScore);

                        var newPlayerAssist = new TopAssist
                        {
                            PlayerId = p.Id,
                            LeagueId = newLeague.LeagueId,
                            NumberOfAssists = 0,
                            DivisionId = divisionId
                        };

                        newPlayerAssists.Add(newPlayerAssist);
                    }

                    

                    _context.PlayerPerformanceReports.AddRange(newPlayerPerformanceReports);
                    _context.TopScores.AddRange(newPlayerScores);
                    _context.TopAssists.AddRange(newPlayerAssists);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = $"{newLeague.LeagueYears} has been started successfully and all running data will be related to it. This league will be referred to as the current league.";

                    await _requestLogService.LogSuceededRequest("New season started successfully.", StatusCodes.Status200OK);

                    await _activityLogger.Log($"Started season {newSeason.LeagueYears}", user.Id);
                    return RedirectToAction(nameof(Leagues));
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to start a new league: " + ex.Message,
                    errorDetails = new
                    {
                        InnerException = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
           
            return View(viewModel);
        }

        [Authorize(Roles = ("Division Manager"))]
        private string GenerateLeagueCode()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{year}{month}{day}{randomString}";
        }

    }
}
