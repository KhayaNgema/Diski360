using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MyField.Models
{
    public class SportNews
    {
        [Display(Name = "Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NewsId { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "News title is required")]
        public string NewsHeading { get; set; }

        [Display(Name = "Date")]
        [Required(ErrorMessage = "Publish time is required")]
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDateTime { get; set; } = DateTime.Now;

        [Display(Name = "Modified By")]
        public string ModifiedById { get; set; }
        [ForeignKey("ModifiedById")]
        public virtual SportsMember ModifiedBy { get; set; }

        [Display(Name = "Authored By")]
        public string AuthoredById { get; set; }
        [ForeignKey("AuthoredById")]
        public virtual SportsMember AuthoredBy { get; set; }

        [Display(Name = "Published By")]
        public string PublishedById { get; set; }
        [ForeignKey("PublishedById")]
        public virtual SportsMember PublishedBy { get; set; }

        [Display(Name = "Rejected By")]
        public string RejectedById { get; set; }
        [ForeignKey("RejectedById")]
        public virtual SportsMember RejectedBy { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public DateTime RejectedDateTime { get; set; } = DateTime.Now;

        [Display(Name = "Body")]
        [Required(ErrorMessage = "News body is required")]
        public string NewsBody { get; set; }

        [Display(Name = "Image")]
        public string NewsImage { get; set; }

        [Display(Name = "Status")]
        public NewsStatus NewsStatus { get; set; } 
        

        public string? ReasonForReEdit { get; set; }

        public SportNews()
        {

        }
    }

    public enum NewsStatus
    { 
        Awaiting_Approval,
        Approved,
        Rejected,
        ToBeModified
    }
}
