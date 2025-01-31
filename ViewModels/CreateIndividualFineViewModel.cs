namespace MyField.ViewModels
{
    public class CreateIndividualFineViewModel
    {
        public string FineDetails { get; set; }
        public string RuleViolated { get; set; }

        public string OffenderId { get; set; }

        public decimal FineAmount { get; set; }

        public DateTime FineDuDate { get; set; }
    }
}
