namespace MyField.ViewModels
{
    public class ActivityLogViewModel
    {
        public int ActivityLogId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ClubName { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceDetails { get; set; }
        public string BrowserDetails { get; set; }
        public string LocationDetails { get; set; }
    }

}
