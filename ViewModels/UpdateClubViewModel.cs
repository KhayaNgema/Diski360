using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class UpdateClubViewModel
    {
        public int ClubId { get; set; }

        [Required(ErrorMessage = "Club name is required")]
        [Display(Name = "Club")]
        public string? ClubName { get; set; }


        [Required(ErrorMessage = "Club email adress is required")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Club area is required")]
        [Display(Name = "Home Area")]
        public string? ClubLocation { get; set; }

        [Display(Name = "ClubHistory")]
        public string? ClubHistory { get; set; }

        [Display(Name = "ClubSummary")]
        public string? ClubSummary { get; set; }

        [Display(Name = "Badge")]
        public string? ClubBadges { get; set; }

        public IFormFile? ClubBadgeFile { get; set; }

        public string? ClubAbbr { get; set; }
    }
}
