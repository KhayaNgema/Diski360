using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TournamentFixture
    {
        [Key]
        public int FixtureId { get; set; }

        public int HomeTeamClubId { get; set; }
        public int HomeTeamTournamentId { get; set; }

        [ForeignKey(nameof(HomeTeamClubId) + "," + nameof(HomeTeamTournamentId))]
        public TournamentClubs HomeTeam { get; set; }

        public int AwayTeamClubId { get; set; }
        public int AwayTeamTournamentId { get; set; }

        [ForeignKey(nameof(AwayTeamClubId) + "," + nameof(AwayTeamTournamentId))]
        public TournamentClubs AwayTeam { get; set; }

        public DateTime? KickOffDate { get; set; }
        public DateTime? KickOffTime { get; set; }
        public string? Location { get; set; }
        public FixtureRound FixtureRound { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }
        [ForeignKey("ModifiedById")]
        public UserBaseModel ModifiedBy { get; set; }

        public FixtureStatus FixtureStatus { get; set; }
        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public Tournament Tournament { get; set; }
    }


    public enum FixtureRound
    { 
        Round_1,
        Round_2,
        Round_3,
        Round_4,
        Round_5,
        Round_6,
        Round_7,
        Round_8,
        Round_9,
        Round_10,
        Quater_Final,
        Semi_Final,
        Final
    }

}
