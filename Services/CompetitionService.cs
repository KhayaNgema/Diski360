using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using MyField.Models;

namespace MyField.Services
{
    public class CompetitionService
    {
        private readonly Ksans_SportsDbContext _context;
        private readonly EmailService _emailService;

        public CompetitionService(Ksans_SportsDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task EndCurrentCompetitionAndStartNewOne()
        {
            try
            {
                var currentCompetition = await _context.Competition
                    .Where(c => c.CompetitionStatus == CompetitionStatus.Current)
                    .OrderByDescending(c => c.Month)
                    .Include(c => c.Winner)
                    .FirstOrDefaultAsync();

                if (currentCompetition != null)
                {
                    var competitionWinner = await _context.CompetitionParticipants
                        .Where(cw => cw.CompetitionId == currentCompetition.CompetitionId)
                        .OrderByDescending(cw => cw.Points)
                        .Include(cw => cw.Participant)
                        .FirstOrDefaultAsync();

if (competitionWinner != null && competitionWinner.Points != 0)
{
    currentCompetition.WinnerId = competitionWinner.Participant.Id;

    _context.Update(competitionWinner);
    await _context.SaveChangesAsync();

    string winnerEmailBody = $@"
        Dear {competitionWinner.Participant.FirstName} {competitionWinner.Participant.LastName},<br/><br/>
        We are thrilled to announce that you have been selected as the winner of our <b>Most Engaging User for {currentCompetition.Month.ToString("MMMM")} Competition</b>! Your exceptional participation and enthusiasm have truly stood out, and we couldn’t be more excited to reward you for your efforts.<br/><br/>
        <b>You have won a cash prize of R1000!</b> 🎉💵<br/><br/>
        Our team will be in touch with you within the next <b>24 hours</b> to provide details on how you will receive your prize. In the meantime, if you have any questions, feel free to reach out to us.<br/><br/>
        Thank you for being an integral part of our community. Your engagement continues to inspire us!<br/><br/>
        Warm regards,<br/>
        The Diski360 Team
    ";

    BackgroundJob.Enqueue<EmailService>(service =>
        service.SendEmailAsync(competitionWinner.Participant.Email, "Congratulations! You’re Our Most Engaging User!", winnerEmailBody, "Diski360"));
}

                    else
                    {
                        
                    }

                    currentCompetition.CompetitionStatus = CompetitionStatus.Ended;

                    _context.Update(currentCompetition);
                    await _context.SaveChangesAsync();
                }

                var newCompetition = new Competition
                {
                    Month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    CompetitionStatus = CompetitionStatus.Current,
                    NumberOfParticipants = 0,
                };

                _context.Competition.Add(newCompetition);
                await _context.SaveChangesAsync();

                var newSavedCompetition = await _context.Competition
                    .Where(nc => nc.Equals(newCompetition))
                    .FirstOrDefaultAsync();

                var activeSubscribers = await _context.Subscriptions
                    .Where(ac => ac.SubscriptionStatus == SubscriptionStatus.Active &&
                    ac.SubscriptionPlan == SubscriptionPlan.Premium)
                    .Include(ac => ac.SystemUser)
                    .ToListAsync();

                foreach (var subscriber in activeSubscribers)
                {
                    var activeParticipant = new CompetitionParticipants
                    {
                        UserId = subscriber.UserId,
                        CompetitionId = newSavedCompetition.CompetitionId,
                        Points = 0
                    };

                    _context.CompetitionParticipants.Add(activeParticipant);
                    await _context.SaveChangesAsync();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EndCurrentCompetitionAndStartNewOne: {ex.Message}");
            }
        }

        public async Task ScheduleEndOfMonthCompetitionCheck()
        {
            if (DateTime.Now.Day == DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
            {
                await EndCurrentCompetitionAndStartNewOne();
            }
        }
    }
}
