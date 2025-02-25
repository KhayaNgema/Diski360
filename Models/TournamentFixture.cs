using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TournamentFixture
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FixtureId { get; set; }

        [Display(Name = "Home Team")]
        [Required(ErrorMessage = "Home Team is required")]
        [ForeignKey("HomeTeamId")]
        public int HomeTeamId { get; set; }
        public TournamentClub HomeTeam { get; set; }

        [Display(Name = "Away Team")]
        [Required(ErrorMessage = "Away Team is required")]
        [ForeignKey("AwayTeamId")]
        public int AwayTeamId { get; set; }
        public TournamentClub AwayTeam { get; set; }

        [DataType(DataType.Date)]
        public DateTime? KickOffDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime? KickOffTime { get; set; }

        [Display(Name = "Location")]
        [Required(ErrorMessage = "Match location is required")]
        public string? Location { get; set; }

        public FixtureRound FixtureRound { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }


        public FixtureStatus FixtureStatus { get; set; }

        [Display(Name = "League")]
        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }

        public TournamentFixture()
        {
            KickOffDate = DateTime.MinValue;
        }
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
