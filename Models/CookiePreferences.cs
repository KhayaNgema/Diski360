using System.ComponentModel.DataAnnotations;

namespace MyField.Models
{
    public class CookiePreferences
    {
        [Key]
        public string UserId { get; set; } 
        public bool PerformanceCookies { get; set; }
        public bool FunctionalityCookies { get; set; }
        public bool TargetingCookies { get; set; }
    }
}
