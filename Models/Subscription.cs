using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Subscription
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionId {  get; set; }

        public  SubscriptionPlan SubscriptionPlan { get; set; }

        public SubscriptionStatus SubscriptionStatus { get; set; }

        
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserBaseModel SystemUser { get; set; }


        public int? ClubId { get; set; }
        public virtual Club Club { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpirationDate { get; set; }
    }

    public enum SubscriptionPlan
    { 
        Basic,
        Premium,
        Club_Premium
    }

    public enum SubscriptionStatus 
    {
        Active,
        Cancelled,
        Expired
    }

}
