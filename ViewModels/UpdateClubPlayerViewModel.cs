using MyField.Models;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class UpdateClubPlayerViewModel
    {
        [Required]
        public string Id { get; set; }

        public string ProfilePicture {  get; set; } 

        public string PlayerCard { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Position")]
        public Position Position { get; set; }

        [Display(Name = "Jersey Number")]
        public int JerseyNumber { get; set; }
        public IFormFile? ProfilePictureFile { get; set; }

        public IFormFile? PlayerCardFile { get; set; }
        public decimal MarketValue { get; set; }    
    }
}