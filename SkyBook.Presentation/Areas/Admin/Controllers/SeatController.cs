using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SeatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
