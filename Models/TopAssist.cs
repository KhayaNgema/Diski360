using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TopAssist
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TopAssistId { get; set; }

        public int LeagueId { get; set; }

        public virtual League League { get; set; }

        public string PlayerId { get; set; }

        public virtual Player Player { get; set; }

        public int NumberOfAssists { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }
}
