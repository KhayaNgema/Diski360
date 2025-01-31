using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class UserManuals
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ManualId { get; set; } 

        public string UserRole { get; set; }

        public string ManualDocument {  get; set; }
    }
}
