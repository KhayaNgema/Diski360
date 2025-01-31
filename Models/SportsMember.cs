using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class SportsMember : UserBaseModel
    {
        public int? DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        public virtual Division Division { get; set; }
    }
}
