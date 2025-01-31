using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.Models
{
    public class LineUpXI
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int LineUpXIId { get; set; }

        [ForeignKey("Player")]
        public string PlayerId { get; set; }

        public virtual Player ClubPlayer { get; set; }

        public int FixtureId { get; set; }

        public virtual Fixture Fixture { get; set; }

        public int ClubId { get; set; }

        public virtual Club Club { get; set; }


        [ForeignKey("CreatedById")]

        public string CreatedById { get; set; }
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

    }

    public class LineUpXIHolder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int LineUpXIId { get; set; }

        [ForeignKey("Player")]
        public string PlayerId { get; set; }

        public virtual Player ClubPlayer { get; set; }


        public int FixtureId { get; set; }

        public virtual Fixture Fixture{ get; set; }

        public int ClubId { get; set; }

        public virtual Club Club { get; set; }


        [ForeignKey("CreatedById")]

        public string CreatedById { get; set; }
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

    }
}
