using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TournamentClub
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TournamentClubId { get; set; }

        public string ClubName { get; set; }

        public string ClubDivision { get; set; }

        public int TournamentId {get; set;}

        public virtual Tournament Tournament { get; set; }

        public bool IsStillActive { get; set; }

        public string ClubManagerName { get; set; }

        public string ClubManagerPhone { get; set; }

        public string ClubManagerEmail { get; set; }

        public bool HasPaid { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string? ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }
    }
}
