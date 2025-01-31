using Microsoft.AspNetCore.Mvc;

namespace MyField.Controllers
{
    public class QueriesController : Controller
    {
        public IActionResult SupportQueries()
        {
            return View();
        }
    }
}
