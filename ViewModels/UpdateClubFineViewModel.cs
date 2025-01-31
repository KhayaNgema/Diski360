namespace MyField.ViewModels
{
    public class UpdateClubFineViewModel
    {
        public int? FineId { get; set; }
        public string ClubName { get; set; }
        public string RuleViolated {  get; set; }

        public string FineDetails { get; set; }

        public decimal FineAmount { get; set; }

        public DateTime FineDueDate { get; set; }
    }
}
