namespace MyField.ViewModels
{
    public class AwaySubViewModel
    {
        public IEnumerable<dynamic> StartingXi { get; set; }
        public IEnumerable<dynamic> Substitutes { get; set; }

        public int FixtureId { get; set; }

        public string AwayTeam { get; set; }
    }
}
