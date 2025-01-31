using Microsoft.EntityFrameworkCore.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class LineUp
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LineUpId { get; set; } 

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

        public virtual ICollection<LineUpXI> LineUpXI { get; set; }

        public virtual ICollection<LineUpSubstitutes> LineUpSubstitutes { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }
    }
}
