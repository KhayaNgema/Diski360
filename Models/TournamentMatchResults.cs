using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TournamentMatchResults
    {
        [Key]
        public int ResultsId { get; set; }

        public int FixtureId { get; set; }
        [ForeignKey("FixtureId")]
        public TournamentFixture TournamentFixture { get; set; }
        public int HomeTeamClubId { get; set; }
        public int HomeTeamTournamentId { get; set; }

        [ForeignKey(nameof(HomeTeamClubId) + "," + nameof(HomeTeamTournamentId))]
        public TournamentClubs HomeTeam { get; set; }

        public int AwayTeamClubId { get; set; }
        public int AwayTeamTournamentId { get; set; }

        [ForeignKey(nameof(AwayTeamClubId) + "," + nameof(AwayTeamTournamentId))]
        public TournamentClubs AwayTeam { get; set; }

        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

        public DateTime MatchDate { get; set; }
        public DateTime MatchTime { get; set; }
        public string Location { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }
        [ForeignKey("ModifiedById")]
        public UserBaseModel ModifiedBy { get; set; }

        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; }
    }

}
