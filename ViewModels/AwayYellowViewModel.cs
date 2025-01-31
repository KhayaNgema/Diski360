using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyField.ViewModels
{
    public class AwayYellowViewModel
    {
        public int FixtureId { get; set; }

        public IEnumerable<dynamic> Players { get; set; }

        public string AwayTeam { get; set; }

        public List<SelectListItem> YellowCardTypes { get; set; }
    }
}
