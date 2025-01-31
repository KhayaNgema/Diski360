using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.Models
{
    public class Warning
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarningId { get; set; }

        public string UserId { get; set; }
        public virtual UserBaseModel UserBaseModel { get; set; }

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


}
