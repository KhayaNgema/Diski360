using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class MatchFormation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int MatchFormationId { get; set; }

        public int ClubId { get; set; } 

        public virtual Club Club { get; set; }

        public int FormationId { get; set; }

        public virtual Formation Formation { get; set; }

        public int FixtureId { get; set; }

        public virtual Fixture Fixture { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ClubManager CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual ClubManager ModifiedBy { get; set; }

    }
}
