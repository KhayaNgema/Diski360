namespace MyField.ViewModels
{
    public class UpdateTournamentClubViewModel
    {
        public int TournamentId { get; set; }

        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public string? Email { get; set; }
        public string? ClubLocation { get; set; }
        public string? ClubBadge { get; set; }
        public IFormFile ClubBadges { get; set; }
        public string? ManagerProfilePicture { get; set; }
        public IFormFile ManagerProfilePictures { get; set; }
        public string? ClubHistory { get; set; }
        public string? ClubSummary { get; set; }
        public string ClubAbbr { get; set; }
        public string? ClubManagerName { get; set; }
        public string? ClubManagerPhone { get; set; }
        public string? ClubManagerEmail { get; set; }
        public int? DivisionId { get; set; }
    }
}
