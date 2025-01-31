using MyField.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.ViewModels
{
    public class PayPlayerTransferViewModel
    {
        public int TransferId { get; set; }

        public string PlayerId { get; set; }

        public string? ProfilePicture { get; set; }  

        public int SellerClubId { get; set; }

        public int CustomerClubId { get; set; } 

        public int PlayerTransferMarketId { get; set; } 

        public string? SellerClubName { get; set; }

        public string? SellerClubBadge {  get; set; }

        public string? BuyerClubName { get; set; }   

        public string? BuyerClubBadge { get; set; }  

        public decimal PlayerAmount { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public Position Position { get; set; }    

        public int JerseyNumber { get; set; }   

        public DateTime DateOfBirth { get; set; }

        
        public PaymentTransferStatus PaymentStatus { get; set; }
    }
}
