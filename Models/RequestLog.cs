using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class RequestLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        public string RequestDescription { get; set; }

        public RequestType RequestType { get; set; }

        public DateTime TimeStamp { get; set; }

        public int ResponseCode { get; set; }
    }

    public enum RequestType
    {
        Succeeded,
        Failed
    }
}
