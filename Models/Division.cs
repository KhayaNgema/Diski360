using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Division
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DivisionId { get; set; }

        public string? DivisionBadge { get; set; }

        public string DivisionName { get; set; }    

        public string DivisionAbbr { get; set; }

        public string DivisionDescription { get; set;}

        public string Address { get; set; }

        public bool HasPaid { get; set; }

        public bool IsActive { get; set; }

        public bool IsSuspended { get; set; }


        public DateTime CreatedDateTime { get; set; }

        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public DivisionType DivisionType { get; set; }

        public DivisionStatus Status { get; set; }

        public string? SignedContract {  get; set; }

        public Division()
        {
            DivisionBadge = "Images/division_logo.jpeg";
        }
    }

    public enum DivisionType
    {
        Soccer,
        Netball
    }

    public enum DivisionStatus
    {
        Active, 
        Inactive, 
        Suspended,
        Deleted
    }
}
