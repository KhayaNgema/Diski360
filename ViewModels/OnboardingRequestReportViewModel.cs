namespace MyField.ViewModels
{
    public class OnboardingRequestReportViewModel
    {
        public int OnboardingRequestsTotalCount { get; set; }

        public int PendingRequestsCount { get; set; }
        public int ApprovedRequestsCount { get; set; }

        public int RejectedRequestsCount { get; set; }

        public int CompletedRequestsCount { get; set;}
    }
}
