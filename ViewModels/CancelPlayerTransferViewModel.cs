namespace MyField.ViewModels
{
    public class CancelPlayerTransferViewModel
    {
        public int TransferId { get; set; }
        public string PlayerId { get; set; }

        public int CustomerClubId { get; set; }

        public int SellerClubId { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        public string ModifiedById { get; set; }

        public decimal TransferAmount { get; set; }
        public string CancelReason { get; set; }
    }
}
