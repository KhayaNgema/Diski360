using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }

        public string AnnouncementText { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual UserBaseModel CreatedBy { get; set; }

        public string ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual UserBaseModel ModifiedBy { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }
}
