using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class ActivityLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActivityLogId { get; set; }
        public string Activity { get; set; }


        [ForeignKey("UserId")]
        public string UserId { get; set; }

        public virtual UserBaseModel UserBaseModel { get; set; }

        public DateTime Timestamp { get; set; }

        public int DeviceInfoId { get; set; }

        public virtual DeviceInfo DeviceInfo { get; set; }  
    }
}
