using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.Models
{
    public class TournamentRules
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RuleId { get; set; }

        public int TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; }

        public string RuleDescription { get; set; }

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
