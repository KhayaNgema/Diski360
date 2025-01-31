using Microsoft.AspNetCore.Mvc.Rendering;
using MyField.Models;

namespace MyField.ViewModels
{
    public class HomePenaltyViewModel
    {
        public int FixtureId { get; set; }

        public IEnumerable<dynamic> Players { get; set; }

        public string HomeTeam { get; set; }
    }
}
