using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class OnboardingRequestViewModel
    {
        public string ManagerFirstName { get; set; }

        public string ManagerLastName { get; set; }

        public string ManagerEmail { get; set; }

        public string ManagerPhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public IFormFile? DivisionBadges { get; set; }

        public string DivisionName { get; set; }

        public string DivisionAbbr { get; set; }

        public string DivisionDescription { get; set; }

        public string AddressLine_1 { get; set; }

        public string AddressLine_2 { get; set; }

        public string Suburb { get; set; }

        public string Town_City { get; set; }

        public string Province { get; set; }

        public string ZipCode { get; set; }

        public DivisionType DivisionType { get; set; }
    }
}
