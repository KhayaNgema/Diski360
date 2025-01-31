using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Formation
    {
        public int FormationId { get; set; }

        public string FormationName { get; set; }

        public string FormationImage { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

    }
}
