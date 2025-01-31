using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class DeviceInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceInfoId { get; set; }

        public string? IpAddress { get; set; }
        public string? DeviceName { get; set; }
        public string? Browser { get; set; }
        public string? DeviceModel { get; set; }

        public string? OSName { get; set; }
        public string? OSVersion { get; set; }

        public string? BrowserVersion { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
