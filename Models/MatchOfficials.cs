using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class MatchOfficials
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int MatchOfficialsId { get; set; } 

        public int FixtureId { get; set; }  

        public virtual Fixture Fixture { get; set; }


        [ForeignKey("AssistantOneId")]
        public string RefeereId { get; set; }

        public virtual Officials Refeere { get; set; }

        [ForeignKey("AssistantOneId")]
        public string AssistantOneId { get; set; }

        public virtual Officials AssistantOne { get; set; } 

        public string AssistantTwoId { get; set; }

        public virtual Officials AssistantTwo { get; set; }

        public int? DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        public virtual Division Division { get; set; }
    }
}
