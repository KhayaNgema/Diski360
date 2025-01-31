using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class FixtureViewModel
    {
        public int FixtureId { get; set; }


        [Required(ErrorMessage ="Home team is required")]
        [Display(Name = "Home Team")]
        public int HomeTeamId { get; set; }

        [Required(ErrorMessage = "Away team is required")]

        [Display(Name = "Away Team")]
        public int AwayTeamId { get; set; }

        [DataType(DataType.Date)]
        public DateTime KickOffDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime KickOffTime { get; set; }

        [Required(ErrorMessage = "Stadium is required")]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Match official (Referee) is required")]
        public string Refeere { get; set; }

        [Required(ErrorMessage = "Match official is required")]

        public string AssistantOne {  get; set; }

        [Required(ErrorMessage = "Match officail is required")]

        public string AssistantTwo { get; set; }

        public FixtureStatus FixtureStatus { get; set; }
    }

}
