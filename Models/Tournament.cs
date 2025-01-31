using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Tournament
    {
        public int TournamentId { get; set; }

        public string TournamentName { get; set; }

        public string TournamentDescription { get; set; }

        public string TournamentType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string TournamentOrgarnizer { get; set; }

        public string JoiningFee { get; set; }

        public string TournamentRules { get; set; }

        public string TournamentStatus { get; set; }

        public string TournamentLocation { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual IdentityUser CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual IdentityUser ModifiedBy { get; set; }

    }
}
