namespace MyField.ViewModels
{
    public class UpdateTournamentViewModel
    {
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }

        public string TournamentDescription { get; set; }

        public DateTime StartDate { get; set; }

        public double? JoiningFee { get; set; }

        public string TournamentLocation { get; set; }

        public string TournamentImage { get; set; }

        public int? NumberOfTeams { get; set; }

        public DateTime JoiningDueDate { get; set; }

        public string SponsorName { get; set; }

        public string Sponsorship { get; set; }
        public string? SponsorContactDetails { get; set; }

        public IFormFile TournamentImages { get; set; }
    }
}
