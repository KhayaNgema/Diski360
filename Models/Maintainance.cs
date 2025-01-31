using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Maintainance
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int MaintainanceId { get; set; }

        [ForeignKey("CreatedById")]
        public string CreatedById {  get; set; }          

        public virtual UserBaseModel CreatedBy { get; set; } 

        public DateTime CreatedDateTime { get; set; }

        [ForeignKey("ResolvedById")]
        public string ResolvedById { get; set; }

        public virtual UserBaseModel ResolvedBy { get; set; }

        public string MaintainanceDetails { get; set; }

        public MaintainanceRequestStatus maintainanceRequestStatus { get; set; }

    }

    public enum MaintainanceRequestStatus 
    { 
        Pending,
        Working_On,
        Resolved,
    }

}
