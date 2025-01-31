using MyField.Models;

namespace MyField.ViewModels
{
    public class StartLiveViewModel
    {
        public int LiveId { get; set; }
        public int FixtureId { get; set; }
        public string FixturedClubs { get; set; }

        public DateTime KickOffDate { get; set; }

        public DateTime KickOffTime { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }

        public string HomeTeamName { get; set; }

        public string AwayTeamName { get; set; }

        public int HomeTeamScore { get; set; }

        public int AwayTeamScore { get; set; }

        public string HomeTeamBadge { get; set; }

        public string AwayTeamBadge { get; set;}

        public int LiveTime { get; set; }

        public bool IsLive { get; set; }

        public bool IsHalfTime { get; set; }

        public bool IsEnded { get; set; }

        public LiveStatus LiveStatus { get; set; }
    }
}
