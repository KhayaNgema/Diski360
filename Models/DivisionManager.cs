using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class DivisionManager : UserBaseModel
    {
        public int DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }
}
