using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyField.ViewModels
{
    public class HomeYellowViewModel
    {
        public int FixtureId { get; set; }

        public IEnumerable<dynamic> Players { get; set; }

        public string HomeTeam { get; set; }

        public List<SelectListItem> YellowCardTypes { get; set; }
    }
}
