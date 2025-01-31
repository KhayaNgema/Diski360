using MyField.Models;

namespace MyField.ViewModels
{
    public class CreateMatchLineUpViewModel
    {
        public List<MatchLineUpXIViewModel> MatchLineUpXI { get; set; }
        public List<MatchLineUpSubstitutesViewModel> MatchLineUpSubstitutes { get; set; }
        public List<Player> Players { get; set; }
    }
}
