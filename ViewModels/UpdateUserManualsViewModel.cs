namespace MyField.ViewModels
{
    public class UpdateUserManualsViewModel
    {
        public int ManualId { get; set; }

        public string UserRole { get; set; }

        public string ManualDocument { get; set; }

        public IFormFile ManualDocuments { get; set; }
    }
}
