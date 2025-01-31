using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Meeting
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MeetingId { get; set; } 

        public string MeetingTitle { get; set;}

        public string MeetingDescription { get; set;}

        public string Venue { get; set;}

        [DataType(DataType.Date)]
        public DateTime MeetingDate { get; set;}

        [DataType(DataType.Time)]
        public DateTime MeetingTime { get; set;}  

        public string AdditionalComments { get; set;}

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual SportsMember CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual SportsMember ModifiedBy { get; set; }

        public MeetingAttendees MeetingAttendees { get; set; }

        public MeetingStatus MeetingStatus { get; set; }
    }


    public enum MeetingAttendees
    {
        [Display(Name = "Everyone")]
        Everyone,

        [Display(Name = "Sport Administrators")]
        Sport_Administrators,

        [Display(Name = "Division Managers")]
        Division_Managers,

        [Display(Name = "Sport Coordinators")]
        Sport_Coordinators,

        [Display(Name = "News Administrators")]
        News_Administrators,

        [Display(Name = "News Updaters")]
        News_Updaters,

        [Display(Name = "Club Administrators")]
        Club_Administrators,

        [Display(Name = "Club Managers")]
        Club_Managers,

        [Display(Name = "Personnel Administrators")]
        Personnel_Administrators,

        [Display(Name = "Fans Administrators")]
        Fans_Administrators,

        [Display(Name = "Officials")]
        Officials,

        [Display(Name = "Players")]
        Players
    }


    public enum MeetingStatus
    {
        Upcoming,
        Postponed,
        Cancelled
    }
}
