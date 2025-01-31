using MyField.Models;

namespace MyField.ViewModels
{
    public class NetUsersViewModel
    {
        public List<UserBaseModel> Users { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
