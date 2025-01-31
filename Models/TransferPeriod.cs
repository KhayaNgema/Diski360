using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TransferPeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransferPeriodId { get; set; }

        public int LeagueId { get; set; }

        public virtual League League { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public int PeriodOpenCount { get; set; }

        public bool IsCurrent {  get; set; }

        public bool IsOpened { get; set; }

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
