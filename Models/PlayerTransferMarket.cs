using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class PlayerTransferMarket
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerTransferMarketId { get; set; }

        [ForeignKey("PlayerId")]
        public string PlayerId { get; set; }

        public virtual Player Player { get; set; }

        public int LeagueId { get; set; }

        public virtual League League { get; set; }  

        public int ClubId { get; set; }

        public virtual Club Club { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual SportsMember CreatedBy { get; set; }

        public decimal SellingPrice { get; set; }

        public SaleStatus SaleStatus { get; set; }

        public virtual ICollection<Transfer> Transfers { get; set; }

        public bool IsArchived { get; set; }

        public int? DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        public virtual Division Division { get; set; }
    }

    public enum SaleStatus
    {
        Available,
        Unavailable,
    }
}
