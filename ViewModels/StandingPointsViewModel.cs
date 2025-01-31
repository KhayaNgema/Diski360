using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyField.ViewModels
{
    public class StandingPointsViewModel
    {
        public int StandingId { get; set; }

        [Display(Name = "Points")]
        public int Points { get; set; }

        public int PointsToBeAdded { get; set; }

        public int PointsToBeSubtracted { get; set; }

        public int Goals { get; set; }

        public int GoalsToBeAdded { get; set; }

        public int GoalsToBeSubtracted { get; set; }

        public string? ClubName { get; set; }

        public string? ClubBadge { get; set; }

        [Required(ErrorMessage = "Club code is required when updating standings")]
        public string ClubCode { get; set; }

        public string? Reason { get; set; }  
    }
}

