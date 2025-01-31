using MyField.Models;

namespace MyField.ViewModels
{
    public class MeetingViewModel
    {
        public string MeetingTitle { get; set; }

        public string MeetingDescription { get; set; }

        public string Venue { get; set; }

        public DateTime MeetingDate { get; set; }

        public DateTime MeetingTime { get; set; }

        public string AdditionalComments { get; set; }

        public MeetingAttendees MeetingAttendes { get; set; }
    }
}
