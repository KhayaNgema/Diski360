using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TournamentMatchResults
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResultsId { get; set; }

        public int FixtureId { get; set; }

        public virtual TournamentFixture TournamentFixture { get; set; }

        [ForeignKey("AwayTeamId")]
        public int AwayTeamId { get; set; }
        public virtual TournamentClub AwayTeam { get; set; }

        [ForeignKey("HomeTeamId")]
        public int HomeTeamId { get; set; }
        public virtual TournamentClub HomeTeam { get; set; }


        [Display(Name = "Home team score")]
        [Required(ErrorMessage = "The home team score is required")]
        public int HomeTeamScore { get; set; }

        [Display(Name = "Away team score")]
        [Required(ErrorMessage = "The away team score is required")]
        public int AwayTeamScore { get; set; }

        [Display(Name = "Match date")]
        [Required(ErrorMessage = "The match date is required")]

        [DataType(DataType.Date)]
        public DateTime MatchDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime MatchTime { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }
        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        [Display(Name = "Match Location")]
        [Required(ErrorMessage = "The location is required")]
        public string Location { get; set; }

        [Display(Name = "Tournament")]
        public int TournamentId { get; set; }

        [ForeignKey("TournamentId")]
        public virtual Tournament Tournament { get; set; }
    }
}
