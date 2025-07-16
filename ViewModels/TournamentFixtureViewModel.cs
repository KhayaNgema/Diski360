using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class TournamentFixtureViewModel
    {
        public int TournamentId { get; set; }

        [Required(ErrorMessage = "Home team is required")]
        [Display(Name = "Home Team")]
        public int HomeTeamClubId { get; set; }

        [Required(ErrorMessage = "Away team is required")]

        [Display(Name = "Away Team")]
        public int AwayTeamClubId { get; set; }

        [DataType(DataType.Date)]
        public DateTime KickOffDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime KickOffTime { get; set; }

        [Required(ErrorMessage = "Stadium is required")]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Match official (Referee) is required")]

        public FixtureStatus FixtureStatus { get; set; }

        public FixtureRound FixtureRound { get; set; }
    }
}
