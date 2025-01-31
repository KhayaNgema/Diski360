using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class MeetingDetailsViewModel
    {
        public string MeetingDescription { get; set; }

        public string AdditionalComments { get; set; }

        [DataType(DataType.Date)]
        public DateTime MeetingDate { get; set; }


        [DataType(DataType.Time)]

        public DateTime MeetingTime { get; set; }

        public string Venue { get; set; }

        public string MeetingTitle { get; set; }

        public MeetingAttendees MeetingAttendees { get; set; }

        public MeetingStatus MeetingStatus { get; set; }    
    }
}
