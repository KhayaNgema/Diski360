using Hangfire;
using Microsoft.EntityFrameworkCore;
using MyField.Data;
using System;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using MyField.Models;

public class SubscriptionCheckerService
{
    private readonly Ksans_SportsDbContext _context;

    public SubscriptionCheckerService(Ksans_SportsDbContext context)
    {
        _context = context;
    }

    [AutomaticRetry(Attempts = 3)] 
    public async Task CheckExpiredSubscriptions()
    {
        var expiredSubscriptions = await _context.Subscriptions
            .Where(s => s.SubscriptionStatus == SubscriptionStatus.Active &&
                        s.ExpirationDate <= DateTime.Now &&
                        (s.SubscriptionPlan == SubscriptionPlan.Premium || 
                        s.SubscriptionPlan == SubscriptionPlan.Club_Premium))
            .ToListAsync();


        foreach (var subscription in expiredSubscriptions)
        {
            var existingSubscriptionHistory = await _context.SubscriptionHistories
                .Where(s => s.SubscriptionId == subscription.SubscriptionId &&
                s.Status == SubscriptionStatus.Active)
                .FirstOrDefaultAsync();

            if(subscription != null && existingSubscriptionHistory != null)
            {
                subscription.SubscriptionStatus = SubscriptionStatus.Expired;
                subscription.SubscriptionPlan = SubscriptionPlan.Basic;
                existingSubscriptionHistory.Status = SubscriptionStatus.Expired;

                _context.Update(subscription);
                _context.Update(existingSubscriptionHistory);
                await _context.SaveChangesAsync();
            }

        }

    }
}
