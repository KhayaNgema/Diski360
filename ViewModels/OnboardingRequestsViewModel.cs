using MyField.Models;

namespace MyField.ViewModels
{
    public class OnboardingRequestsViewModel
    {
        public string ReferenceNumber { get; set; }

        public string DivisionName { get; set; }

        public RequestStatus Status { get; set; }

        public DateTime DateTime { get; set; }

        public string ManagerName { get; set; }

        public string DivisionAbbr { get; set; }

        public int RequestId { get; set; }
    }
}
