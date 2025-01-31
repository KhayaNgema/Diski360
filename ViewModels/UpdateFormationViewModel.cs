namespace MyField.ViewModels
{
    public class UpdateFormationViewModel
    {
        public int FormationId { get; set; }

        public string FormationName { get; set; }

        public string FormationImage { get; set; }

        public IFormFile FormationImages { get; set; }
    }
}
