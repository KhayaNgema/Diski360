using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Invoice
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }   

        public int? PaymentId { get; set; }

        public virtual Payment Payment { get; set; }

        public int? FineId { get; set; }

        public virtual Fine Fine { get; set; }  

        public int? TransferId { get; set; } 

        public virtual Transfer Transfer { get; set; }

        public DateTime InvoiceTimeStamp { get; set; }   

        public string CreatedById { get; set; } 

        public virtual UserBaseModel CreatedBy { get; set; }   
        
        public bool IsEmailed { get; set; } 
    }
}
