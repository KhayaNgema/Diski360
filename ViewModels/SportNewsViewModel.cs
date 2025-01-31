using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class SportNewsViewModel
    {
        [Display(Name = "Title")]
        [Required(ErrorMessage = "News title is required")]
        public string NewsHeading { get; set; }

        [Display(Name = "Body")]
        [Required(ErrorMessage = "News body is required")]
        public string NewsBody { get; set; }

        [Display(Name = "Image")]
        [Required(ErrorMessage = "News Image is required")]
        public IFormFile NewsImages { get; set; }
    }
}
