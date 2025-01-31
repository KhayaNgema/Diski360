using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class PayFastWebhookPayload
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayFastWebhookPayloadId {  get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public decimal AmountPaid { get; set; }
        public string ItemName { get; set; }
    }

}
