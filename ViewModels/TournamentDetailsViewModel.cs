﻿using MyField.Models;

namespace MyField.ViewModels
{
    public class TournamentDetailsViewModel
    {
        public int TournamentId { get; set; }   
        public string TournamentName { get; set; }

        public string TournamentDescription { get; set; }

        public DateTime StartDate { get; set; }

        public double? JoiningFee { get; set; }

        public string TournamentLocation { get; set; }

        public string TournamentImage { get; set; }

        public string? TrophyImage { get; set; }

        public int? NumberOfTeams { get; set; }

        public DateTime JoiningDueDate { get; set; }

        public string SponsorName { get; set; }

        public string Sponsorship { get; set; }
        public string? SponsorContactDetails { get; set; }

        public TournamentStatus TournamentStatus { get; set; }

        public bool IsPublished { get; set; }
    }
}
