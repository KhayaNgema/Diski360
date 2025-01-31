using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyField.Models;

namespace MyField.ViewModels
{
    public class StandingsViewModel
    {
        public int ClubId { get; set; }
        public int Position { get; set; }

        public int MatchPlayed { get; set; }

        public int Points { get; set; }

        public int Wins { get; set; }

        public int Lose { get; set; }

        public int GoalsScored { get; set; }

        public int GoalsConceded { get; set; }

        public int GoalDifference { get; set; }
        public int Draw { get; set; }
    }

}

