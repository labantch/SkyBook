using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Controllers
{
    public class FlightController : Controller
    {
        public IActionResult Results() => View();
    }
}
