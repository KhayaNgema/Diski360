using Microsoft.EntityFrameworkCore;
using MyField.Data;
using Polly;
using Polly.Retry;
using System;
using System.Threading.Tasks;

public class RetrySavingService
{
    private static readonly AsyncRetryPolicy<bool> RetryPolicy = Policy<bool>
        .Handle<DbUpdateConcurrencyException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, timeSpan, retryCount, context) =>
            {
                
            });

    private readonly Ksans_SportsDbContext _context;

    public RetrySavingService(Ksans_SportsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TrySaveChangesWithRetryAsync()
    {
        return await RetryPolicy.ExecuteAsync(async () => await _context.SaveChangesAsync() > 0);
    }
}