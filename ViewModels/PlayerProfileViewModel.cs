
using MyField.Models;

namespace MyField.ViewModels
{
    public class PlayerProfileViewModel
    {
        public string UserId { get; set; }
        public string Names { get; set; }

        public string LastName {get; set; }

        public string Phone {  get; set; }

        public string Email { get; set; }

        public int JerseyNumber { get; set; }

        public  Position Position { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string ProfilePicture { get; set; }
    }
}
