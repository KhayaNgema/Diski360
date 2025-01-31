using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class ClubWarning
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarningId { get; set; }

        public int CLubId { get; set; }
        public virtual Club Club { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int NumberOfOffences { get; set; }

        public WarningStatus Status { get; set; }
    }

    public enum WarningStatus
    {
        Active,
        Expired,
        Cancelled
    }

}
