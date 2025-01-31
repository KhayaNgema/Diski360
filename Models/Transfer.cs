using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Transfer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransferId { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public int LeagueId { get; set; }

        public virtual League League { get; set; }

        [ForeignKey("PlayerId")]
        public string PlayerId { get; set; }

        public virtual Player Player { get; set; }

        [ForeignKey("CustomerClubId")]
        public int CustomerClubId { get; set; }

        public virtual Club CustomerClub { get; set; }

        [ForeignKey("PlayerTransferMarketId")]
        public int PlayerTransferMarketId { get; set; }

        public virtual PlayerTransferMarket PlayerTransferMarket { get; set; }

        [ForeignKey("SellerClubId")]
        public int SellerClubId { get; set; }

        public virtual Club SellerClub { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual ClubAdministrator CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual ClubAdministrator ModifiedBy { get; set; }

        public string Approved_Declined_ById { get; set; }

        [ForeignKey("Approved_Declined_ById")]
        public virtual ClubAdministrator Approved_Declined_By { get; set; }

        public TransferStatus Status { get; set; }

        public PaymentTransferStatus paymentTransferStatus { get; set; }
    }

    public enum TransferStatus
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled,
        Completed,
    }

    public enum PaymentTransferStatus
    {
        [Display(Name="Pending payment")]
        Pending_Payment,

        [Display(Name = "Payment successful")]
        Payment_Completed,

        [Display(Name = "Declined")]
        Declined,

        [Display(Name = "Cancelled")]
        Cancelled,
    }
}
