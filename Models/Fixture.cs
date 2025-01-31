using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MyField.Models
{
    public class Fixture
    {
        public int FixtureId { get; set; }

        [Display(Name = "Home Team")]
        [Required(ErrorMessage = "Home Team is required")]
        public int HomeTeamId { get; set; }

        [Display(Name = "Away Team")]
        [Required(ErrorMessage = "Away Team is required")]
        public int AwayTeamId { get; set; }

        [DataType(DataType.Date)]
        public DateTime KickOffDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime KickOffTime { get; set; }

        [Display(Name = "Location")]
        [Required(ErrorMessage = "Match location is required")]
        public string Location { get; set; }

        [ForeignKey("HomeTeamId")]
        public Club HomeTeam { get; set; }

        [ForeignKey("AwayTeamId")]
        public Club AwayTeam { get; set; }

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
        public int LeagueId { get; set; }

        [ForeignKey("LeagueId")]
        public virtual League League { get; set; }

        public Fixture()
        {
            KickOffDate = DateTime.MinValue;
        }
        public ICollection<MatchOfficials> MatchOfficials { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }

    public enum FixtureStatus
    {
        Upcoming,
        Postponed,
        Interrupted,
        Ended,
        Live
    }
}
