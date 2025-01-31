using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class LineUpSubstitutes
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LineUpSubstituteId { get; set; }

        [ForeignKey("PlayerId")]
        public string PlayerId { get; set; }

        public virtual Player ClubPlayer { get; set; }

        public int ClubId { get; set; }

        public virtual Club Club { get; set; }

        public int FixtureId { get; set; }

        public virtual Fixture Fixture { get; set; }


        [ForeignKey("CreatedById")]
        public string CreatedById { get; set; }
        public virtual UserBaseModel CreatedBy { get; set; }



        [ForeignKey("ModifiedById")]
        public string ModifiedById { get; set; }
        public virtual UserBaseModel ModifiedBy { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }
    }

    public class LineUpSubstitutesHolder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LineUpSubstituteId { get; set; }


        [ForeignKey("PlayerId")]
        public string PlayerId { get; set; }

        public virtual Player ClubPlayer { get; set; }

        public int ClubId { get; set; }

        public virtual Club Club { get; set; }

        public int FixtureId { get; set; }

        public virtual Fixture Fixture { get; set; }


        [ForeignKey("CreatedById")]
        public string CreatedById { get; set; }
        public virtual UserBaseModel CreatedBy { get; set; }



        [ForeignKey("ModifiedById")]
        public string ModifiedById { get; set; }
        public virtual UserBaseModel ModifiedBy { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }
    }
}
