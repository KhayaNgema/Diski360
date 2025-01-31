using Microsoft.AspNetCore.Mvc.Rendering;
using MyField.Models;

namespace MyField.ViewModels
{
    public class AwayPenaltyViewModel
    {
        public int FixtureId { get; set; }

        public IEnumerable<dynamic> Players { get; set; }

        public string AwayTeam { get; set; }
    }
}
