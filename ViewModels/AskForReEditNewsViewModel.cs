namespace MyField.ViewModels
{
    public class AskForReEditNewsViewModel
    {
        public int? NewsId { get; set; }

        public string NewsTitle { get; set; }


        public string NewsImage { get; set; }

        public IFormFile NewsImages { get; set; }

        public string NewsBody { get; set; }

        public string ReasonForReEdit {  get; set; }
    }
}
