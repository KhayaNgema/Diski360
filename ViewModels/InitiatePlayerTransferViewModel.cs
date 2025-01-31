using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.ViewModels
{
    public class InitiatePlayerTransferViewModel
    {
        public int LeagueId { get; set; }
        public int MarketId { get; set; }
        public string PlayerId { get; set; }
        public int SellerClubId { get; set; }
        public string? ProfilePicture {  get; set; } 

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Position Position { get; set; }

        public int JerseyNumber { get; set; }
  

        public decimal MarketValue { get; set; } 

        public DateTime DateOfBirth { get; set; }

        public string? ClubName { get; set; }    

        public string? ClubBadge { get; set; }   
    }
}
