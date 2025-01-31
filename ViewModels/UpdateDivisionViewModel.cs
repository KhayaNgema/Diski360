namespace MyField.ViewModels
{
    public class UpdateDivisionViewModel
    {
        public int DivisionId { get; set; }
        public IFormFile DivisionBadgeFile { get; set; }

        public string? DivisionBadges { get; set; }

        public string DivisionName { get; set; }

        public string DivisionAbbr { get; set; }

        public string DivisionDescription { get; set; }
    }
}
