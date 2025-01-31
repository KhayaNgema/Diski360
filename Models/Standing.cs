using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MyField.Models
{
    public class Standing
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StandingId { get; set; }

        [Display(Name = "Club")]
        public int ClubId { get; set; }

        public virtual Club Club { get; set; }

        [Display(Name = "Position")]
        public int Position { get; set; }

        [Display(Name = "MP")]
        public int MatchPlayed { get; set; }

        [Display(Name = "Pts")]
        public int Points { get; set; }

        [Display(Name = "W")]
        public int Wins { get; set; }

        [Display(Name = "L")]
        public int Lose { get; set; }

        [Display(Name = "GF")]
        public int GoalsScored { get; set; }

        [Display(Name = "GA")]
        public int GoalsConceded { get; set; }

        [Display(Name = "GD")]
        public int GoalDifference { get; set; }
        [Display(Name = "D")]
        public int Draw { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual IdentityUser CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual IdentityUser ModifiedBy { get; set; }

        public string Last5Games { get; set; }

        [Display(Name = "League")]
        public int LeagueId { get; set; }

        [ForeignKey("LeagueId")]
        public virtual League League { get; set; }

        public string? Reason { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public Standing()
        {
            Last5Games = "";
        }
    }
}

