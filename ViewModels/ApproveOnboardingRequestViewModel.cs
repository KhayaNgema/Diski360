using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class ApproveOnboardingRequestViewModel
    {
        public int RequestId { get; set; }
        public string? DivisionBadge { get; set; }

        public string DivisionName { get; set; }

        public string DivisionDescription { get; set; }

        public string Address { get; set; }

        public DivisionType DivisionType { get; set; }

        public string ManagerFirstName { get; set; }

        public string ManagerLastName { get; set; }

        public string ManagerEmail { get; set; }

        public string ManagerPhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime AgreementStartDate {  get; set; }

        public DateTime AgreementEndDate {  get; set; }
    }
}
