using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.ViewModels
{
    public class MatchLineUpSubstitutesViewModel
    {
        public string UserId { get; set; }

        public int ClubId { get; set; }

        public int FixtureId { get; set; }
    }
}
