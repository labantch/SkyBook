using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult SelectSeats() => View();
        public IActionResult ConfirmPayment() => View();
        public IActionResult MyBookings() => View();
    }
}
