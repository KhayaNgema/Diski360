using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Comment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }  

        public string CommentText { get; set; }

        [ForeignKey("CommenterId")]
        public string CommenterId { get; set; } 

        public virtual UserBaseModel CommentBy { get; set; } 

        public DateTime CommentDateTime { get; set; }   

    }
}
