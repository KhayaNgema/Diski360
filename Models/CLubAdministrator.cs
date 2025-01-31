using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class ClubAdministrator : UserBaseModel
    {
        [Required(ErrorMessage = "Please select a club.")]
        [Display(Name = "Club")]
        public int ClubId { get; set; }
        public virtual Club Club { get; set; }

        public int? DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        public virtual Division Division { get; set; }
    }
}
