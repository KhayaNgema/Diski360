using MyField.Models;

namespace MyField.ViewModels
{
    public class FineDetailsViewModel
    {
        public int FineId { get; set; } 

        public string FineDetails { get; set; }

        public decimal FineAmount { get; set; }

        public DateTime FineDueDate { get; set; }

        public PaymentStatus PaymentStatus { get; set; }    
    }
}
