using MyField.Models;

namespace MyField.ViewModels
{
    public class DivisionDetailsViewModel
    {
        public string DivisionBadge {  get; set; }

        public string DivisionName { get; set; }

        public DivisionType DivisionType { get; set;}

        public string DivisionAbbr { get; set;}

        public string Address { get; set;}

        public string ManagerFirstName { get; set;}
        public string ManagerLastName { get; set; }


        public string ManagerEmail { get; set;}

        public string ManagerPhone { get; set;}

        public string Contract {  get; set;}

        public string DivisionDescription { get; set;}
    }
}
