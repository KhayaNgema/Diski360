using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Tournament
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TournamentId { get; set; }

        public string TournamentName { get; set; }

        public string TournamentDescription { get; set; }

        public DateTime StartDate { get; set; }

        public double? JoiningFee { get; set; }

        public TournamentStatus TournamentStatus { get; set; }

        public string TournamentLocation { get; set; }

        public string TournamentImage { get; set; }

        public string? TrophyImage { get; set; }

        public int? NumberOfTeams { get; set; }

        public DateTime JoiningDueDate { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public string? SponsorName { get; set; }

        public string? Sponsorship {  get; set; }
        public string? SponsorContactDetails { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public  bool IsPublished { get; set; }

        public TournamentType TournamentType { get; set; }


        public Tournament()
        {
            TrophyImage = "Images/trophy_placeholder.jpg";
        }

    }

    public enum TournamentStatus
    { 
        Upcoming,
        Started,
        Ended
    }

    public enum TournamentType
    {
        [Display(Name = "Open Entry")]
        Open_Entry,

        [Display(Name = "Qualification Based")]
        Qualification_Based
    }
}
