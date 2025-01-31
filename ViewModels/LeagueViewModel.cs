using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class LeagueViewModel
    {
        public string LeagueYears { get; set; }

        [Required]
        public string OldLeagueCode { get; set; }
    }

}
