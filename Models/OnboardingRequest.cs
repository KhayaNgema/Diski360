using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class OnboardingRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public string ManagerFirstName { get; set; }

        public string ManagerLastName { get; set; }

        public string ManagerEmail { get; set; }

        public string ManagerPhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public string? DivisionBadge { get; set; }

        public string DivisionName { get; set; }

        public string DivisionAbbr { get; set; }

        public string DivisionDescription { get; set; }

        public string Address { get; set; }

        public DivisionType DivisionType { get; set; }

        public string ReferenceNumber { get; set; }
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed
    }
}
