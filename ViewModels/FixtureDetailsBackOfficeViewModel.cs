using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class FixtureDetailsBackOfficeViewModel
    {
        public string HomeTeamName { get; set; }

        public string AwayTeamName { get; set; }

        public string HomeTeamBadge { get; set; }

        public string AwayTeamBadge { get; set; }

        [DataType(DataType.Date)]
        public DateTime KickOffDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime KickOffTime { get; set; }

        public string RefereeName { get; set; } 

        public string AssistantOneName { get; set; }    

        public string AssistantTwoName { get; set;}

        public string StadiumName { get; set; }
    }
}
