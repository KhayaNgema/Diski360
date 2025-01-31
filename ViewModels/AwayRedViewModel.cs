using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyField.ViewModels
{
    public class AwayRedViewModel
    {
        public int FixtureId { get; set; }

        public IEnumerable<dynamic> Players { get; set; }

        public string AwayTeam { get; set; }

        public List<SelectListItem> RedCardTypes { get; set; }
    }
}
