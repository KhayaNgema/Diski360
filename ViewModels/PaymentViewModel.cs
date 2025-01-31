using MyField.Models;

namespace MyField.ViewModels
{
    public class PaymentViewModel
    {
        public int PaymentId { get; set; }  
        public decimal AmountPaid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime PaymentDate { get; set; }  

        public PaymentPaymentStatus PaymentStatus { get; set; } 

        public string DeviceName {  get; set; }

        public string Browser { get; set; }

        public string DeviceLocation { get; set; }

    }
}
