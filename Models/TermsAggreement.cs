using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class TermsAggreement
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TermsAggreementId { get; set; }

        public string Id { get; set; }

        public virtual UserBaseModel UserBaseModel { get; set; }    

        public bool IsAggreed { get; set; }

        public DateTime AggreementTimeStamp { get; set; }
    }
}
