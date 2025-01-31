using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Club
    {
        public Club()
        {
            ClubBadge = "Images/placeholder_club_badge.jpg";
        }

        [Display(Name = "Club Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClubId { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        [Display(Name = "Club")]
        public string ClubName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Club email is required")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Club area is required")]
        [Display(Name = "Home Area")]
        public string ClubLocation { get; set; }

        [Display(Name = "Badge")]
        public string? ClubBadge { get; set; } 

        [Display(Name = "ClubHistory")]
        public string? ClubHistory { get; set; }

        [Display(Name = "ClubSummary")]
        public string? ClubSummary { get; set; }

        public string ClubAbbr { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        [Display(Name = "League")]
        public int LeagueId { get; set; }

        [ForeignKey("LeagueId")]
        public virtual League League { get; set; }

        public string ClubCode { get; set; }

        public bool IsActive { get; set; }

        public ClubStatus Status { get; set; }

        [ForeignKey("ClubManagerId")]
        public string? ClubManagerId { get; set; }

        public ClubManager ClubManager { get; set; }

        public bool IsSuspended { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }

    public enum ClubStatus
    {
        Active,
        Previous_Season,
    }
}
