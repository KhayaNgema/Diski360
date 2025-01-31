using MyField.Models;

namespace MyField.ViewModels
{
    public class AwayGoalCombinedViewModel
    {
        public AwayGoalViewModel AwayGoalViewModel { get; set; }
        public IEnumerable<dynamic> Players { get; set; }

        public int FixtureId { get; set; }

        public string AwayTeam {  get; set; }
    }

}
