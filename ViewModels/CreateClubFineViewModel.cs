using MyField.Models;

namespace MyField.ViewModels
{
    public class CreateClubFineViewModel
    {
        public string FineDetails { get; set; }

        public string RuleViolated { get; set; }

        public int ClubId { get; set; }
        public decimal FineAmount { get; set; }

        public DateTime FineDuDate { get; set; }
    }
}
