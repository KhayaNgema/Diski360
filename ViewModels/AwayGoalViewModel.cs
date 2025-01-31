using MyField.Models;

namespace MyField.ViewModels
{
    public class AwayGoalViewModel
    {
        public int LiveId { get; set; }
        public int FixtureId { get; set; }

        public string GoalScorerById { get; set; }

        public string? AssistedById { get; set;}

    }
}
