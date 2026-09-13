using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
