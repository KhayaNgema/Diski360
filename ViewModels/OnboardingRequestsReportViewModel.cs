namespace MyField.ViewModels
{
    public class OnboardingRequestsReportViewModel
    {
        public int TotalOnboardingRequests { get; set; }

        public int PendingOnboardingRequests { get; set; }

        public int ApprovedOnboardingRequests { get; set;}

        public int RejectedOnboardingRequests { get; set;}

        public int CompletedOnboardingRequests { get; set;}
    }
}
