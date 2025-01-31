using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class ModifyFixtureViewModel
    {
        public int FixtureId { get; set; }
        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }

        [DataType(DataType.Date)]
        public DateTime KickOffDate { get; set; }

        [DataType(DataType.Time)]
        public DateTime KickOffTime { get; set; }

        public string RefereeId { get; set; }

        public string AssistantOneId { get; set; }

        public string AssistantTwoId { get; set; }

        public string Stadium { get; set; }

        public FixtureStatus FixtureStatus { get; set; }
    }
}
