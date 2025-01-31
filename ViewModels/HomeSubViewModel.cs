namespace MyField.ViewModels
{
    public class HomeSubViewModel
    {
        public IEnumerable<dynamic> StartingXi{ get; set; }
        public IEnumerable<dynamic> Substitutes { get; set; }

        public int FixtureId { get; set; }

        public string HomeTeam { get; set; }
    }
}
