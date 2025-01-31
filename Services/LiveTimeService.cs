using MyField.Data;

namespace MyField.Services
{
    public class LiveTimeService
    {
        private readonly Ksans_SportsDbContext _context;

        public LiveTimeService(Ksans_SportsDbContext context)
        {
            _context = context;
        }

        public async Task UpdateLiveTime(int liveId)
        {
            var live = await _context.Live.FindAsync(liveId);
            if (live == null || !live.IsLive) return;

            live.LiveTime++;
            _context.Update(live);
            await _context.SaveChangesAsync();
        }
    }

}
