using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class HeadTohead
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HeadToHeadId { get; set; }    

        public int ClubId { get; set; } 

        public virtual Club Club { get; set; }
        

        [ForeignKey("HomeTeamId")]
        public int HomeTeamId{ get; set; }

        public virtual Club HomeTeam { get; set; }


        [ForeignKey("AwayTeamId")]
        public int AwayTeamId { get; set; }
        public virtual Club AwayTeam { get; set; }

        public string MatchResults {  get; set; }  

        public int HomeTeamGoals { get; set; }

        public int AwayTeamGoals{ get; set; }

        public DateTime HeadToHeadDate { get; set; }
    }
}
