using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.ViewModels
{
    public class PayClubFineViewModel
    {
        public int FineId { get; set; }

        public string FineDetails { get; set; }

        public int ClubId { get; set; }

        public string ClubName { get; set; }

        public string ClubBadge { get; set; }

        public decimal FineAmount { get; set; }

        public DateTime FineDuDate { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
    }
}
