using MyField.Models;

namespace MyField.ViewModels
{
    public class HomeGoalCombinedViewModel
    {
        public AwayGoalViewModel HomeGoalViewModel { get; set; }
        public IEnumerable<dynamic> Players { get; set; }

        public int FixtureId { get; set; }

        public string HomeTeam {  get; set; }   
    }

}
