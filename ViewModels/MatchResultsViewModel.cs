using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class MatchResultsViewModel
    {
        public int FixtureId { get; set; }

        [Display(Name = "Home team")]
        [Required(ErrorMessage = "The home team is required")]
        public string HomeTeam { get; set; }

        [Display(Name = "Away team")]
        [Required(ErrorMessage = "The away team is required")]
        public string AwayTeam { get; set; }

        [Display(Name = "Home team score")]
        [Required(ErrorMessage = "The home team score is required")]
        public int HomeTeamScore { get; set; }

        [Display(Name = "Away team score")]
        [Required(ErrorMessage = "The away team score is required")]
        public int AwayTeamScore { get; set; }

        [Display(Name = "Home team badge")]
        [Required(ErrorMessage = "The home team badge is required")]
        public string HomeTeamBadge { get; set; }

        [Display(Name = "Away team badge")]
        [Required(ErrorMessage = "The away team badge is required")]
        public string AwayTeamBadge { get; set; }

        [Display(Name = "Match date")]
        [Required(ErrorMessage = "The match date is required")]
        public DateTime MatchDate { get; set; }

        public DateTime MatchTime { get; set; }

        [Display(Name = "Match Location")]
        [Required(ErrorMessage = "The location is required")]
        public string Location { get; set; }

        public int HomeTeamId { get; set; } 

        public int AwayTeamId { get; set; }
    }
}
