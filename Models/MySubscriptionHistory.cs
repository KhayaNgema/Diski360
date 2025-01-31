using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class SubscriptionHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MySubscriptionHistoryId { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserBaseModel AccountHolder { get; set; }

        public int SubscriptionId { get; set; }
        public virtual Subscription Subscription { get; set; }

        public int? ClubId { get; set; }
        public virtual Club Club { get; set; }

        public SubscriptionPlan Plan { get; set; }

        public SubscriptionStatus Status { get; set; }

        public DateTime SubscribedDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public decimal AmountPaid { get; set; }
    }
}
