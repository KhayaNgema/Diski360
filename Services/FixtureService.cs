using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyField.Services
{
    public class FixtureService
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly HashSet<int> _scheduledClubsForWeekend;
        private readonly HashSet<(int, int)> _previouslyScheduledPairs;

        public FixtureService(Ksans_SportsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _scheduledClubsForWeekend = new HashSet<int>();
            _previouslyScheduledPairs = new HashSet<(int, int)>();
        }

        public async Task ScheduleFixturesAsync()
        {
            var clubs = await _context.Club
                .Where(c => c.League.IsCurrent)
                .ToListAsync();

            if (clubs == null || clubs.Count < 8)
            {
                throw new InvalidOperationException("Not enough clubs available.");
            }

            DateTime today = DateTime.Today;
            int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            DateTime nextSaturday = today.AddDays(daysUntilSaturday);

            await LoadPreviouslyScheduledPairs();
            await ScheduleFixturesForDay(nextSaturday, clubs);
            await ScheduleFixturesForDay(nextSaturday.AddDays(1), clubs);
        }

        private async Task LoadPreviouslyScheduledPairs()
        {
            var previousFixtures = await _context.Fixture.ToListAsync();
            foreach (var fixture in previousFixtures)
            {
                _previouslyScheduledPairs.Add((fixture.HomeTeamId, fixture.AwayTeamId));
                _previouslyScheduledPairs.Add((fixture.AwayTeamId, fixture.HomeTeamId));
            }
        }

        private async Task ScheduleFixturesForDay(DateTime day, List<Club> clubs)
        {
            const int fixturesPerDay = 4;
            TimeSpan startTime = new TimeSpan(9, 0, 0); 
            TimeSpan interval = new TimeSpan(2, 0, 0); 

            var scheduledMatches = new HashSet<(int, int)>();
            var scheduledClubsForDay = new HashSet<int>();

            var existingFixtures = await _context.Fixture
                .Where(f => f.KickOffDate == day.Date)
                .ToListAsync();

            foreach (var fixture in existingFixtures)
            {
                scheduledMatches.Add((fixture.HomeTeamId, fixture.AwayTeamId));
                scheduledMatches.Add((fixture.AwayTeamId, fixture.HomeTeamId));
                scheduledClubsForDay.Add(fixture.HomeTeamId);
                scheduledClubsForDay.Add(fixture.AwayTeamId);
                _scheduledClubsForWeekend.Add(fixture.HomeTeamId);
                _scheduledClubsForWeekend.Add(fixture.AwayTeamId);
            }

            Random random = new Random();
            clubs = clubs.OrderBy(x => random.Next()).ToList();

            var currentSeason = await _context.League
                .Where(c => c.IsCurrent)
                .FirstOrDefaultAsync();

            if (currentSeason == null)
            {
                throw new InvalidOperationException("No current season found.");
            }

            int matchCount = 0;

            foreach (var homeTeam in clubs)
            {
                if (matchCount >= fixturesPerDay)
                {
                    break; 
                }

                var availableAwayTeams = clubs.Where(awayTeam =>
                    homeTeam.ClubId != awayTeam.ClubId &&
                    !scheduledClubsForDay.Contains(homeTeam.ClubId) &&
                    !scheduledClubsForDay.Contains(awayTeam.ClubId) &&
                    !_scheduledClubsForWeekend.Contains(homeTeam.ClubId) &&
                    !_scheduledClubsForWeekend.Contains(awayTeam.ClubId) &&
                    !scheduledMatches.Contains((homeTeam.ClubId, awayTeam.ClubId)) &&
                    !scheduledMatches.Contains((awayTeam.ClubId, homeTeam.ClubId)) &&
                    !_previouslyScheduledPairs.Contains((homeTeam.ClubId, awayTeam.ClubId)) &&
                    !_previouslyScheduledPairs.Contains((awayTeam.ClubId, homeTeam.ClubId))
                ).ToList();

                if (availableAwayTeams.Any())
                {
                    var awayTeam = availableAwayTeams.First();

                    var kickOffTime = day.Date + startTime.Add(interval * matchCount);

                    var newFixture = new Fixture
                    {
                        HomeTeamId = homeTeam.ClubId,
                        AwayTeamId = awayTeam.ClubId,
                        KickOffDate = kickOffTime.Date,
                        KickOffTime = kickOffTime,
                        Location = "Nkungwini Sports Ground",
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        CreatedById = "ddfc5eee-93b6-4728-916f-e0db8a5e4e9d",
                        ModifiedById = "ddfc5eee-93b6-4728-916f-e0db8a5e4e9d",
                        FixtureStatus = FixtureStatus.Upcoming,
                        LeagueId = currentSeason.LeagueId
                    };

                    _context.Fixture.Add(newFixture);
                    scheduledMatches.Add((homeTeam.ClubId, awayTeam.ClubId));
                    scheduledMatches.Add((awayTeam.ClubId, homeTeam.ClubId));
                    scheduledClubsForDay.Add(homeTeam.ClubId);
                    scheduledClubsForDay.Add(awayTeam.ClubId);
                    _scheduledClubsForWeekend.Add(homeTeam.ClubId);
                    _scheduledClubsForWeekend.Add(awayTeam.ClubId);
                    _previouslyScheduledPairs.Add((homeTeam.ClubId, awayTeam.ClubId));
                    _previouslyScheduledPairs.Add((awayTeam.ClubId, homeTeam.ClubId));

                    matchCount++;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
