using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Controllers
{
    public class PassengerController : Controller
    {
        public IActionResult Details() => View();
    }
}
