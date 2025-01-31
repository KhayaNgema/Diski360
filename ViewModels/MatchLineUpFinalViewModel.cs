using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.ViewModels
{
    public class MatchLineUpFinalViewModel
    {
        public int FixtureId { get; set; }

        public int ClubId { get; set; }

        public DateTime FixtureDate { get; set; }

        public DateTime FixtureTime { get; set; }
    }
}
