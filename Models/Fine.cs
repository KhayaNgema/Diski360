using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Fine
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int FineId { get; set; } 

        public string RuleViolated { get; set; }    

        public string FineDetails {  get; set; }

        public int? ClubId { get; set; }
        public virtual Club Club { get; set; }


        [ForeignKey("OffenderId")]
        public string? OffenderId { get; set; }

        public virtual UserBaseModel Offender { get; set; }


        [ForeignKey("CreatedById")]
        public string CreatedById { get; set; } 

        public virtual SportsMember CreatedBy { get; set; } 

        public DateTime CreatedDateTime { get; set; }

        [ForeignKey("ModifiedById")]
        public string ModifiedById { get; set; }

        public virtual SportsMember ModifiedBy { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public decimal FineAmount { get; set; } 

        public DateTime FineDuDate { get; set; }    

        public PaymentStatus PaymentStatus { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,    
        Paid,
        Overdue
    }
}
