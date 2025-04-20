using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TournamentClubs
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int ClubId { get; set; }

        public int TournamentId { get; set; }

        public virtual Tournament Tournament { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        [Display(Name = "Club")]
        public string ClubName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Club email is required")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Club area is required")]
        [Display(Name = "Home Area")]
        public string? ClubLocation { get; set; }

        [Display(Name = "Badge")]
        public string? ClubBadge { get; set; }

        [Display(Name = "ClubHistory")]
        public string? ClubHistory { get; set; }

        [Display(Name = "ClubSummary")]
        public string? ClubSummary { get; set; }

        public string ClubAbbr { get; set; }

        public string ClubManagerName { get; set; }

        public string ClubManagerPhone { get; set; }

        public string ClubManagerEmail { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public bool? HasJoined { get; set; }

        public bool? IsEliminated { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public TournamentClubs()
        {
            ClubBadge = "Images/placeholder_club_badge.jpg";
        }
    }
}
