using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AircraftController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
