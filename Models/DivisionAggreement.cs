using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class DivisionAggreement
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AggreementId { get; set; }

        public int DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public string SignedContract { get; set; }


        [DataType(DataType.Date)]
        public DateTime AgreementStartDate { get; set; }


        [DataType(DataType.Date)]
        public DateTime AgreementEndDate { get; set; }


        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }
    }
}
